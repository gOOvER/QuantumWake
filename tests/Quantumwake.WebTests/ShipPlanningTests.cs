namespace Quantumwake.WebTests;

/// <summary>
/// Planning starts with the hull, so a generic price lead cannot masquerade as
/// a cargo run a ground vehicle (or a ship without a grid) can make.
/// </summary>
public class ShipPlanningTests
{
    [Fact]
    public void A_selected_cargo_ship_sizes_routes_and_names_its_supported_options()
    {
        var page = new Page();
        page.Serve("/api/routes?scu=72&capital=0&from=&ranking=reliable&freshOnly=false&evidence=reported", "[]");

        page.Do("""
            libraryStats = { ships: [
              { name: 'Drake Corsair', className: 'DRAK_Corsair', lastFlown: '2026-09-13T00:00:00Z' }
            ] };
            shipCatalogue = [{ name: 'Drake Corsair', cargoScu: 72, crew: 4, isSpaceship: true }];
            planningShipName = 'Drake Corsair';
            renderShipPlan();
            await loadRoutes();
            """);

        Assert.Contains("Drake Corsair", page.NodeText("#ship-plan-title"));
        Assert.Contains("72 SCU cargo grid", page.NodeText("#ship-plan-options"));
        Assert.Contains("Crew", page.NodeText("#ship-plan-options"));
        Assert.Contains(page.Fetched(), url => url.Contains("/api/routes?scu=72"));
    }

    [Fact]
    public void A_ground_vehicle_hides_trade_routes_instead_of_pricing_an_impossible_haul()
    {
        var page = new Page();
        page.Do("""
            libraryStats = { ships: [
              { name: 'Greycat UTV', className: 'GRIN_UTV', lastFlown: '2026-09-13T00:00:00Z' }
            ] };
            shipCatalogue = [{ name: 'Greycat UTV', cargoScu: 0, crew: 1, isSpaceship: false }];
            planningShipName = 'Greycat UTV';
            renderShipPlan();
            await loadRoutes();
            """);

        Assert.Contains("Ground vehicle", page.NodeText("#ship-plan-options"));
        Assert.Contains("cargo routes are hidden", page.NodeText("#routes-table tbody"));
        Assert.DoesNotContain(page.Fetched(), url => url.StartsWith("/api/routes?", StringComparison.Ordinal));
    }

    [Fact]
    public void A_spaceship_without_the_optional_catalogue_can_use_a_pilot_verified_local_hold_size()
    {
        var page = new Page();
        page.Serve("/api/routes?scu=6&capital=0&from=&ranking=reliable&freshOnly=false&evidence=reported", "[]");

        page.Do("""
            libraryStats = { ships: [{ name: 'RSI Hermes', className: 'RSI_Hermes', lastFlown: '2026-09-13T00:00:00Z' }] };
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

        page.Do("""
            libraryStats = { ships: [{ name: 'RSI Hermes', className: 'RSI_Hermes', lastFlown: '2026-09-13T00:00:00Z' }] };
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
}
