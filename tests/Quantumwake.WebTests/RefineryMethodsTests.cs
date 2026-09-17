namespace Quantumwake.WebTests;

/// <summary>
/// The refining methods on the runs pane and the estimate under a run at a
/// refinery: UEX's three-point ratings shown as pips and called ratings, the
/// estimate called a ceiling and said to be before the method and the fee.
/// </summary>
public class RefineryMethodsTests
{
    private const string Pending = """
        [{"id":"m2","place":"Aaron Halo","resource":"Copper","scu":40,"stage":"Refining",
          "refinery":{"place":"MIC-L5","method":"Cormack","expectedAt":"2026-09-18T12:00:00+00:00"},
          "estimate":{"perScu":4200,"sellAt":"Pyro Gateway (Stanton)","gross":168000,"bonusPercent":9,"bonusAt":"Refinement Processing - MIC-L5","withBonus":183120,"bonusKnown":true},
          "method":{"name":"Cormack","code":"COR","yieldRating":1,"costRating":2,"speedRating":3},
          "caveat":"The game keeps the refinery timer and logs nothing about it, so this is the time you told us to expect."},
         {"id":"m3","place":"Aaron Halo","resource":"Quantainium","scu":12,"stage":"Refining",
          "refinery":{"place":"ArcCorp 141","method":"Dinyx","expectedAt":null},
          "estimate":{"perScu":170000,"sellAt":"ArcCorp 141","gross":2040000,"bonusKnown":true},
          "method":null,
          "caveat":"The game keeps the refinery timer and logs nothing about it, so this is the time you told us to expect."}]
        """;

    private const string Model = """
        {"ready":true,"lasers":[],"modules":[],"gadgets":[],"minerals":[],"compositions":[],"ships":[],
         "methodsKnown":true,
         "methods":[{"name":"Cormack","code":"COR","yieldRating":1,"costRating":2,"speedRating":3},
                    {"name":"Dinyx Solventation","code":"DIN","yieldRating":3,"costRating":1,"speedRating":1}],
         "rule":{"requiredWattsPerKg":0.36,"soloRatio":1.15,"gadgetRatio":0.7,"source":"scminer.rocks"}}
        """;

    [Fact]
    public void A_run_at_a_refinery_carries_a_ceiling_with_the_stations_bonus_and_the_methods_pips()
    {
        var page = new Page();
        page.Serve("/api/mining/pending", Pending);
        page.Do("await loadMiningPending();");

        var rows = "__dom.node('#mining-pending-list').children";
        Assert.Equal(2, page.Count($"{rows}.length"));

        var copper = page.Text($"{rows}[0].textContent");
        Assert.Contains("≤ 168,000 aUEC at 4,200/SCU refined (Pyro Gateway (Stanton))", copper);
        Assert.Contains("+9 % station bonus at Refinement Processing - MIC-L5 makes 183,120", copper);
        Assert.Contains("under Cormack (yield 1/3, cost 2/3, speed 3/3)", copper);
        Assert.Contains("before the method’s own yield and the fee, which nobody publishes", copper);

        // No bonus reported for this ore here, and a method the feed did not match is named as typed.
        var quantainium = page.Text($"{rows}[1].textContent");
        Assert.Contains("≤ 2,040,000 aUEC", quantainium);
        Assert.Contains("no station bonus reported for this ore here", quantainium);
        Assert.Contains("under Dinyx", quantainium);
        Assert.DoesNotContain("yield 3/3", quantainium);
    }

    [Fact]
    public void Without_a_price_the_run_has_no_estimate_line()
    {
        var page = new Page();
        page.Serve("/api/mining/pending", Pending.Replace("\"estimate\":{\"perScu\":170000,\"sellAt\":\"ArcCorp 141\",\"gross\":2040000,\"bonusKnown\":true}", "\"estimate\":null"));
        page.Do("await loadMiningPending();");
        Assert.Equal(0, page.Count("__dom.node('#mining-pending-list').children[1].querySelectorAll('.mining-estimate').length"));
        Assert.Equal(1, page.Count("__dom.node('#mining-pending-list').children[0].querySelectorAll('.mining-estimate').length"));
    }

    [Fact]
    public void The_methods_table_shows_the_ratings_as_pips_and_says_whose_they_are()
    {
        var page = new Page();
        page.Serve("/api/mining/model", Model);
        page.Serve("/api/mining/crack", """{"verdict":null,"matrix":[]}""");
        page.Do("crackModel = JSON.parse(" + System.Text.Json.JsonSerializer.Serialize(Model) + "); renderMiningMethods();");

        Assert.False(page.Truth("__dom.node('#mining-methods').hidden"));
        var rows = "__dom.node('#mining-methods-table tbody').children";
        Assert.Equal(2, page.Count($"{rows}.length"));
        // Best yield first.
        Assert.Contains("Dinyx Solventation", page.Text($"{rows}[0].textContent"));
        Assert.Contains("●●●", page.Text($"{rows}[0].textContent"));
        Assert.Contains("●○○", page.Text($"{rows}[1].textContent"));
        Assert.Contains("UEX’s ratings", page.NodeText("#mining-methods-note"));
        Assert.Contains("percentages behind them are the server’s", page.NodeText("#mining-methods-note"));
    }

    [Fact]
    public void With_the_feed_off_the_methods_table_stays_hidden()
    {
        var page = new Page();
        page.Do("crackModel = { methodsKnown: false, methods: [] }; renderMiningMethods();");
        Assert.True(page.Truth("__dom.node('#mining-methods').hidden"));
    }
}
