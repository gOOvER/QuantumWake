namespace Quantumwake.WebTests;

public class MapClosestTests
{
    private static Page Loaded()
    {
        var page = new Page();
        page.Do("""
            atlas = [
              { rawId: 'here', name: 'Home', system: 'Stanton', body: 'Origin', kind: 'City', visits: 1 },
              { rawId: 'far', name: 'Far refinery', system: 'Stanton', body: 'Far', kind: 'RestStop', visits: 0 },
              { rawId: 'near', name: 'Near refinery', system: 'Stanton', body: 'Near', kind: 'RestStop', visits: 0 },
              { rawId: 'other', name: 'Other system', system: 'Pyro', body: 'Origin', kind: 'RestStop', visits: 0 }
            ];
            hereId = 'here';
            bodyPositions = { stanton: { Origin: { x: 0, y: 0 }, Near: { x: 3, y: 4 }, Far: { x: 0, y: 6 } } };
            for (const location of atlas.slice(1)) amenitiesByPlace.set(location.name.toLowerCase(), ['Refinery', 'Cargo']);
            performance = { now: () => 0 };
            """);
        return page;
    }

    [Fact]
    public void Uses_body_geometry_in_the_players_system_not_display_positions()
    {
        var page = Loaded();
        page.Do("nodeAt.set('near', { x: 999, y: 999 }); nodeAt.set('far', { x: 0, y: 0 });");
        Assert.Equal("near", page.Text("closestMapFacilities('Refinery').locations[0].rawId"));
        Assert.Contains("Approximate", page.Text("closestMapFacilities('Refinery').message"));
    }

    [Fact]
    public void Current_place_wins_when_it_has_the_facility()
    {
        var page = Loaded();
        page.Do("amenitiesByPlace.set('home', ['Cargo']); bodyPositions = {};");
        Assert.Equal("here", page.Text("closestMapFacilities('Cargo').locations[0].rawId"));
        Assert.DoesNotContain("Approximate", page.Text("closestMapFacilities('Cargo').message"));
    }

    [Fact]
    public void Same_body_facilities_remain_tied_without_invented_distances()
    {
        var page = Loaded();
        page.Do("atlas[1].body = 'Origin'; atlas[2].body = 'Origin'; bodyPositions = {};");
        Assert.Equal(2, page.Count("closestMapFacilities('Cargo').locations.length"));
        Assert.Contains("tied", page.Text("closestMapFacilities('Cargo').message"));
    }

    [Fact]
    public void Missing_coordinates_are_reported_and_not_treated_as_zero()
    {
        var page = Loaded();
        page.Do("delete bodyPositions.stanton.Far;");
        Assert.Equal("near", page.Text("closestMapFacilities('Refinery').locations[0].rawId"));
        Assert.Contains("could not be compared", page.Text("closestMapFacilities('Refinery').message"));
        page.Do("bodyPositions = {};");
        Assert.Contains("Cannot determine", page.Text("closestMapFacilities('Refinery').message"));
    }

    [Fact]
    public void Missing_origin_choice_and_matches_each_explain_what_is_missing()
    {
        var page = Loaded();
        Assert.Contains("Choose a facility", page.Text("closestMapFacilities('').message"));
        Assert.Contains("No Hospital", page.Text("closestMapFacilities('Hospital').message"));
        page.Do("hereId = null;");
        Assert.Contains("location is unavailable", page.Text("closestMapFacilities('Refinery').message"));
    }

    [Fact]
    public void Service_choices_use_the_service_feed()
    {
        var page = Loaded();
        page.Do("mapServicesByPlace.set('near', ['refuel']);");
        Assert.Equal("near", page.Text("closestMapFacilities('', 'refuel').locations[0].rawId"));
    }

    [Fact]
    public void Cargo_labels_are_readable_and_keep_the_install_facility_key()
    {
        var page = Loaded();
        page.Serve("/api/map/amenities", """
            [{"place":"Near refinery","amenities":["Commodity Trading - Freight Elevator"]}]
            """);
        page.Do("amenitiesByPlace.clear(); await loadMapAmenities();");
        Assert.Equal("Cargo · Freight elevator (1)", page.Text("__dom.node('#map-amenity').options[0].textContent"));
        Assert.Equal("near", page.Text("closestMapFacilities(__dom.node('#map-amenity').options[0].value).locations[0].rawId"));
    }

    [Fact]
    public void Tied_results_wait_for_a_choice_and_remain_available_after_selection()
    {
        var page = Loaded();
        page.Do("""
            showMapInfo = (location) => { globalThis.__opened = location.rawId; };
            globalThis.__opened = '';
            atlas[1].body = 'Origin'; atlas[2].body = 'Origin';
            mapAmenityFilter = 'Refinery';
            selectClosestMapFacility();
            """);
        Assert.Equal("", page.Text("__opened"));
        Assert.Equal(2, page.Count("__dom.node('#map-closest-result').querySelectorAll('button').length"));
        page.Do("__dom.node('#map-closest-result').querySelectorAll('button')[1].click();");
        Assert.Equal("near", page.Text("__opened"));
        Assert.Equal(2, page.Count("__dom.node('#map-closest-result').querySelectorAll('button').length"));
        Assert.False(page.Truth("__dom.node('#map-closest-result').hidden"));
    }

    [Fact]
    public void Repeated_service_selection_keeps_the_choice_and_open_card()
    {
        var page = Loaded();
        page.Do("""
            showMapInfo = (location) => {
              globalThis.__opened = (globalThis.__opened || 0) + 1;
              entityShown = `place|${location.rawId}`;
              __dom.node('#entity-drawer').hidden = false;
            };
            mapServicesByPlace.set('near', ['refuel']);
            mapServiceFilter = 'refuel';
            selectClosestMapFacility();
            selectClosestMapFacility();
            """);
        Assert.Equal("refuel", page.Text("mapServiceFilter"));
        Assert.Equal(1, page.Count("__opened"));
    }

    [Fact]
    public void Button_reveals_unvisited_result_in_its_system_and_opens_its_card()
    {
        var page = Loaded();
        page.Do("""
            initMap();
            showMapInfo = (location) => { globalThis.__opened = location.rawId; };
            mapAmenityFilter = 'Cargo';
            mapServiceFilter = 'clinic';
            mapFocusFilter = 'plan';
            __dom.node('#map-mode').value = 'network';
            __dom.node('#map-system').value = 'Pyro';
            __dom.node('#map-visited-only').checked = true;
            __dom.node('#map-search').value = 'unrelated';
            __dom.node('#map-closest').click();
            """);
        Assert.Equal("near", page.Text("__opened"));
        Assert.Equal("Stanton", page.Text("mapSystem()"));
        Assert.Equal("system", page.Text("mapMode()"));
        Assert.True(page.Truth("nodeAt.has('near')"));
        Assert.Contains("Home (last known location)", page.NodeText("#map-closest-result"));
        page.Do("setHere('far');");
        Assert.True(page.Truth("__dom.node('#map-closest-result').hidden"));
    }
}
