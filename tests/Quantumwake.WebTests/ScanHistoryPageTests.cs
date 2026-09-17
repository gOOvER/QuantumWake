namespace Quantumwake.WebTests;

/// <summary>
/// The Log page's second pane: every Game.log the install has and what the
/// scans made of each, beside the scans themselves.
/// </summary>
/// <remarks>
/// Every other page is built from these files and none of them says so. This
/// one exists so "the Cargo page is empty" can be answered with "because the
/// log it would come from was never read", which is a different fix from
/// "because the parser is wrong".
/// </remarks>
public class ScanHistoryPageTests
{
    private const string TwoFilesOneGone = """
        {"root":"C:\\Games\\LIVE",
         "runs":[
           {"startedAt":"2026-09-17T10:00:00Z","finishedAt":"2026-09-17T10:00:02Z","files":3,"parsed":1,"forced":false},
           {"startedAt":"2026-09-16T10:00:00Z","finishedAt":"2026-09-16T10:00:31Z","files":3,"parsed":3,"forced":true}
         ],
         "files":[
           {"name":"Game.log","path":"C:\\Games\\LIVE\\Game.log","live":true,"bytes":3145728,
            "writtenAt":"2026-09-17T11:30:00Z","state":"changed","scannedAt":"2026-09-17T10:00:01Z",
            "sessionId":"s3","startedAt":"2026-09-17T09:00:00Z","endedAt":"2026-09-17T09:55:00Z","handle":"nekron"},
           {"name":"Game Backup 2026-09-16.log","path":"C:\\Games\\LIVE\\logbackups\\Game Backup 2026-09-16.log",
            "live":false,"bytes":2097152,"writtenAt":"2026-09-16T22:00:00Z","state":"current",
            "scannedAt":null,"sessionId":"s2","startedAt":"2026-09-16T20:00:00Z","endedAt":"2026-09-16T21:50:00Z","handle":"nekron"},
           {"name":"Game Backup 2026-09-10.log","path":"C:\\Games\\LIVE\\logbackups\\Game Backup 2026-09-10.log",
            "live":false,"bytes":null,"writtenAt":null,"state":"gone",
            "scannedAt":"2026-09-11T10:00:00Z","sessionId":"s1","startedAt":"2026-09-10T20:00:00Z","endedAt":"2026-09-10T21:00:00Z","handle":"nekron"}
         ],
         "onDisk":2,"bytes":5242880}
        """;

    private static Page Loaded(string history)
    {
        var page = new Page();
        page.Serve("/api/scan/history", history);
        page.Do("await loadScanHistory();");
        return page;
    }

    [Fact]
    public void Every_file_is_listed_with_what_the_scan_made_of_it()
    {
        var body = Loaded(TwoFilesOneGone).NodeText("#scan-files-table tbody");

        Assert.Contains("Game.log", body);
        Assert.Contains("live", body);
        Assert.Contains("grown", body);
        Assert.Contains("3 MB", body);
        Assert.Contains("read", body);
        Assert.Contains("gone", body);
        Assert.Contains("nekron", body);
        // The live log's session, as the log's own clock had it.
        Assert.Contains("55m", body);
    }

    [Fact]
    public void The_summary_counts_what_is_in_the_folder_not_what_the_store_remembers()
    {
        var summary = Loaded(TwoFilesOneGone).NodeText("#scan-files-summary");

        Assert.Contains("2 in the folder", summary);
        Assert.Contains("5 MB", summary);
        Assert.Contains("1 read as they stand", summary);
        Assert.Contains("1 gone", summary);
    }

    /// <summary>
    /// A row written by a build that did not yet stamp its reads says so,
    /// rather than showing a blank that reads as "never".
    /// </summary>
    [Fact]
    public void A_read_from_before_the_stamp_existed_is_named_rather_than_blank()
    {
        var body = Loaded(TwoFilesOneGone).NodeText("#scan-files-table tbody");

        Assert.Contains("before 0.14.10", body);
    }

    [Fact]
    public void Scans_are_listed_newest_first_and_a_forced_one_says_so()
    {
        var page = Loaded(TwoFilesOneGone);
        var body = page.NodeText("#scan-runs-table tbody");

        Assert.Contains("full re-read", body);
        Assert.Contains("new logs", body);
        Assert.Contains("31s", body);
        Assert.True(body.IndexOf("new logs", StringComparison.Ordinal) < body.IndexOf("full re-read", StringComparison.Ordinal));
        Assert.Contains("1 of 3 read", page.NodeText("#scan-runs-summary"));
    }

    [Fact]
    public void No_install_says_so_instead_of_an_empty_table()
    {
        var page = Loaded("""{"root":null,"runs":[],"files":[],"onDisk":0,"bytes":0}""");

        Assert.Contains("No install found", page.NodeText("#scan-files-table tbody"));
        Assert.Contains("No scan recorded yet", page.NodeText("#scan-runs-table tbody"));
    }

    private static string Active(Page page, string id) =>
        page.Text($"__dom.node('#{id}').classList.contains('active') ? 'on' : 'off'");

    [Fact]
    public void The_panes_take_turns_and_the_header_buttons_follow()
    {
        var page = new Page();
        page.Serve("/api/scan/history", """{"root":null,"runs":[],"files":[],"onDisk":0,"bytes":0}""");

        page.Do("showLogPane('files');");
        Assert.Equal("on", Active(page, "log-pane-files"));
        Assert.Equal("off", Active(page, "log-pane-readings"));
        Assert.True(page.Truth("__dom.node('#screen-log-clear').hidden"));
        Assert.False(page.Truth("__dom.node('#scan-now').hidden"));
        Assert.Equal("files", page.Text("localStorage.getItem('qw-log-pane')"));
        Assert.Contains("Every log the game has written", page.NodeText("#log-caption"));
        // Opening the pane is what reads it; nobody has to press Refresh first.
        Assert.Contains("GET /api/scan/history", page.Fetched());

        page.Do("showLogPane('readings');");
        Assert.Equal("on", Active(page, "log-pane-readings"));
        Assert.False(page.Truth("__dom.node('#screen-log-clear').hidden"));
        Assert.True(page.Truth("__dom.node('#scan-now').hidden"));

        page.Do("showLogPane('nonsense');");
        Assert.Equal("on", Active(page, "log-pane-readings"));
    }

    /// <summary>
    /// Scan now is the routine pass, not the full re-read Settings offers: it
    /// must not send <c>force</c>, or a click costs half a minute and 400 MB.
    /// </summary>
    [Fact]
    public void Scan_now_asks_for_a_routine_pass_and_then_re_reads_the_table()
    {
        var page = new Page();
        page.Serve("/api/scan", """{"parsed":1,"sessions":150}""");
        page.Serve("/api/scan/history", TwoFilesOneGone);

        page.Do("__dom.node('#scan-now').click();");

        Assert.Contains("POST /api/scan", page.Fetched());
        Assert.DoesNotContain("POST /api/scan?force=true", page.Fetched());
        Assert.Contains("Game.log", page.NodeText("#scan-files-table tbody"));
    }
}
