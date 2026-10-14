namespace Quantumwake.WebTests;

/// <summary>
/// Settings → General and the banners in French, including the parts built
/// from what the server says.
/// </summary>
/// <remarks>
/// The picker tells a French reader that this page is translated, so a string
/// that slips back to English here makes the picker untrue. The signal and feed
/// rows are the server's English translated on the page, which is the part a
/// catalogue test cannot see working.
/// </remarks>
public class SettingsFrenchTests
{
    private const string Signals = """
        [{"name":"Quantum jumps","group":"Flight","total":12,"sessions":3,"lastSeen":"2026-10-12T20:00:00Z","note":null},
         {"name":"Kills","group":"Casualties","total":0,"sessions":0,"lastSeen":null,
          "note":"Not logged at all since 4.9. The parser is written and dormant, and this fills in by itself if CIG restore the events."}]
        """;

    private const string Feeds = """
        [{"key":"fuel","title":"Fuel prices","description":"Quantum and hydrogen fuel prices per terminal - what a long haul actually costs to fly.",
          "cost":"~40 KB","enabled":false,"fetchedAt":null}]
        """;

    [Fact]
    public void Signal_rows_from_the_server_come_out_french()
    {
        var page = new Page("fr");
        page.Serve("/api/signals", Signals);

        page.Do("await renderSignals();");

        var table = page.NodeText("#signals-table");
        Assert.Contains("Vol", table);
        Assert.Contains("Sauts quantiques", table);
        Assert.Contains("Éliminations", table);
        Assert.Contains("jamais", table);
        Assert.DoesNotContain("never", table);
        Assert.StartsWith("Plus du tout enregistré depuis 4.9",
            page.Text("__dom.node('#signals-table').querySelector('tbody').children[3].title"));
    }

    [Fact]
    public void Feed_rows_from_the_server_come_out_french()
    {
        var page = new Page("fr");
        page.Serve("/api/uex/feeds", Feeds);

        page.Do("await renderUexFeeds();");

        var list = page.NodeText("#uex-feed-list");
        Assert.Contains("Prix du carburant", list);
        Assert.Contains("~40 Ko", list);
        Assert.Contains("Récupérer", list);
        Assert.Contains("désactivé", list);
        Assert.DoesNotContain("Fetch", list);
    }

    /// <summary>The same rows on an English page are the server's words, untouched.</summary>
    [Fact]
    public void Signal_rows_stay_the_servers_english_on_an_english_page()
    {
        var page = new Page();
        page.Serve("/api/signals", Signals);

        page.Do("await renderSignals();");

        Assert.Contains("Quantum jumps", page.NodeText("#signals-table"));
        Assert.Contains("never", page.NodeText("#signals-table"));
    }

    [Theory]
    [InlineData(null, "Oct 15, 2026")]
    [InlineData("fr", "15 oct. 2026")]
    public void A_wipe_day_is_written_in_the_page_language(string? language, string expected)
    {
        var page = new Page(language);

        Assert.Equal(expected, page.Text("dayUtc('2026-10-15T00:00:00Z')"));
    }

    [Theory]
    [InlineData(null, 3, "3d ago")]
    [InlineData("fr", 3, "il y a 3 j")]
    [InlineData("fr", 0, "à l’instant")]
    public void How_long_ago_is_written_in_the_page_language(string? language, int days, string expected)
    {
        var page = new Page(language);

        Assert.Equal(expected, page.Text($"ago(new Date(Date.now() - {days} * 86400000 - 1000).toISOString())"));
    }

    /// <summary>One session before the line is singular in French as in English, and so is nought.</summary>
    [Theory]
    [InlineData(1, "1 session antérieure est conservée mais pas comptée")]
    [InlineData(239, "239 sessions antérieures sont conservées mais pas comptées")]
    public void The_wipe_line_counts_in_french(int hidden, string expected)
    {
        var page = new Page("fr");

        page.Do($"showWipeStatus({{ at: '2026-09-01T00:00:00Z', hidden: {hidden}, covers: ['money', 'ships', 'inventory', 'history'] }});");

        Assert.Equal(expected, page.NodeText("#wipe-status"));
    }

    [Fact]
    public void A_partial_wipe_names_what_it_took_in_french()
    {
        var page = new Page("fr");

        page.Do("showWipeStatus({ at: '2026-09-01T00:00:00Z', hidden: 4, covers: ['money', 'ships'] });");

        Assert.Equal("4 sessions antérieures comptent toujours, sauf pour : l’argent, les vaisseaux",
            page.NodeText("#wipe-status"));
    }
}
