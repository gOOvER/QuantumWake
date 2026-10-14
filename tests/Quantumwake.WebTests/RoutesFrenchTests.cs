namespace Quantumwake.WebTests;

/// <summary>
/// The Routes page in French: the result rows, the server's words for what
/// limited a run, and the evidence marks.
/// </summary>
/// <remarks>
/// The same route as <see cref="RouteReliabilityTests"/>, so a French run that
/// drifts from the English one shows up as a missing French phrase rather
/// than as nothing at all.
/// </remarks>
public class RoutesFrenchTests
{
    private static Page Loaded()
    {
        var page = new Page("fr");
        page.Serve("/api/routes?scu=64&capital=10000&from=&ranking=reliable&freshOnly=false&evidence=reported", """
            [{"commodity":"Beryl","buyAt":"Shubin","buyPrice":100,"sellAt":"TDD","sellPrice":150,
              "marginPerScu":50,"units":18,"profit":900,"outlay":1800,"limitedBy":"demand",
              "desiredUnits":64,"buyStockScu":80,"sellDemandScu":18,"buyAvailability":"enough","sellAvailability":"limited","availability":"reported-partial","mapReady":false,"buySeenAt":"2026-08-23T10:00:00Z",
              "sellSeenAt":"2026-08-23T11:00:00Z","freshness":"fresh","buySecurity":"monitored","sellSecurity":"lawless",
              "fallbackSells":[{"terminal":"Area18 TDD","sellPrice":142,"demandScu":64,"seenAt":"2026-08-23T10:00:00Z","freshness":"fresh"}]}]
            """);

        page.Do("__dom.node('#routes-ship').value = '64'; __dom.node('#routes-capital').value = '10000'; await loadRoutes();");
        return page;
    }

    [Fact]
    public void A_route_row_reads_in_french()
    {
        var text = Loaded().NodeText("#routes-table tbody");

        Assert.Contains("Relevés récents", text);
        Assert.Contains("Partiel signalé · 18 / 64 SCU", text);
        Assert.Contains("stock à l’achat 80 SCU (suffisant)", text);
        Assert.Contains("Autres acheteurs", text);
        Assert.Contains("Enregistrer en texte", text);
        Assert.Contains("Fiabilité", text);
        Assert.Contains("1 extrémité sans loi", text);
        Assert.DoesNotContain("Fresh reports", text);
        Assert.DoesNotContain("Save text route", text);
    }

    /// <summary>The words are French; the classes that colour them stay the server's.</summary>
    [Fact]
    public void Security_and_limit_words_are_translated_but_their_classes_are_not()
    {
        var page = Loaded();
        var row = "__dom.node('#routes-table tbody').children[0]";

        Assert.Contains("sans loi", page.Text($"{row}.textContent"));
        Assert.Contains("demande", page.Text($"{row}.textContent"));
        Assert.Equal(1, page.Count($"{row}.querySelectorAll('.sec-lawless').length"));
    }

    [Fact]
    public void Evidence_marks_are_french_but_keep_their_english_class()
    {
        var page = new Page("fr");

        var marks = "sourceMarks(['logs', 'community'])";
        Assert.Contains("JOURNAL DU JEU", page.Text($"{marks}.textContent"));
        Assert.Contains("DONNÉES COMMUNAUTAIRES", page.Text($"{marks}.textContent"));
        Assert.Equal("data-source game-log", page.Text($"{marks}.children[0].className"));
    }
}
