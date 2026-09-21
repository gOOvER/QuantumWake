namespace Quantumwake.WebTests;

/// <summary>
/// The hauling run on Shopping, and the Now page leading with where a haul
/// is going.
/// </summary>
/// <remarks>
/// The plan is the server's; what is tested here is that the page says
/// what the plan could not see - a card unread, a pickup unknown, a place
/// not on the map - next to the thing it could not see, and that making
/// it a flight plan posts to the right place and reports back.
/// </remarks>
public class HaulPlanTests
{
    private const string Now = """
        {"connected":true,"inGame":true,"sessionStarted":"2026-09-09T01:00:00Z","ship":"RSI Hermes",
         "contracts":[]}
        """;

    private const string Contracts = """
        [
          {"at":"2026-09-09T01:10:00Z","name":"Junior | Stellar Small Haul | to Stanton Gateway","issuer":"Red Wind","type":"Cargo Hauling",
           "system":"Pyro","difficulty":null,"outcome":"InProgress","steps":4,"stepsDone":1,"hauling":true,"delivery":"Stanton Gateway"},
          {"at":"2026-09-09T01:12:00Z","name":"Large Covalex Shipment Needs Recovering","issuer":"Covalex","type":"Recover Cargo",
           "system":"Stanton","difficulty":"Hard","outcome":"InProgress","steps":0,"stepsDone":0,"hauling":false}
        ]
        """;

    private const string Plan = """
        {"inGame":true,"ship":"RSI Hermes","shipScu":480,
         "contracts":[
           {"title":"Junior | Stellar Small Haul | to Stanton Gateway","missionId":"m1","commodity":"Aluminum","shape":"MultiToSingle",
            "legs":[
              {"pickup":"Fallow Field","pickupBody":"Pyro IV","delivery":"Stanton Gateway","deliveryBody":null,"commodity":"Aluminum","scu":null,"scuDone":null},
              {"pickup":"Ashland","pickupBody":"Pyro 5a","delivery":"Stanton Gateway","deliveryBody":null,"commodity":"Aluminum","scu":null,"scuDone":null}
            ],
            "source":"screenshot","shot":"ScreenShot-2026-09-08_21-48-31-84A.jpg","shotAt":"2026-09-09T01:48:31Z",
            "pickups":3,"pickupsDone":1,"deliveries":1,"deliveriesDone":0,"scu":18,"note":null},
           {"title":"Junior | Stellar Small Haul | to Ruin Station","missionId":"m2","commodity":"Copper","shape":"MultiToSingle",
            "legs":[{"pickup":null,"pickupBody":null,"delivery":"Ruin Station","deliveryBody":null,"commodity":"Copper","scu":null,"scuDone":null}],
            "source":"title","shot":null,"shotAt":null,"pickups":0,"pickupsDone":0,"deliveries":0,"deliveriesDone":0,"scu":null,
            "note":"2 pickups, places unknown until this card is photographed"}
         ],
         "stops":[
           {"place":"Fallow Field","placeId":"","body":"Pyro IV","system":null,
            "actions":[{"kind":"load","commodity":"Aluminum","scu":null,"contract":0,"contractTitle":"Junior | Stellar Small Haul | to Stanton Gateway"}],"note":null,
            "aboard":[{"commodity":"Aluminum","knownScu":0,"amountUnknown":true}]},
           {"place":"Ashland","placeId":"","body":"Pyro 5a","system":null,
            "actions":[{"kind":"load","commodity":"Aluminum","scu":null,"contract":0,"contractTitle":"Junior | Stellar Small Haul | to Stanton Gateway"}],"note":null},
           {"place":"Stanton Gateway","placeId":"Pyro_StantonGateway","body":"Pyro Jump","system":"Pyro",
            "actions":[{"kind":"unload","commodity":"Aluminum","scu":18,"contract":0,"contractTitle":"Junior | Stellar Small Haul | to Stanton Gateway"}],"note":null},
           {"place":"Ruin Station","placeId":"Pyro_RuinStation","body":"Pyro VI","system":"Pyro",
            "actions":[{"kind":"unload","commodity":"Copper","scu":null,"contract":1,"contractTitle":"Junior | Stellar Small Haul | to Ruin Station"}],"note":null}
         ],
         "knownScu":18,
         "notes":["1 of 2 contracts has no screenshot of its card: open each on the Contracts app's Accepted tab and take one, and its pickups and SCU will be read.",
                  "The SCU total is a floor: it counts only contracts whose card was read.",
                  "Not on the map yet: Fallow Field, Ashland. A place is on the map once the logs have seen you there; the stop keeps its name either way."]}
        """;

    private static Page Loaded(string plan = Plan)
    {
        var page = new Page();
        page.Serve("/api/now", Now);
        page.Serve("/api/contracts?days=2", Contracts);
        page.Serve("/api/haul/plan", plan);
        page.Serve("/api/trips", "[]");
        page.Do("await loadJobContracts();");
        return page;
    }

    private const string Host = "__dom.node('#jobs-contracts')";

    [Fact]
    public void Each_haul_shows_its_legs_and_where_they_came_from()
    {
        var page = Loaded();

        Assert.Equal(2, page.Count($"{Host}.byClass('haul-contract').length"));

        var read = page.Text($"{Host}.byClass('haul-contract')[0].textContent");
        Assert.Contains("card read", read);
        Assert.Contains("Fallow Field (Pyro IV) → Stanton Gateway", read);
        Assert.Contains("1 of 3 pickups done", read);
        Assert.Contains("0 of 1 drop-off done", read);
        Assert.Contains("read from ScreenShot-2026-09-08_21-48-31-84A.jpg", read);

        var titled = page.Text($"{Host}.byClass('haul-contract')[1].textContent");
        Assert.Contains("title only", titled);
        Assert.Contains("pickup not known → Ruin Station", titled);
        Assert.Contains("2 pickups, places unknown until this card is photographed", titled);
    }

    [Fact]
    public void The_stops_say_what_to_do_and_which_are_not_on_the_map()
    {
        var page = Loaded();

        Assert.Equal(4, page.Count($"{Host}.byClass('haul-stops')[0].byClass('have').length"));

        var first = page.Text($"{Host}.byClass('haul-stops')[0].byClass('have')[0].textContent");
        Assert.Contains("Fallow Field", first);
        Assert.Contains("not on the map", first);
        Assert.Contains("load Aluminum — Junior | Stellar Small Haul | to Stanton Gateway", first);
        Assert.Contains("aboard after this stop: Aluminum amount unknown", first);

        var gateway = page.Text($"{Host}.byClass('haul-stops')[0].byClass('have')[2].textContent");
        Assert.Contains("unload 18 SCU Aluminum", gateway);
        Assert.DoesNotContain("not on the map", gateway);

        var summary = page.Text($"{Host}.byClass('haul-plan')[0].byClass('caption')[0].textContent");
        Assert.Contains("2 contracts", summary);
        Assert.Contains("1 with the card read", summary);
        Assert.Contains("18 SCU remaining known (a floor)", summary);
        Assert.Contains("RSI Hermes holds 480 SCU", summary);

        Assert.Contains("1 of 2 contracts has no screenshot", page.Text($"{Host}.textContent"));
        Assert.Contains("Load and unload actions stay manual", page.Text($"{Host}.textContent"));
    }

    /// <summary>The non-hauling contract keeps its plain card, named as the game names it.</summary>
    [Fact]
    public void Other_contracts_keep_their_own_card_under_the_run()
    {
        var page = Loaded();

        var plain = page.Text($"{Host}.byClass('job-card').filter(c => !c.className.includes('haul-contract'))[0].textContent");
        Assert.Contains("Large Covalex Shipment Needs Recovering", plain);
        Assert.Contains("Covalex · Recover Cargo · Stanton", plain);
    }

    [Fact]
    public void A_load_beyond_the_hold_is_said_in_so_many_words()
    {
        var page = Loaded(Plan.Replace("\"shipScu\":480", "\"shipScu\":12"));

        Assert.Contains("18 SCU is more than the RSI Hermes holds", page.Text($"{Host}.textContent"));
    }

    [Fact]
    public void Making_it_the_flight_plan_posts_once_and_reports_the_trip()
    {
        var page = Loaded();
        page.Serve("/api/haul/plan/trip", """{"id":"t9","title":"Hauling run · 2 contracts","stops":4}""");

        page.Do($"await {Host}.byClass('haul-make')[0].fire('click');");

        Assert.Contains("POST /api/haul/plan/trip", page.Fetched());
        Assert.Contains("Hauling run · 2 contracts — 4 stops, tracked", page.Text($"{Host}.byClass('haul-make')[0].textContent"));
        Assert.True(page.Truth($"{Host}.byClass('haul-make')[0].disabled"));
    }

    // ---- the Now page ----

    [Fact]
    public void One_haul_open_leads_with_where_it_goes_and_points_at_the_plan()
    {
        var page = new Page();
        page.Do("""
            renderNowFocus({ contracts: [
              { name: 'Junior | Stellar Small Haul | to Stanton Gateway', hauling: true, delivery: 'Stanton Gateway' } ] });
            """);

        Assert.Equal("Hauling", page.NodeText("#now-focus-title"));
        Assert.Equal("Deliver to Stanton Gateway", page.NodeText("#now-focus-detail"));
        Assert.Equal("Plan the run", page.NodeText("#now-focus-open"));
    }

    [Fact]
    public void Several_hauls_open_say_so_with_every_destination()
    {
        var page = new Page();
        page.Do("""
            renderNowFocus({ contracts: [
              { name: 'Junior | Stellar Small Haul | to Stanton Gateway', hauling: true, delivery: 'Stanton Gateway' },
              { name: 'Junior | Stellar Small Haul | to Ruin Station', hauling: true, delivery: 'Ruin Station' },
              { name: 'Junior Rank - Direct Small Cargo Haul', hauling: true, delivery: null } ] });
            """);

        Assert.Equal("Hauling run", page.NodeText("#now-focus-title"));
        Assert.Equal("3 hauls open — to Stanton Gateway, Ruin Station", page.NodeText("#now-focus-detail"));
    }

    [Fact]
    public void A_contract_that_is_not_a_haul_leads_as_before()
    {
        var page = new Page();
        page.Do("renderNowFocus({ contracts: [ { name: 'Large Covalex Shipment Needs Recovering', hauling: false } ] });");

        Assert.Equal("Active contract", page.NodeText("#now-focus-title"));
        Assert.Equal("Large Covalex Shipment Needs Recovering", page.NodeText("#now-focus-detail"));
        Assert.Equal("Contracts", page.NodeText("#now-focus-open"));
    }
}
