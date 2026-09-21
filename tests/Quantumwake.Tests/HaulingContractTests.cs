using Quantumwake.Core.Events;
using Quantumwake.Core.State;

namespace Quantumwake.Tests;

/// <summary>
/// What the logs say about where a hauling contract goes, which is two
/// fragments: the title the acceptance toast showed, and the archetype id on
/// the objective marker. Every string below is one that occurs in this
/// install's backups.
/// </summary>
public class HaulingContractTests
{
    // ---- the title ----

    [Theory]
    [InlineData("Junior | Stellar Small Haul | to Stanton Gateway <EM4>[50/200/250/500/1000/2000/4000 Rep]</EM4>", null, "Stanton Gateway")]
    [InlineData("Rookie | Extra Small Haul | from Port Tressler <EM4>[BP]*</EM4>", "Port Tressler", null)]
    [InlineData("Rookie | <EM3>DIRECT</EM3> Extra Small Haul | Port Tressler > NB Int. Spaceport <EM4>[BP]*</EM4>", "Port Tressler", "NB Int. Spaceport")]
    [InlineData("Member | Stellar Medium Haul | to Ruin Station", null, "Ruin Station")]
    public void The_title_names_the_ends_it_names(string title, string? pickup, string? delivery)
    {
        var route = HaulingContract.RouteFromTitle(title);

        Assert.NotNull(route);
        Assert.Equal(pickup, route.Pickup);
        Assert.Equal(delivery, route.Delivery);
    }

    /// <summary>
    /// The vanilla titles say nothing about place, and the mod's half-filled
    /// template says "Destinationname" - 11 times on this install. Neither is
    /// a route.
    /// </summary>
    [Theory]
    [InlineData("Junior Rank - Direct Small Cargo Haul")]
    [InlineData("Rookie Hauler Needed for Large Shipment")]
    [InlineData("Junior | Stellar Small Haul | to Destinationname <EM4>[50 Rep]</EM4>")]
    [InlineData("")]
    [InlineData(null)]
    public void A_title_that_names_no_place_gives_no_route(string? title) =>
        Assert.Null(HaulingContract.RouteFromTitle(title));

    // ---- the archetype ----

    [Theory]
    [InlineData("RedWind_Pyro_SmallGrade_Solar_CFP_TradepostToStation_Aluminum_CargoHauling_Multi3ToSingle", HaulShape.MultiToSingle, 3, 1, "Aluminum")]
    [InlineData("HaulCargo_SingleToMulti2_RefinedOre_Aluminium_Stanton4_SmallGrade", HaulShape.SingleToMulti, 1, 2, "Aluminium")]
    [InlineData("HaulCargo_AToB_Processed_AgriculturalSupplies_Stanton4_SmallGrade", HaulShape.Direct, 1, 1, "Agricultural Supplies")]
    [InlineData("HaulCargo_AToB_Waste_Mixed_ScrapWaste_Stanton4_SupplyGrade1", HaulShape.Direct, 1, 1, "Mixed: Scrap Waste")]
    [InlineData("RedWind_Pyro_SupplyGrade_RegionA_CFP_StationToTradepost_Carbon_CargoHauling_AtoB_Intro", HaulShape.Direct, 1, 1, "Carbon")]
    [InlineData("HaulCargo_MultiToSingle_Interstellar_Small_Souv_DistSpri_ProFoo_Stim_Intro", HaulShape.MultiToSingle, null, 1, null)]
    [InlineData("HaulCargo_SingleToMulti_Interstellar_Bulk_ShipAmm_Hydro_Med", HaulShape.SingleToMulti, 1, null, null)]
    [InlineData("HaulCargo_AToB_Stanton4_Intro", HaulShape.Direct, 1, 1, null)]
    public void The_archetype_spells_the_shape_and_the_cargo(
        string raw, HaulShape shape, int? pickups, int? deliveries, string? commodity)
    {
        var read = HaulingContract.FromArchetype(raw);

        Assert.NotNull(read);
        Assert.Equal(shape, read.Shape);
        Assert.Equal(pickups, read.Pickups);
        Assert.Equal(deliveries, read.Deliveries);
        Assert.Equal(commodity, read.Commodity);
    }

    /// <summary>The salvage bounties carry "Cargo" in their name and are not hauling.</summary>
    [Theory]
    [InlineData("Covalex_Stanton_VeryHard_RecoverCargo")]
    [InlineData("Ling_Stanton_VeryEasy_RecoverCargo_2")]
    [InlineData("GillysPilotSchool_Mission06_2")]
    public void Recovering_cargo_is_not_hauling_it(string raw)
    {
        Assert.False(HaulingContract.IsHauling(raw));
        Assert.Null(HaulingContract.FromArchetype(raw));
    }

    // ---- the join in the builder ----

    private static readonly DateTimeOffset T0 = new(2026, 9, 10, 1, 10, 18, TimeSpan.Zero);
    private const string Mission = "19fac03f-cab1-4bca-8f33-d1709877af59";
    private const string Raw = "RedWind_Pyro_SmallGrade_Solar_CFP_TradepostToStation_Aluminum_CargoHauling_Multi3ToSingle";
    private const string Toast = "Contract Accepted:  Junior | Stellar Small Haul | to Checkmate <EM4>[50/200/250/500/1000/2000/4000 Rep]</EM4>: ";

    private static SessionSummary Built(params GameEvent[] events)
    {
        var builder = new SessionBuilder("Game Build(12572603) 09 Sep 26 (21 04 45).log");
        foreach (var e in events) builder.Add(e);
        return builder.Build();
    }

    private static ContractEvent Marker(DateTimeOffset at) => new(at, Mission, null, Raw, null);

    private static NotificationEvent Accepted(DateTimeOffset at) => new(at, Toast.Trim(), "41", Mission);

    /// <summary>
    /// The marker and the toast are 20 ms apart on this install and arrive in
    /// either order; the title lands on the contract whichever came first.
    /// </summary>
    [Fact]
    public void The_toast_title_lands_on_the_contract_in_either_order()
    {
        foreach (var session in new[]
        {
            Built(Marker(T0), Accepted(T0.AddMilliseconds(23))),
            Built(Accepted(T0), Marker(T0.AddMilliseconds(23))),
        })
        {
            var contract = Assert.Single(session.Contracts);
            Assert.Equal("Junior | Stellar Small Haul | to Checkmate <EM4>[50/200/250/500/1000/2000/4000 Rep]</EM4>", contract.Title);
            Assert.Equal(contract.Title, contract.Name);
            Assert.Equal("Checkmate", HaulingContract.RouteFromTitle(contract.Title)!.Delivery);
        }
    }

    /// <summary>A marker with no toast - a contract carried over - keeps the composed name.</summary>
    [Fact]
    public void Without_a_toast_the_composed_name_stands()
    {
        var contract = Assert.Single(Built(Marker(T0)).Contracts);

        Assert.Null(contract.Title);
        Assert.Equal(contract.DisplayName, contract.Name);
    }

    /// <summary>
    /// The objective ids name their end. Two pickups done of three and the
    /// drop-off still open is what a half-flown Multi3ToSingle looks like.
    /// </summary>
    [Fact]
    public void Steps_are_counted_by_the_end_they_belong_to()
    {
        MissionObjectiveEvent Step(int s, string id, ObjectiveState state) =>
            new(T0.AddSeconds(s), Mission, id, state, ShownInLog: true);

        var contract = Assert.Single(Built(
            Marker(T0),
            Step(10, "pickup_c484ba67-475b-429d-84fa-e628047cc226_0", ObjectiveState.Completed),
            Step(20, "pickup_c484ba67-475b-429d-84fa-e628047cc226_1", ObjectiveState.Completed),
            Step(30, "pickup_c484ba67-475b-429d-84fa-e628047cc226_2", ObjectiveState.InProgress),
            Step(40, "dropoff_c484ba67-475b-429d-84fa-e628047cc226_0", ObjectiveState.InProgress),
            Step(50, "phase_0", ObjectiveState.InProgress)).Contracts);

        Assert.Equal(3, contract.Pickups);
        Assert.Equal(2, contract.PickupsDone);
        Assert.Equal(1, contract.Deliveries);
        Assert.Equal(0, contract.DeliveriesDone);
        Assert.Equal(5, contract.Steps);
        Assert.Equal(2, contract.StepsDone);
    }
}
