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
}
