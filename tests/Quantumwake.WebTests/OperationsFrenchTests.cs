namespace Quantumwake.WebTests;

/// <summary>
/// The Operations pages in French: the command deck, Contracts, Checklists,
/// Blueprints, Wikelo and Events.
/// </summary>
/// <remarks>
/// What stays English is checked as carefully as what changes. Contract
/// titles, issuers, Wikelo's trade names and an event's bar names are the
/// game's text and are shown as the game wrote them; a list's title is the
/// pilot's. The difficulty column is not one of those - it is the parser's
/// own word for a fixed set, so it is page text.
/// </remarks>
public class OperationsFrenchTests
{
    private static Page Contracts()
    {
        var page = new Page("fr");
        page.Serve("/api/contracts?days=0", """
            [{"at":"2026-10-12T20:00:00Z","name":"Foxwell · Patrol · Hard","issuer":"Foxwell Enforcement","type":"Patrol",
              "difficulty":"Very Hard","system":"Stanton","outcome":"Abandoned","rep":0,"blueprint":false,"eventPoints":417,
              "event":"RSI Discovery Event","eventTracks":["Your total"],"steps":4,"stepsDone":1,"minutes":12}]
            """);
        page.Serve("/api/standing?days=0", """
            [{"issuer":"Iasi","contracts":37,"completed":23,"abandoned":3,"failed":10,"repFrom":0,"rep":0,
              "first":"2026-10-09T18:00:00Z","last":"2026-10-12T18:00:00Z"}]
            """);

        page.Do("""
            renderContracts({ contractsSeen: 38, contractsCompleted: 24, contractsAbandoned: 3, contractsFailed: 10,
                              contractIssuers: [], contractTypes: [] });
            await loadContractList(); await loadStanding();
            """);
        return page;
    }

    [Fact]
    public void Contracts_read_in_french_and_keep_the_games_names()
    {
        var page = Contracts();
        var table = page.NodeText("#contracts-table tbody");

        Assert.Contains("Échoués", page.NodeText("#contract-summary"));
        Assert.Contains("Taux de réussite", page.NodeText("#contract-summary"));
        Assert.Contains("abandonné", table);
        Assert.Contains("Très difficile", table);
        Assert.Contains("417 pts", table);
        Assert.Contains("Foxwell Enforcement", table);
        Assert.Contains("Patrol", table);
        Assert.Contains("échecs", page.NodeText("#standing-table tbody"));
        Assert.DoesNotContain("abandoned", table);
        Assert.DoesNotContain("failed", page.NodeText("#standing-table tbody"));
    }

    private const string Events = """
        {"available":true,"countedFrom":null,"tracked":null,
         "events":[
          {"id":"Iasi_ScenarioProgress","title":"RSI Discovery Event","description":"","percent":true,
           "firstSeen":"2026-10-09T18:00:00Z","lastSeen":"2026-10-09T19:00:00Z","completed":5,"unrecognised":1,
           "tracks":[
            {"id":"iasi_Journal_Defense","name":"Defense","overall":true,"color":null,"points":2085,
             "tiers":[{"minPoints":2000,"reward":"Scorpius Echo Livery","reached":true},
                      {"minPoints":4000,"reward":"Meteor PHB and TSB Flight Blades","reached":false}],
             "nextTier":4000,"toNext":1915,
             "fastest":[{"title":"RSI Disc. Month: Orange Lvl. - Neutralize Threats","points":417,"needed":5,"done":5}]}],
           "contracts":[{"id":"Iasi_Patrol_Hard","title":"RSI Disc. Month: Orange Lvl. - Neutralize Threats","issuer":"Foxwell Enforcement",
                         "points":417,"tracks":["Defense"],"completed":5}]}],
         "open":[]}
        """;

    [Fact]
    public void An_event_reads_in_french_with_its_bar_names_as_the_game_wrote_them()
    {
        var page = new Page("fr");
        page.Serve("/api/events", Events);
        page.Do("rememberEvent(null); await loadEvents();");
        var body = page.NodeText("#events-body");

        Assert.Contains("Prochain objectif d’événement", body);
        Assert.Contains("avant le palier 2 (Defense)", body);
        Assert.Contains("417 chacun, fait 5× déjà", body);
        Assert.Contains("2 paliers et leurs récompenses", body);
        Assert.Contains("1 contrat de vos journaux ressemble", body);
        Assert.Contains("Meteor PHB and TSB Flight Blades", body);
        Assert.DoesNotContain("to tier", body);
        Assert.Equal("1 dans ce patch", page.NodeText("#events-count"));
    }

    private const string Emporium = """
        {"available":true,
         "standings":[{"id":"001","name":"Very Good Customer","minReputation":340}],
         "trades":[
          {"id":"wolf","title":"Most Special Wolf","description":"","group":"vehicles","reputation":30,"minStanding":"001","retired":false,
           "rewards":[],"requirements":[{"name":"Wikelo Favor","count":5,"unit":"","have":true,"where":["Ruin Station"]}],
           "trackedJobId":null,"trackedDone":false},
          {"id":"old","title":"Retired","description":"","group":"items","reputation":0,"minStanding":null,"retired":true,
           "rewards":[],"requirements":[],"trackedJobId":null,"trackedDone":false}]}
        """;

    [Fact]
    public void Wikelo_reads_in_french_and_his_trades_keep_their_names()
    {
        var page = new Page("fr");
        page.Serve("/api/wikelo", Emporium);
        page.Do("wikeloGroup = 'vehicles'; await loadWikelo();");
        var list = page.NodeText("#wikelo-list");

        Assert.Contains("Most Special Wolf", list);
        Assert.Contains("+30 rép.", list);
        Assert.Contains("vu à Ruin Station", list);
        Assert.Contains("Nécessite Very Good Customer (340 rép.) ou mieux", list);
        Assert.Contains("Suivre comme objectif", list);
        Assert.Contains("Vaisseaux et véhicules", page.NodeText("#wikelo-groups"));
        Assert.Equal("1 échange · retirés du fichier, non affichés : 1", page.NodeText("#wikelo-count").Replace(' ', ' '));
    }

    [Fact]
    public void A_shopping_list_reads_in_french_but_keeps_the_title_its_pilot_gave_it()
    {
        var page = new Page("fr");
        page.Serve("/api/jobs", """
            [{"id":"restock","title":"Restock after Pyro","kind":"list","done":false,"pinned":false,
              "haveCount":1,"totalCount":4,"items":[{"name":"Agricium","needed":4,"unit":"SCU","have":false,"where":[],"wornNow":true}]}]
            """);
        page.Do("await loadJobList();");
        var list = page.NodeText("#jobs-list");

        Assert.Equal("1 liste active · 3 articles à trouver", page.NodeText("#operations-shopping-status"));
        Assert.Contains("Restock after Pyro", list);
        Assert.Contains("Épingler sur Direct", list);
        Assert.Contains("1 sur 4 en main", list);
        Assert.Contains("porté en ce moment", list);
    }

    [Fact]
    public void A_checklist_reads_in_french()
    {
        var page = new Page("fr");
        page.Serve("/api/checklists", """
            [{"id":"c1","title":"Pyro departure","pinned":false,"items":[
              {"id":"i1","text":"Refuel","dueAt":null,"note":null,"attachments":[],"done":true,"doneAt":null},
              {"id":"i2","text":"Bring tractor beam","dueAt":null,"note":null,"attachments":[],"done":false,"doneAt":null}]}]
            """);
        page.Do("await loadChecklists();");
        var list = page.NodeText("#checklists-list");

        Assert.Contains("Pyro departure", list);
        Assert.Contains("1 sur 2 faits", list);
        Assert.Contains("Ajouter la tâche", list);
        Assert.DoesNotContain("Add task", list);
    }
}
