namespace Quantumwake.WebTests;

/// <summary>A history row opens the evidence the session already carries.</summary>
public class SessionDebriefTests
{
    private const string Detail = """
        {
          "id":"s1","startedAt":"2026-08-20T20:00:00Z","endedAt":"2026-08-20T21:30:00Z","gameVersion":"4.9",
          "ships":[{"displayName":"RSI Zeus","sorties":2}],
          "locations":[
            {"at":"2026-08-20T20:05:00Z","rawId":"RR_MIC_LEO","displayName":"Port Tressler","system":"Stanton","body":"microTech"},
            {"at":"2026-08-20T20:45:00Z","rawId":"RR_HUR_L1","displayName":"HUR-L1","system":"Stanton"}
          ],
          "jumps":[
            {"at":"2026-08-20T20:20:00Z","toId":"party","toName":"PartyMemberMarker_12345"},
            {"at":"2026-08-20T20:30:00Z","toId":"RR_HUR_L1","toName":"HUR-L1"}
          ],
          "contracts":[{"displayName":"Supply run","outcome":"Completed","steps":2,"stepsDone":2}],
          "timeline":[{"at":"2026-08-20T21:10:00Z","kind":"ship","text":"Left RSI Zeus","detail":"~30 min"}],
          "purchases":[{"at":"2026-08-20T20:10:00Z","item":"MedPen","total":500,"quantity":2,"confirmed":true}],
          "trades":[{"at":"2026-08-20T21:00:00Z","amount":4000,"quantity":8,"isSell":true}],
          "partyNotes":[{"at":"2026-08-20T20:20:00Z","handle":"B-Kon","moment":"Connected"}],
          "spend":500,"income":4000,"commoditySpend":0,"deaths":0
        }
        """;

    [Fact]
    public void A_session_row_expands_into_route_economy_contract_and_ship_evidence()
    {
        var page = new Page();
        page.Serve("/api/sessions/s1", Detail);

        page.Do("""
            allSessions = [{ id:'s1', startedAt:'2026-08-20T20:00:00Z', inGame:5100, menu:300,
              primaryShip:'RSI Zeus', lastLocation:'HUR-L1', jumps:1, contracts:1, deaths:0, incapacitations:0 }];
            await toggleSessionDebrief('s1');
            """);

        var text = page.NodeText("#sessions-table tbody");
        Assert.Contains("RSI Zeus · 2 sorties", text);
        Assert.Contains("Port Tressler", text);
        Assert.Contains("HUR-L1", text);
        Assert.Contains("Cargo sold · 8 SCU", text);
        Assert.Contains("Supply run", text);
        Assert.Contains("2 / 2 steps", text);
        Assert.Contains("1 named", text);
        Assert.Contains("Crew observed*", text);
        Assert.Contains("Cargo amounts are kiosk requests", text);
        Assert.DoesNotContain("PartyMemberMarker", text);
    }

    [Fact]
    public void Highlights_drop_the_contract_markup_and_keep_their_own_layout()
    {
        // 1,515 lines of the real logs carry StarStrings' <EM> tags in the
        // toast; the highlights printed them, and as a third grid column the
        // title was squeezed under the detail until the two overprinted.
        var page = new Page();
        page.Serve("/api/sessions/s1", Detail.Replace(
            "\"timeline\":[",
            "\"timeline\":[{\"at\":\"2026-08-20T21:05:00Z\",\"kind\":\"contract\",\"text\":\"Contract accepted\"," +
            "\"detail\":\"Rookie | <EM3>DIRECT</EM3> Small Haul | Ruin Station > Checkmate <EM4>[BP]*</EM4>\"},"));

        page.Do("""
            allSessions = [{ id:'s1', startedAt:'2026-08-20T20:00:00Z', inGame:5100, menu:300,
              primaryShip:'RSI Zeus', lastLocation:'HUR-L1', jumps:1, contracts:1, deaths:0, incapacitations:0 }];
            await toggleSessionDebrief('s1');
            """);

        var text = page.NodeText("#sessions-table tbody");
        Assert.Contains("Rookie | DIRECT Small Haul | Ruin Station > Checkmate [BP]*", text);
        Assert.DoesNotContain("<EM", text);
        Assert.Equal(1, page.Count("__dom.node('#sessions-table tbody').querySelectorAll('.session-debrief-highlights').length"));
    }

    [Fact]
    public void Several_ships_are_listed_one_per_line()
    {
        // Six hulls joined with middots wrapped into a narrow tile as one run
        // of text: "DRAK Clipper · 2 sorties · ORIG 100i · 2 sorties · RSI…".
        var page = new Page();
        page.Serve("/api/sessions/s1", Detail.Replace(
            "\"ships\":[{\"displayName\":\"RSI Zeus\",\"sorties\":2}]",
            "\"ships\":[{\"displayName\":\"DRAK Clipper\",\"sorties\":2},{\"displayName\":\"ORIG 100i\",\"sorties\":1}]"));

        page.Do("""
            allSessions = [{ id:'s1', startedAt:'2026-08-20T20:00:00Z', inGame:5100, menu:300,
              primaryShip:'DRAK Clipper', lastLocation:'HUR-L1', jumps:1, contracts:1, deaths:0, incapacitations:0 }];
            await toggleSessionDebrief('s1');
            """);

        var tile = "__dom.node('#sessions-table tbody').querySelectorAll('.session-metric-ships')";
        Assert.Equal(1, page.Count($"{tile}.length"));
        var lines = $"{tile}[0].querySelectorAll('.session-metric-line')";
        Assert.Equal(2, page.Count($"{lines}.length"));
        Assert.Equal("DRAK Clipper · 2 sorties", page.Text($"{lines}[0].textContent"));
        Assert.Equal("ORIG 100i · 1 sortie", page.Text($"{lines}[1].textContent"));
        Assert.Equal("Ships", page.Text($"{tile}[0].querySelectorAll('.session-metric-label')[0].textContent"));
    }

    [Fact]
    public void Repeating_a_session_creates_an_ordered_flight_plan()
    {
        var page = new Page();
        page.Serve("/api/sessions/s1", Detail);
        page.Serve("/api/trips", "[]");

        page.Do("await toggleSessionDebrief('s1'); await repeatSessionRoute(sessionDetails.get('s1'));");

        var body = page.BodyOf("/api/trips");
        Assert.Contains("Repeat", body);
        Assert.True(body.IndexOf("Port Tressler", StringComparison.Ordinal)
                    < body.IndexOf("HUR-L1", StringComparison.Ordinal));
        Assert.Contains("RR_MIC_LEO", body);
        Assert.Contains("RR_HUR_L1", body);
        Assert.DoesNotContain("PartyMemberMarker", body);
    }
}
