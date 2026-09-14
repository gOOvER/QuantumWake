namespace Quantumwake.WebTests;

/// <summary>
/// Planning starts with the hull, so a generic price lead cannot masquerade as
/// a cargo run a ground vehicle (or a ship without a grid) can make - and a
/// hull nobody has sized cannot hide every route either.
/// </summary>
/// <remarks>
/// The reference rides on the fleet row, as <c>/api/stats</c> delivers it:
/// resolved by class name on the server. The page once re-matched display
/// names against <c>/api/reference/ships</c> and lost the grid on every ship
/// until that catalogue landed, which on a cold start is a good half minute
/// after the fleet.
/// </remarks>
public class ShipPlanningTests
{
    private const string Corsair =
        "{ name: 'Drake Corsair', className: 'DRAK_Corsair', lastFlown: '2026-09-13T00:00:00Z', reference: { name: 'Drake Corsair', cargoScu: 72, crew: 4, isSpaceship: true } }";

    private const string Utv =
        "{ name: 'Greycat UTV', className: 'GRIN_UTV', lastFlown: '2026-09-13T00:00:00Z', reference: { name: 'Greycat UTV', cargoScu: 0, crew: 1, isSpaceship: false } }";

    private const string Hermes =
        "{ name: 'RSI Hermes', className: 'RSI_Hermes', lastFlown: '2026-09-13T00:00:00Z' }";

    [Fact]
    public void A_selected_cargo_ship_sizes_routes_and_names_its_supported_options()
    {
        var page = new Page();
        page.Serve("/api/routes?scu=72&capital=0&from=&ranking=reliable&freshOnly=false&evidence=reported", "[]");

        page.Do($$"""
            libraryStats = { ships: [{{Corsair}}] };
            shipCatalogue = [];
            planningShipName = 'Drake Corsair';
            renderShipPlan();
            await loadRoutes();
            """);

        Assert.Contains("Drake Corsair", page.NodeText("#ship-plan-title"));
        Assert.Contains("72 SCU cargo grid", page.NodeText("#ship-plan-options"));
        Assert.Contains("Crew", page.NodeText("#ship-plan-options"));
        Assert.Contains(page.Fetched(), url => url.Contains("/api/routes?scu=72"));
    }

    /// <summary>
    /// The grid is known from the fleet row alone, with the catalogue empty:
    /// nothing about the plan waits on the second request.
    /// </summary>
    [Fact]
    public void The_hold_comes_from_the_fleet_row_and_not_from_the_catalogue()
    {
        var page = new Page();
        page.Serve("/api/routes?scu=72&capital=0&from=&ranking=reliable&freshOnly=false&evidence=reported", "[]");

        page.Do($$"""
            libraryStats = { ships: [{{Corsair}}] };
            shipCatalogue = [{ name: 'Drake Corsair', cargoScu: 1, crew: 1, isSpaceship: true }];
            planningShipName = 'Drake Corsair';
            renderShipPlan();
            await loadRoutes();
            """);

        Assert.Contains("72 SCU", page.NodeText("#ship-plan-options"));
        Assert.DoesNotContain(page.Fetched(), url => url.Contains("scu=1&"));
    }

    [Fact]
    public void A_ground_vehicle_hides_trade_routes_instead_of_pricing_an_impossible_haul()
    {
        var page = new Page();
        page.Do($$"""
            libraryStats = { ships: [{{Utv}}] };
            planningShipName = 'Greycat UTV';
            renderShipPlan();
            await loadRoutes();
            """);

        Assert.Contains("Ground vehicle", page.NodeText("#ship-plan-options"));
        Assert.Contains("cargo routes are hidden", page.NodeText("#routes-table tbody"));
        Assert.DoesNotContain(page.Fetched(), url => url.StartsWith("/api/routes?", StringComparison.Ordinal));
    }

    /// <summary>
    /// An unknown hold is not an empty one. With no catalogue, no screenshot
    /// and no entry, the table falls back to the per-SCU rows it always had
    /// and says why - the alternative hid every route from a pilot with the
    /// dataset switched off, and there was no way to get them back.
    /// </summary>
    [Fact]
    public void A_ship_whose_hold_is_unknown_gets_the_per_scu_table_and_a_note()
    {
        var page = new Page();
        page.Serve("/api/routes?scu=0&capital=0&from=&ranking=reliable&freshOnly=false&evidence=reported", "[]");

        page.Do($$"""
            libraryStats = { ships: [{{Hermes}}] };
            hangarShips = { ships: [{ className: 'RSI_Hermes', kind: 'Spaceship' }] };
            shipCatalogue = [];
            planningShipName = 'RSI Hermes';
            renderShipPlan();
            await loadRoutes();
            """);

        Assert.Contains(page.Fetched(), url => url.Contains("/api/routes?scu=0"));
        Assert.Contains("Sized per SCU", page.NodeText("#routes-origin-note"));
        Assert.DoesNotContain("cargo routes are hidden", page.NodeText("#routes-table tbody"));
    }

    [Fact]
    public void A_spaceship_without_the_optional_catalogue_can_use_a_pilot_verified_local_hold_size()
    {
        var page = new Page();
        page.Serve("/api/routes?scu=6&capital=0&from=&ranking=reliable&freshOnly=false&evidence=reported", "[]");

        page.Do($$"""
            libraryStats = { ships: [{{Hermes}}] };
            hangarShips = { ships: [{ className: 'RSI_Hermes', kind: 'Spaceship' }] };
            shipCatalogue = [];
            planningShipName = 'RSI Hermes';
            planningManualCargo = { rsihermes: 6 };
            renderShipPlan();
            await loadRoutes();
            """);

        Assert.Contains("local 6 SCU hold entry", page.NodeText("#ship-plan-options"));
        Assert.Contains(page.Fetched(), url => url.Contains("/api/routes?scu=6"));
    }

    [Fact]
    public void A_kiosk_screenshot_hold_is_preferred_to_a_manual_entry_when_the_catalogue_is_unavailable()
    {
        var page = new Page();
        page.Serve("/api/routes?scu=6&capital=0&from=&ranking=reliable&freshOnly=false&evidence=reported", "[]");

        page.Do($$"""
            libraryStats = { ships: [{{Hermes}}] };
            hangarShips = { ships: [{ className: 'RSI_Hermes', kind: 'Spaceship', cargoScu: 6, cargoSource: 'screenshot', cargoReadAt: '2026-09-13T00:00:00Z' }] };
            shipCatalogue = [];
            planningShipName = 'RSI Hermes';
            planningManualCargo = { rsihermes: 4 };
            renderShipPlan();
            await loadRoutes();
            """);

        Assert.Contains("latest commodity-terminal screenshot", page.NodeText("#ship-plan-options"));
        Assert.Contains(page.Fetched(), url => url.Contains("/api/routes?scu=6"));
        Assert.Contains("RSI Hermes · 6 SCU · Screenshot/OCR", page.NodeText("#route-ship-context"));
        Assert.Contains("No current prices", page.NodeText("#route-readiness"));
    }

    // ---- the choice itself ----

    /// <summary>
    /// "On foot" is a choice, not the absence of one. The first version
    /// re-picked a ship whenever the saved name was not in the roster, and ''
    /// never is, so the option could be clicked and never kept.
    /// </summary>
    [Fact]
    public void Choosing_no_ship_sticks_across_renders_and_gives_the_per_scu_table()
    {
        var page = new Page();
        page.Serve("/api/routes?scu=0&capital=0&from=&ranking=reliable&freshOnly=false&evidence=reported", "[]");

        page.Do($$"""
            localStorage.store = {};
            libraryStats = { ships: [{{Corsair}}] };
            nowState = { ship: 'Drake Corsair' };
            choosePlanningShip('');
            renderShipPlan();
            renderShipPlan();
            await loadRoutes();
            """);

        Assert.Equal("", page.Text("planningShipName"));
        Assert.Equal("", page.Text("localStorage.getItem('qw-planning-ship')"));
        Assert.Equal("", page.Text("__dom.node('#routes-ship').value"));
        Assert.Contains(page.Fetched(), url => url.Contains("/api/routes?scu=0"));
        Assert.Contains("Choose a flown ship", page.NodeText("#ship-plan-title"));
    }

    [Fact]
    public void A_choice_never_made_falls_to_the_ship_in_use_without_being_written()
    {
        var page = new Page();

        page.Do($$"""
            localStorage.store = {};
            planningShipName = null;
            libraryStats = { ships: [{{Utv}}, {{Corsair}}] };
            nowState = { ship: 'Drake Corsair' };
            renderShipPlan();
            """);

        Assert.Equal("Drake Corsair", page.Text("__dom.node('#routes-ship').value"));
        Assert.Contains("Drake Corsair", page.NodeText("#ship-plan-title"));
        Assert.True(page.Truth("planningShipName === null"));
        Assert.True(page.Truth("localStorage.getItem('qw-planning-ship') === null"));
    }

    /// <summary>
    /// A preset's ship goes through the same door as the selector: kept for
    /// the next load, and the Now page's advice follows it.
    /// </summary>
    [Fact]
    public void A_route_preset_applies_its_ship_the_way_the_selector_does()
    {
        var page = new Page();
        page.Serve("/api/routes?scu=72&capital=5000&from=&ranking=profit&freshOnly=false&evidence=any", "[]");

        page.Do($$"""
            localStorage.store = {};
            libraryStats = { ships: [{{Utv}}, {{Corsair}}] };
            planningShipName = 'Greycat UTV';
            applyRouteProfile({ ship: 'Drake Corsair', capital: '5000', ranking: 'profit', evidence: 'any' });
            await loadRoutes();
            """);

        Assert.Equal("Drake Corsair", page.Text("localStorage.getItem('qw-planning-ship')"));
        Assert.Contains(page.Fetched(), url => url.Contains("/api/routes?scu=72&capital=5000"));
    }
}
