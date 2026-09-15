namespace Quantumwake.WebTests;

/// <summary>
/// The one-click summary for a support thread: the app, the game data, the
/// price feeds and the parser in a few lines of text, built from the same
/// chosen lists the full report is, so nothing private can be in it.
/// </summary>
public class DiagnosticSummaryTests
{
    private const string Report = """
        {"producer":{"name":"Quantumwake","version":"0.13.26"},"takenAt":"2026-09-15T18:00:00+00:00",
         "install":{"found":true,"channel":"LIVE","hasGameLog":true,"backups":151},
         "library":{"sessions":198,"counted":190,"first":"2026-05-15T00:00:00+00:00","last":"2026-09-15T12:00:00+00:00",
                    "builds":[{"build":"4.10.0","sessions":120},{"build":"4.9.0","sessions":78}]},
         "parser":{"unread":3,"samples":false,"tags":[{"tag":"x","count":2},{"tag":"y","count":1}]},
         "views":{"ships":19,"places":88,"contracts":40},
         "data":{"community":true,"communityDump":"4.10.0-LIVE.12519617","uex":true,"uexKeysStored":false},
         "wipe":{"at":"2026-05-15T00:00:00+00:00","patch":"4.9","scope":"Money","hidden":8}}
        """;

    [Fact]
    public void The_summary_names_the_app_the_data_the_feeds_and_the_parser_and_nothing_private()
    {
        var page = new Page();
        page.Do("window.__summary = diagnosticSummary("
            + "{version:'0.13.26', build:'0.13.26+abc'},"
            + "{state:'ready', problem:null, finishedAt:'2026-09-15T17:00:00+00:00', counts:{vehicles:1094, items:26028}},"
            + "{enabled:true, prices:24140, fetchedAt:'2026-09-13T05:48:30+00:00'},"
            + "[{key:'rentals', enabled:true, fetchedAt:'2026-09-13T05:48:30+00:00'}, {key:'marketplace', enabled:false}],"
            + Report + ");");
        var text = page.Text("window.__summary");

        Assert.StartsWith("Quantum Wake 0.13.26 (0.13.26+abc)", text);
        Assert.Contains("Install: LIVE, Game.log present, 151 backup logs", text);
        Assert.Contains("Game data: ready", text);
        Assert.Contains("vehicles 1094, items 26028", text);
        Assert.Contains("Community dataset: on, dump 4.10.0-LIVE.12519617", text);
        Assert.Contains("UEX prices: on, 24140 prices, fetched", text);
        Assert.Contains("UEX feeds: rentals", text);
        Assert.DoesNotContain("marketplace", text);
        Assert.Contains("Sessions: 198 read, 190 counted, 2026-05-15 to 2026-09-15", text);
        Assert.Contains("Game builds: 4.10.0 (120), 4.9.0 (78)", text);
        Assert.Contains("Parser: 3 unreadable lines across 2 tags", text);
        Assert.Contains("Counts: ships 19, places 88, contracts 40", text);
        Assert.Contains("Wipe line: 2026-05-15 (4.9), 8 sessions before it", text);
        // No path, handle or key ever enters: the inputs have none, and the text carries only what was named.
        Assert.DoesNotContain("C:\\", text);
        Assert.DoesNotContain("AppData", text);
    }

    [Fact]
    public void Missing_answers_read_as_off_or_unknown_rather_than_throwing()
    {
        var page = new Page();
        page.Do("window.__summary = diagnosticSummary(null, null, null, [], {takenAt:'2026-09-15T18:00:00+00:00'});");
        var text = page.Text("window.__summary");

        Assert.StartsWith("Quantum Wake ? (build unknown)", text);
        Assert.Contains("Install: not found", text);
        Assert.Contains("Game data: unknown", text);
        Assert.Contains("UEX prices: off", text);
        Assert.Contains("UEX feeds: none", text);
        Assert.Contains("Parser: nothing unreadable", text);
    }

    /// <summary>The click fetches the live answers and, with no clipboard in the stub, shows the text to copy by hand.</summary>
    [Fact]
    public void The_button_builds_the_summary_from_live_answers_and_shows_it_when_there_is_no_clipboard()
    {
        var page = new Page();
        page.Serve("/api/version", """{"version":"0.13.26","build":"0.13.26+abc"}""");
        page.Serve("/api/gamedata", """{"state":"ready","problem":null,"counts":{}}""");
        page.Serve("/api/uex", """{"enabled":false,"prices":0,"fetchedAt":null}""");
        page.Serve("/api/uex/feeds", "[]");
        page.Serve("/api/diagnostics?samples=false", Report);

        // The stub offers a clipboard; take it away to see the fallback.
        page.Do("navigator.clipboard = undefined; await __dom.node('#diag-copy').fire('click');");

        Assert.Contains("GET /api/diagnostics?samples=false", page.Fetched());
        Assert.Contains("shown below to copy by hand", page.NodeText("#diag-status"));
        Assert.False(page.Truth("__dom.node('#diag-summary').hidden"));
        Assert.StartsWith("Quantum Wake 0.13.26", page.Text("__dom.node('#diag-summary').value"));
    }
}
