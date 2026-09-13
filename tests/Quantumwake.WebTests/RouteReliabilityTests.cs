namespace Quantumwake.WebTests;

/// <summary>The route page makes a report's limits visible before its profit.</summary>
public class RouteReliabilityTests
{
    [Fact]
    public void A_route_names_price_age_demand_and_a_fallback_buyer()
    {
        var page = new Page();
        page.Serve("/api/routes?scu=64&capital=10000&from=&ranking=reliable&freshOnly=false&evidence=reported", """
            [{"commodity":"Beryl","buyAt":"Shubin","buyPrice":100,"sellAt":"TDD","sellPrice":150,
              "marginPerScu":50,"units":18,"profit":900,"outlay":1800,"limitedBy":"demand",
              "desiredUnits":64,"buyStockScu":80,"sellDemandScu":18,"buyAvailability":"enough","sellAvailability":"limited","availability":"reported-partial","mapReady":false,"buySeenAt":"2026-08-23T10:00:00Z",
              "sellSeenAt":"2026-08-23T11:00:00Z","freshness":"fresh",
              "fallbackSells":[{"terminal":"Area18 TDD","sellPrice":142,"demandScu":64,"seenAt":"2026-08-23T10:00:00Z","freshness":"fresh"}]}]
            """);

        page.Do("__dom.node('#routes-ship').value = '64'; __dom.node('#routes-capital').value = '10000'; await loadRoutes();");

        var text = page.NodeText("#routes-table tbody");
        Assert.Contains("Fresh reports", text);
        Assert.Contains("Reported partial · 18 / 64 SCU", text);
        Assert.Contains("buy stock 80 SCU (enough)", text);
        Assert.Contains("sell demand 18 SCU (limited)", text);
        Assert.Contains("Alternate buyers", text);
        Assert.Contains("Area18 TDD", text);
        Assert.Contains("Save alternate", text);
        Assert.Contains("Split load 64 SCU", text);
        Assert.Contains("Save split", text);
        Assert.Contains("Save text route", text);
        Assert.Contains("Repeatability", text);
        Assert.Contains("cargo exposure", text);
        Assert.Contains("Turnaround:", text);
        Assert.Contains("Time: not estimated", text);
        Assert.Contains("demand", text);
    }

    [Fact]
    public void Fresh_only_is_part_of_the_route_query()
    {
        var page = new Page();
        page.Do("__dom.node('#routes-fresh-only').checked = true; await loadRoutes();");

        Assert.Contains(page.Fetched(), url => url.Contains("freshOnly=true"));
        Assert.Contains(page.Fetched(), url => url.Contains("ranking=reliable"));
        Assert.Contains(page.Fetched(), url => url.Contains("evidence=reported"));
    }

    [Fact]
    public void Unknown_capacity_is_named_an_estimate_and_can_be_included()
    {
        var page = new Page();
        page.Serve("/api/routes?scu=64&capital=0&from=&ranking=reliable&freshOnly=false&evidence=any", """
            [{"commodity":"Beryl","buyAt":"Levski","buyPrice":100,"sellAt":"HUR-L1","sellPrice":150,
              "marginPerScu":50,"units":64,"profit":3200,"outlay":6400,"limitedBy":"hold","desiredUnits":64,
              "buyAvailability":"unknown","sellAvailability":"enough","availability":"capacity-unknown","sellDemandScu":85,
              "freshness":"fresh","mapReady":true,"fallbackSells":[]}]
            """);

        page.Do("__dom.node('#routes-evidence').value = 'any'; __dom.node('#routes-ship').value = '64'; await loadRoutes();");

        var text = page.NodeText("#routes-table tbody");
        Assert.Contains("Capacity unknown · projected 64 SCU", text);
        Assert.Contains("buy stock unknown", text);
        Assert.Contains("~3,200", text);
        Assert.Contains("Save route", text);
    }

    [Fact]
    public void A_safety_and_pad_choice_are_sent_to_the_route_selector_and_shown_on_the_run()
    {
        var page = new Page();
        page.Serve("/api/routes?scu=64&capital=0&from=&ranking=reliable&freshOnly=false&evidence=reported&safety=monitored&pad=known", """
            [{"commodity":"Beryl","buyAt":"Area18 TDD","buyPrice":100,"buySecurity":"monitored","buyLandingPads":["Landing Pad XL"],
              "sellAt":"Port Tressler TDD","sellPrice":150,"sellSecurity":"monitored","sellLandingPads":["Landing Pad L"],
              "marginPerScu":50,"units":64,"profit":3200,"outlay":6400,"limitedBy":"hold","desiredUnits":64,
              "availability":"reported-full","buyAvailability":"enough","sellAvailability":"enough","freshness":"fresh","mapReady":true,"fallbackSells":[]}]
            """);

        page.Do("""
            __dom.node('#routes-ship').value = '64';
            __dom.node('#routes-safety').value = 'monitored';
            __dom.node('#routes-pad').value = 'known';
            await loadRoutes();
            """);

        var text = page.NodeText("#routes-table tbody");
        Assert.Contains("monitored", text);
        Assert.Contains("Landing Pad XL", text);
        Assert.Contains(page.Fetched(), url => url.Contains("safety=monitored") && url.Contains("pad=known"));
    }

    [Fact]
    public void Best_safe_route_keeps_the_pad_choice_and_highlights_the_top_reliable_match()
    {
        var page = new Page();
        page.Serve("/api/routes?scu=64&capital=0&from=&ranking=reliable&freshOnly=false&evidence=reported&safety=prefer-monitored&pad=known", """
            [{"commodity":"Beryl","buyAt":"Area18 TDD","buyPrice":100,"buySecurity":"monitored","buyLandingPads":["Landing Pad XL"],
              "sellAt":"Port Tressler TDD","sellPrice":150,"sellSecurity":"monitored","sellLandingPads":["Landing Pad L"],
              "marginPerScu":50,"units":64,"profit":3200,"outlay":6400,"limitedBy":"hold","desiredUnits":64,
              "availability":"reported-full","buyAvailability":"enough","sellAvailability":"enough","freshness":"fresh","mapReady":true,"fallbackSells":[]}]
            """);

        page.Do("""
            __dom.node('#routes-ship').value = '64';
            __dom.node('#routes-pad').value = 'known';
            await chooseBestSafeRoute();
            """);

        Assert.Contains("GET /api/routes?scu=64&capital=0&from=&ranking=reliable&freshOnly=false&evidence=reported&safety=prefer-monitored&pad=known", string.Join("\n", page.Fetched()));
        Assert.Contains("Best safe match", page.NodeText("#routes-table tbody"));
        Assert.Contains("Ready", page.NodeText("#route-readiness"));
        Assert.Contains(page.Fetched(), url => url.Contains("ranking=reliable") && url.Contains("safety=prefer-monitored") && url.Contains("pad=known"));
    }

    /// <summary>
    /// The circuits under the table are asked with the table's own filters.
    /// They used to be asked with the load and the money alone, so a table
    /// set to Monitored only could have a Pyro loop proposed beneath it.
    /// </summary>
    [Fact]
    public void Circuits_carry_every_filter_the_table_was_built_with()
    {
        var page = new Page();
        page.Do("""
            __dom.node('#routes-ship').value = '64';
            __dom.node('#routes-capital').value = '10000';
            __dom.node('#routes-safety').value = 'monitored';
            __dom.node('#routes-pad').value = 'xl';
            __dom.node('#routes-evidence').value = 'full';
            __dom.node('#routes-fresh-only').checked = true;
            await loadRoutes();
            """);

        var circuits = Assert.Single(page.Fetched(), url => url.Contains("/api/routes/circuits?"));
        Assert.Contains("scu=64", circuits);
        Assert.Contains("capital=10000", circuits);
        Assert.Contains("safety=monitored", circuits);
        Assert.Contains("pad=xl", circuits);
        Assert.Contains("evidence=full", circuits);
        Assert.Contains("freshOnly=true", circuits);
    }

    [Fact]
    public void An_empty_table_under_avoid_lawless_names_that_choice()
    {
        var page = new Page();
        page.Do("""
            __dom.node('#routes-ship').value = '64';
            __dom.node('#routes-safety').value = 'avoid-lawless';
            __dom.node('#routes-fresh-only').checked = true;
            await loadRoutes();
            """);

        var text = page.NodeText("#routes-table tbody");
        Assert.Contains("out of lawless space", text);
        Assert.DoesNotContain("Fresh only", text);
    }

    /// <summary>
    /// A recap is built on demand and lives only in the page: a refreshed or
    /// bookmarked #share has nothing to show, so it lands on Settings, where
    /// the button is, rather than on an empty article with a Save button
    /// that does nothing.
    /// </summary>
    [Fact]
    public void A_share_fragment_with_no_recap_built_lands_on_settings()
    {
        var page = new Page();
        page.Do("location.hash = '#share'; shareReportData = null;");

        Assert.Equal("settings", page.Text("viewFromHash()"));

        page.Do("shareReportData = { title: 'x' };");

        Assert.Equal("share", page.Text("viewFromHash()"));
    }
}
