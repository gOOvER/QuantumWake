using Quantumwake.Core.State;
using Quantumwake.Data;

namespace Quantumwake.Tests;

/// <summary>
/// Every open hauling contract on one route, from the logs and from
/// whatever Contracts-app frames have been taken.
/// </summary>
public class HaulPlannerTests
{
    private static readonly DateTimeOffset T0 = new(2026, 9, 9, 1, 0, 0, TimeSpan.Zero);

    private static ContractRecord Contract(string mission, string raw, string? title, int minutes = 0,
        ContractOutcome outcome = ContractOutcome.InProgress) =>
        new(T0.AddMinutes(minutes), raw, ContractNameParser.Parse(raw).DisplayName, "Red Wind", "Pyro", null, "Cargo Hauling", false)
        {
            MissionId = mission,
            Title = title,
            Outcome = outcome,
        };

    private const string Multi3Aluminum = "RedWind_Pyro_SmallGrade_Solar_CFP_TradepostToStation_Aluminum_CargoHauling_Multi3ToSingle";
    private const string Multi2Copper = "RedWind_Pyro_SmallGrade_Solar_CFP_TradepostToStation_Copper_CargoHauling_Multi2ToSingle";
    private const string Multi3Copper = "RedWind_Pyro_SmallGrade_Solar_CFP_TradepostToStation_Copper_CargoHauling_Multi3ToSingle";
    private const string DirectCarbon = "RedWind_Pyro_SupplyGrade_RegionA_CFP_StationToTradepost_Carbon_CargoHauling_AtoB_Intro";
    private const string UnnamedCargo = "HaulCargo_Multi2ToSingle_Interstellar_Bulk_DistSp_Dia_FresFoo_Gol_Aphor";

    private static ScreenSighting Frame(string shot, int minutes, ScreenTextLine[] lines)
    {
        var frame = ScreenFrames.Read(lines, [], []);
        return new ScreenSighting(shot, T0.AddMinutes(minutes), frame.Kind, "", [], null, null, null, null, [], 0, Contracts: frame.Contracts);
    }

    private static ScreenSighting EmptyCard(string shot, int minutes, string title) =>
        new(shot, T0.AddMinutes(minutes), ScreenKind.Contracts, "", [], null, null, null, null, [], 0,
            Contracts: new ContractsReading(null, null, [], title, null, null, []));

    private static ScreenSighting Card(string shot, int minutes, string title, params ContractStep[] steps) =>
        new(shot, T0.AddMinutes(minutes), ScreenKind.Contracts, "", [], null, null, null, null, [], 0,
            Contracts: new ContractsReading(null, null, [], title, null, null, [], steps));

    /// <summary>An atlas that knows the Pyro stations and nothing on the ground.</summary>
    private static ResolvedPlace? Atlas(string name) => name switch
    {
        "Stanton Gateway" => new("Pyro_StantonGateway", "Stanton Gateway", "Pyro Jump", "Pyro"),
        "Ruin Station" => new("Pyro_RuinStation", "Ruin Station", "Pyro VI", "Pyro"),
        "Checkmate" => new("Pyro_Checkmate", "Checkmate", "Pyro IV", "Pyro"),
        _ => null,
    };

    [Fact]
    public void A_photographed_card_gives_its_legs_and_the_rest_say_what_is_missing()
    {
        var plan = HaulPlanner.Plan(
            [
                Contract("m1", Multi3Aluminum, "Junior | Stellar Small Haul | to Stanton Gateway <EM4>[50 Rep]</EM4>"),
                Contract("m2", Multi2Copper, "Junior | Stellar Small Haul | to Ruin Station <EM4>[50 Rep]</EM4>", 1),
                Contract("m3", DirectCarbon, "Junior Rank - Direct Small Cargo Haul", 2),
            ],
            [Frame("ScreenShot-2026-09-08_21-48-31-84A.jpg", 48, ScreenAppFixtures.Contracts)],
            Atlas);

        Assert.Equal(3, plan.Contracts.Count);

        var read = plan.Contracts[0];
        Assert.Equal("screenshot", read.Source);
        Assert.Equal("ScreenShot-2026-09-08_21-48-31-84A.jpg", read.Shot);
        Assert.Equal(3, read.Legs.Count);
        Assert.Equal(18, read.Scu);
        Assert.Null(read.Note);

        var titled = plan.Contracts[1];
        Assert.Equal("title", titled.Source);
        var leg = Assert.Single(titled.Legs);
        Assert.Equal(("Ruin Station", null), (leg.Delivery, leg.Pickup));
        Assert.Equal("2 pickups, places unknown until this card is photographed", titled.Note);

        var blind = plan.Contracts[2];
        Assert.Equal("none", blind.Source);
        Assert.Empty(blind.Legs);
        Assert.Contains("only a screenshot", blind.Note);

        Assert.Equal(18, plan.KnownScu);
        Assert.Contains(plan.Notes, n => n.StartsWith("2 of 3 contracts have no screenshot"));
        Assert.Contains(plan.Notes, n => n.Contains("floor"));
    }

    [Fact]
    public void The_route_collects_everything_before_it_delivers_and_groups_a_body()
    {
        var plan = HaulPlanner.Plan(
            [
                Contract("m1", Multi3Aluminum, "Junior | Stellar Small Haul | to Stanton Gateway"),
                Contract("m2", Multi2Copper, "Junior | Stellar Small Haul | to Ruin Station", 1),
            ],
            [Frame("a.jpg", 48, ScreenAppFixtures.Contracts)],
            Atlas);

        var places = plan.Stops.Select(s => s.Place).ToList();

        // Three sources, then the two destinations; the sources are on three
        // bodies so their order is by what there is to do, and both deliveries
        // come after every pickup.
        Assert.Equal(5, plan.Stops.Count);
        Assert.Equal(["Ashland", "Fallow Field", "The Golden Riviera"], places.Take(3).Order());
        Assert.Contains("Stanton Gateway", places.Skip(3));
        Assert.Contains("Ruin Station", places.Skip(3));

        var gateway = plan.Stops.Single(s => s.Place == "Stanton Gateway");
        var unload = Assert.Single(gateway.Actions);
        Assert.Equal(("unload", "Aluminum", 18), (unload.Kind, unload.Commodity, unload.Scu));
        Assert.Equal("Pyro_StantonGateway", gateway.PlaceId);

        var fallow = plan.Stops.Single(s => s.Place == "Fallow Field");
        Assert.Equal("Pyro IV", fallow.Body);
        Assert.Equal("", fallow.PlaceId);
        Assert.Contains(plan.Notes, n => n.StartsWith("Not on the map yet: Fallow Field, Ashland, The Golden Riviera"));
    }

    /// <summary>Two cards with one title: the cargo tells which frame is whose, and one frame is not spent twice.</summary>
    [Fact]
    public void A_frame_goes_to_the_contract_whose_cargo_it_shows()
    {
        var copperCard = ScreenAppFixtures.Contracts
            .Select(l => new ScreenTextLine(l.Text.Replace("Aluminum", "Copper"), l.Left, l.Top, l.Height))
            .Where(l => !l.Text.Contains("Golden Riviera"))
            .ToArray();

        var plan = HaulPlanner.Plan(
            [
                Contract("m1", Multi2Copper, "Junior | Stellar Small Haul | to Stanton Gateway"),
                Contract("m2", Multi3Aluminum, "Junior | Stellar Small Haul | to Stanton Gateway", 1),
                Contract("m3", Multi3Aluminum, "Junior | Stellar Small Haul | to Stanton Gateway", 2),
            ],
            [
                Frame("copper.jpg", 40, copperCard),
                Frame("aluminum.jpg", 48, ScreenAppFixtures.Contracts),
            ],
            Atlas);

        Assert.Equal("copper.jpg", plan.Contracts[0].Shot);
        Assert.Equal("aluminum.jpg", plan.Contracts[1].Shot);

        // The third card has no frame of its own. The aluminium frame fits it
        // as well as it fits the second, so it is reused - and flagged.
        Assert.Equal("aluminum.jpg", plan.Contracts[2].Shot);
        Assert.Contains("also matched another card", plan.Contracts[2].Note);
        Assert.Null(plan.Contracts[1].Note);
    }

    [Fact]
    public void A_same_title_card_with_the_wrong_cargo_is_not_used()
    {
        var plan = HaulPlanner.Plan(
            [
                Contract("m1", Multi2Copper, "Junior | Stellar Small Haul | to Stanton Gateway"),
                Contract("m2", Multi3Aluminum, "Junior | Stellar Small Haul | to Stanton Gateway", 1),
            ],
            [Frame("aluminum.jpg", 48, ScreenAppFixtures.Contracts)],
            Atlas);

        // The aluminium card cannot stand in for the copper contract just
        // because the destination title repeats.
        Assert.Equal("title", plan.Contracts[0].Source);
        Assert.Null(plan.Contracts[0].Shot);
        Assert.Equal("screenshot", plan.Contracts[1].Source);
        Assert.Equal("aluminum.jpg", plan.Contracts[1].Shot);
    }

    [Fact]
    public void A_same_title_card_with_the_wrong_pickup_count_is_not_used()
    {
        var copperCard = ScreenAppFixtures.Contracts
            .Select(line => new ScreenTextLine(line.Text.Replace("Aluminum", "Copper"), line.Left, line.Top, line.Height))
            .ToArray();

        var plan = HaulPlanner.Plan(
            [
                Contract("m1", Multi2Copper, "Junior | Stellar Small Haul | to Stanton Gateway"),
                Contract("m2", Multi3Copper, "Junior | Stellar Small Haul | to Stanton Gateway", 1),
            ],
            [Frame("three-pickups.jpg", 48, copperCard)],
            Atlas);

        Assert.Equal("title", plan.Contracts[0].Source);
        Assert.Equal("screenshot", plan.Contracts[1].Source);
        Assert.Equal("three-pickups.jpg", plan.Contracts[1].Shot);
    }

    [Fact]
    public void A_selected_card_without_objectives_falls_back_to_its_title()
    {
        var plan = HaulPlanner.Plan(
            [Contract("m1", Multi3Aluminum, "Junior | Stellar Small Haul | to Stanton Gateway")],
            [EmptyCard("unreadable.jpg", 48, "Junior | Stellar Small Haul | to Stanton Gateway")],
            Atlas);

        var contract = Assert.Single(plan.Contracts);
        Assert.Equal("title", contract.Source);
        Assert.Null(contract.Shot);
        Assert.Equal("Stanton Gateway", Assert.Single(contract.Legs).Delivery);
        Assert.Contains(plan.Stops, stop => stop.Place == "Stanton Gateway");
    }

    [Fact]
    public void Separate_cargo_at_one_destination_keeps_its_own_remaining_unload_and_manifest()
    {
        const string title = "Rookie | Solar Cargo | to Seraphim Station";
        var plan = HaulPlanner.Plan(
            [Contract("m1", UnnamedCargo, title)],
            [Card("multi-cargo.jpg", 48, title,
                new("Collect Aluminum from Alpha.", "collect", "Aluminum", "Alpha", null, null, null, 0),
                new("Deliver 0/4 SCU of Aluminum to Seraphim Station.", "deliver", "Aluminum", "Seraphim Station", null, 0, 4, 1),
                new("Collect Copper from Beta.", "collect", "Copper", "Beta", null, null, null, 0),
                new("Deliver 2/6 SCU of Copper to Seraphim Station.", "deliver", "Copper", "Seraphim Station", null, 2, 6, 1))],
            Atlas);

        var contract = Assert.Single(plan.Contracts);
        Assert.Equal((10, 2, 8), (contract.Scu, contract.ScuDone, contract.RemainingScu));
        Assert.Equal(8, plan.KnownScu);

        var destination = plan.Stops.Single(stop => stop.Place == "Seraphim Station");
        Assert.Equal(2, destination.Actions.Count);
        Assert.Contains(destination.Actions, action => action is { Kind: "unload", Commodity: "Aluminum", Scu: 4, ScuDone: 0 });
        Assert.Contains(destination.Actions, action => action is { Kind: "unload", Commodity: "Copper", Scu: 4, ScuDone: 2 });
        Assert.All(destination.Actions, action =>
        {
            Assert.Equal("m1", action.MissionId);
            Assert.NotEmpty(action.LegIds!);
        });
        Assert.Empty(destination.Aboard!);

        var afterSecondPickup = plan.Stops.Single(stop => stop.Place == "Beta").Aboard!;
        Assert.Contains(afterSecondPickup, cargo => cargo is { Commodity: "Aluminum", KnownScu: 4, AmountUnknown: false });
        Assert.Contains(afterSecondPickup, cargo => cargo is { Commodity: "Copper", KnownScu: 4, AmountUnknown: false });
    }

    [Fact]
    public void A_frame_from_before_the_contract_was_taken_does_not_count()
    {
        var plan = HaulPlanner.Plan(
            [Contract("m1", Multi3Aluminum, "Junior | Stellar Small Haul | to Stanton Gateway", 60)],
            [Frame("a.jpg", 48, ScreenAppFixtures.Contracts)],
            Atlas);

        Assert.Equal("title", plan.Contracts[0].Source);
    }

    /// <summary>A place that is one contract's source and another's destination is visited for the pickup first, then again to deliver.</summary>
    [Fact]
    public void A_place_that_is_both_ends_gets_a_second_visit_for_the_delivery()
    {
        var plan = HaulPlanner.Plan(
            [
                Contract("m1", DirectCarbon, "Rookie | DIRECT Small Haul | Checkmate > Ruin Station"),
                Contract("m2", DirectCarbon, "Rookie | DIRECT Small Haul | Ruin Station > Checkmate", 1),
            ],
            [],
            Atlas);

        var places = plan.Stops.Select(s => s.Place).ToList();

        Assert.Equal(3, plan.Stops.Count);
        Assert.Equal(places[0], places[2]);
        Assert.NotNull(plan.Stops[0].Note);
        Assert.Single(plan.Stops[0].Actions, a => a.Kind == "load");
        Assert.Single(plan.Stops[2].Actions, a => a.Kind == "unload");
    }

    [Fact]
    public void Closed_contracts_and_salvage_bounties_are_not_on_the_run()
    {
        var plan = HaulPlanner.Plan(
            [
                Contract("m1", Multi3Aluminum, "Junior | Stellar Small Haul | to Stanton Gateway", 0, ContractOutcome.Completed),
                Contract("m2", "Covalex_Stanton_Hard_RecoverCargo", "Large Covalex Shipment Needs Recovering", 1),
            ],
            [],
            Atlas);

        Assert.Empty(plan.Contracts);
        Assert.Empty(plan.Stops);
    }

    /// <summary>
    /// A contract whose pickups the plan cannot see still has to be loaded
    /// somewhere, so its delivery goes after every pickup the plan can see,
    /// and says why; two visits to one place in a row are one stop.
    /// </summary>
    [Fact]
    public void A_delivery_with_unseen_pickups_comes_after_every_known_pickup()
    {
        var plan = HaulPlanner.Plan(
            [
                Contract("m1", Multi2Copper, "Junior | Stellar Small Haul | to Ruin Station"),
                Contract("m2", DirectCarbon, "Rookie | DIRECT Small Haul | Ruin Station > Checkmate", 1),
            ],
            [],
            Atlas);

        Assert.Equal(["Ruin Station", "Checkmate"], plan.Stops.Select(s => s.Place));

        var ruin = plan.Stops[0];
        Assert.Equal(["load", "unload"], ruin.Actions.Select(a => a.Kind));
        Assert.Equal("after its pickups, which are not on this plan", ruin.Actions[1].Note);
        Assert.Null(ruin.Note);
    }
}
