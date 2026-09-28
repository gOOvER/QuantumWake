namespace Quantumwake.WebTests;

/// <summary>
/// The logbook should make its mixed timeline useful before the pilot reads
/// every line: the newest event, what kinds of activity occurred, and only
/// the net of values the log actually recorded.
/// </summary>
public class LogbookDeckTests
{
    [Fact]
    public void Logbook_brief_summarises_latest_activity_and_recorded_trade_balance()
    {
        var page = new Page();
        page.Serve("/api/logbook?days=0", """
            [
              { "at": "2026-09-27T15:00:00Z", "kind": "session", "what": "Session aboard Drake Corsair", "place": "Orison", "detail": "1h 20m", "amount": null },
              { "at": "2026-09-27T14:00:00Z", "kind": "sold", "what": "Agricium", "place": "Area18", "detail": "TDD", "amount": 250000 },
              { "at": "2026-09-27T13:00:00Z", "kind": "bought", "what": "RMC", "place": "Orison", "detail": "Admin", "amount": -100000 },
              { "at": "2026-09-27T12:00:00Z", "kind": "loot", "what": "Coda Pistol", "place": "Orison", "detail": "First seen", "amount": null }
            ]
            """);

        page.Do("await loadLogbook();");

        Assert.Equal("Session aboard Drake Corsair", page.NodeText("#logbook-latest-title"));
        Assert.Contains("Orison", page.NodeText("#logbook-latest-detail"));
        Assert.Equal("1 session · 2 trade records · 1 discovery", page.NodeText("#logbook-activity-title"));
        Assert.Equal("+150,000 aUEC", page.NodeText("#logbook-net-title"));
        Assert.Contains("2 recorded trade entries", page.NodeText("#logbook-net-detail"));
    }

    [Fact]
    public void Logbook_brief_explains_an_empty_record_range()
    {
        var page = new Page();
        page.Serve("/api/logbook?days=0", "[]");

        page.Do("await loadLogbook();");

        Assert.Equal("No activity recorded", page.NodeText("#logbook-latest-title"));
        Assert.Equal("Nothing in this range", page.NodeText("#logbook-activity-title"));
        Assert.Equal("No recorded trade value", page.NodeText("#logbook-net-title"));
    }
}
