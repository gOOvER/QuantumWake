namespace Quantumwake.WebTests;

/// <summary>
/// Play history leads with the most recent matching session so a pilot does
/// not need to scan the table before deciding which debrief to open.
/// </summary>
public class SessionBriefTests
{
    [Fact]
    public void Session_brief_uses_the_latest_flight_and_its_recorded_progress()
    {
        var page = new Page();

        page.Do("""
            allSessions = [
              { id:'older', startedAt:'2026-08-20T20:00:00Z', inGame:5100, menu:300,
                primaryShip:'RSI Zeus', lastLocation:'HUR-L1', shard:'EU-1', shards:1,
                jumps:1, contracts:1, deaths:1, incapacitations:2 },
              { id:'latest', startedAt:'2026-08-21T20:00:00Z', inGame:7200, menu:600,
                primaryShip:'Drake Corsair', lastLocation:'Port Tressler', shard:'US-3', shards:2,
                jumps:4, contracts:2, deaths:0, incapacitations:1 }
            ];
            renderSessions();
            """);

        Assert.Equal("Drake Corsair", page.NodeText("#sessions-latest-title"));
        Assert.Contains("Port Tressler", page.NodeText("#sessions-latest-detail"));
        Assert.Equal("2h 00m", page.NodeText("#sessions-duration-title"));
        Assert.Contains("4 jumps", page.NodeText("#sessions-duration-detail"));
        Assert.Equal("No deaths recorded", page.NodeText("#sessions-health-title"));
        Assert.Contains("1 incapacitation", page.NodeText("#sessions-health-detail"));
        Assert.Contains("2 servers recorded", page.NodeText("#sessions-health-detail"));
    }

    [Fact]
    public void Session_brief_explains_a_range_without_matching_sessions()
    {
        var page = new Page();
        page.Do("allSessions = []; renderSessions();");

        Assert.Equal("No sessions in this range", page.NodeText("#sessions-latest-title"));
        Assert.Equal("No flight time recorded", page.NodeText("#sessions-duration-title"));
        Assert.Equal("No session health data", page.NodeText("#sessions-health-title"));
        Assert.True(page.Truth("$('#sessions-open-latest').disabled"));
    }

    [Fact]
    public void Latest_session_control_opens_the_matching_debrief()
    {
        var page = new Page();
        page.Serve("/api/sessions/latest", "{ \"id\": \"latest\" }");

        page.Do("""
            allSessions = [
              { id:'older', startedAt:'2026-08-20T20:00:00Z', inGame:60, menu:0,
                jumps:0, contracts:0, deaths:0, incapacitations:0 },
              { id:'latest', startedAt:'2026-08-21T20:00:00Z', inGame:60, menu:0,
                jumps:0, contracts:0, deaths:0, incapacitations:0 }
            ];
            renderSessions();
            await $('#sessions-open-latest').fire('click');
            """);

        Assert.Equal("latest", page.Text("expandedSessionId"));
    }
}
