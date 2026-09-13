namespace Quantumwake.WebTests;

/// <summary>
/// A recap is for another person to read, so it must retain the useful facts
/// and the source/limit that stops a log-derived trading figure reading as all
/// income.
/// </summary>
public class ShareReportTests
{
    [Fact]
    public void A_shareable_recap_combines_the_selected_session_fleet_and_trade_snapshots()
    {
        var page = new Page();
        page.Serve("/api/sessions", """
            [{"id":"s-1","startedAt":"2026-09-12T18:00:00Z","endedAt":"2026-09-12T20:00:00Z",
              "inGame":5400,"primaryShip":"Drake Corsair","locations":2,"contracts":1,"deaths":0}]
            """);
        page.Serve("/api/sessions/s-1", """
            {"ships":[{"displayName":"Drake Corsair","sorties":1}],"locations":[
              {"at":"2026-09-12T18:15:00Z","rawId":"area18","displayName":"Area18","system":"Stanton"}],
              "jumps":[],"contracts":[]}
            """);
        page.Serve("/api/fleet", """
            {"owned":3,"flown":[{"name":"Drake Corsair","sorties":8,
              "estimatedTime":"04:00:00","lastFlown":"2026-09-12T20:00:00Z"}]}
            """);
        page.Serve("/api/earnings?days=30", """
            {"window":{"earned":84000,"inGame":"04:00:00","perHour":21000,"days":30}}
            """);
        page.Serve("/api/market", """
            [{"name":"Agricium","myScuSold":24,"myRevenue":84000,"myTrades":2}]
            """);

        page.Do("await openShareReport();");

        var report = page.NodeText("#share-report");
        Assert.Contains("Latest session", report);
        Assert.Contains("Drake Corsair", report);
        Assert.Contains("Fleet snapshot", report);
        Assert.Contains("Trading snapshot", report);
        Assert.Contains("Agricium", report);
        Assert.Contains("GAME LOG", report);
        Assert.Contains("not total income", report);
    }
}
