namespace Quantumwake.WebTests;

/// <summary>
/// What the star map itself says about a place, on its detail card.
/// </summary>
/// <remarks>
/// The install names a parent for 1,977 map objects and lists services at 257
/// of them. Those services are not the service chips above them on the card:
/// those come from UEX and say where you can trade today, while these are what
/// the game says the place has.
/// </remarks>
public class MapPlaceCardTests
{
    private static Page Opened(string body)
    {
        var page = new Page();
        page.Serve("/api/map/lore?name=Lorville", body);
        page.Do("""
            mapInfoLocation = { rawId: 'LORVILLE', name: 'Lorville' };
            await renderMapInfoPlace(mapInfoLocation);
            """);
        return page;
    }

    [Fact]
    public void The_paragraph_and_the_services_both_show()
    {
        var page = Opened("""
            {"lore":"The capital of Hurston.","parent":"Hurston","kind":"LandingZone",
             "amenities":["Hospital","Buy Armor"]}
            """);

        Assert.Contains("The capital of Hurston.", page.NodeText("#map-info-lore"));
        Assert.Contains("Hospital", page.NodeText("#map-info-amenities"));
        Assert.Contains("Buy Armor", page.NodeText("#map-info-amenities"));
    }

    /// <summary>
    /// What a place orbits is worth a few words rather than a field of its own.
    /// </summary>
    [Fact]
    public void The_parent_is_named_beside_the_services()
    {
        Assert.Contains("In Hurston", Opened("""
            {"lore":null,"parent":"Hurston","kind":"LandingZone","amenities":["Hospital"]}
            """).NodeText("#map-info-amenities"));
    }

    /// <summary>
    /// A place the game says nothing about must leave both rows away rather
    /// than showing an empty one.
    /// </summary>
    [Fact]
    public void A_place_with_nothing_known_shows_neither_row()
    {
        var page = Opened("""{"lore":null,"parent":null,"kind":null,"amenities":[]}""");

        Assert.True(page.Truth("__dom.node('#map-info-lore').hidden"));
        Assert.True(page.Truth("__dom.node('#map-info-amenities').hidden"));
    }

    /// <summary>
    /// A selected place is a decision point: it says whether the route already
    /// reaches it, and offers only support that UEX does not list there. The
    /// missing labels deliberately say "not listed" in the UI rather than
    /// claiming a facility is absent from the game.
    /// </summary>
    [Fact]
    public void The_operational_readout_combines_route_state_and_missing_support()
    {
        var page = new Page();

        page.Do("""
            const place = { rawId: 'clinic', name: 'Seraphim' };
            trips = [{ tracked: true, title: 'Hurston service run', stops: [
              { placeId: 'clinic', place: 'Seraphim', done: false, actions: [] },
              { placeId: 'shop', place: 'Area18', done: false, actions: [] }
            ] }];
            mapServicesByPlace.set('clinic', ['clinic']);
            renderMapInfoServices(place);
            renderMapInfoPlan(place);
            renderMapInfoData();
            """);

        Assert.Contains("Find refuel", page.NodeText("#map-info-support"));
        Assert.Contains("Find repair", page.NodeText("#map-info-support"));
        Assert.DoesNotContain("Find clinic", page.NodeText("#map-info-support"));
        Assert.Contains("Hurston service run", page.NodeText("#map-info-plan"));
        Assert.Contains("Open flight plan", page.NodeText("#map-info-plan"));
        Assert.Contains("No reported price", page.NodeText("#map-info-data"));
        Assert.Equal("Next stop", page.NodeText("#map-place-plan-state"));
    }
}
