namespace Quantumwake.WebTests;

/// <summary>
/// The photographed fit on the Garage: offered, dated, and applied only when
/// the pilot says so - because a screenshot is a moment and the ports are
/// matched to it by order, not read from it.
/// </summary>
public class GaragePhotoTests
{
    private const string Garage = """
        {"known":true,"dump":"4.10.0-LIVE.12519617",
         "mine":[{"class":"AEGS_Gladius","name":"Aegis Gladius","sorties":12,"lastFlown":"2026-09-13T00:00:00+00:00"}],
         "all":[{"class":"AEGS_Gladius","name":"Aegis Gladius","manufacturer":"Aegis Dynamics","role":"Light Fighter","size":2}]}
        """;

    private const string Gladius = """
        {"known":true,"dump":"4.10.0-LIVE.12519617",
         "ship":{"class":"AEGS_Gladius","name":"Aegis Gladius","manufacturer":"Aegis Dynamics","role":"Light Fighter","career":"Combat",
                 "size":2,"crew":1,"hullMass":48552,"health":6110,"cargoScu":0,"quantumFuel":600,"hydrogenFuel":1600,
                 "flight":{"scm":226,"boost":520,"max":1193,"pitch":68,"yaw":52,"roll":200},"powerPools":{},"stockMass":55646},
         "sheet":{"class":"AEGS_Gladius","name":"Aegis Gladius","mass":55646,
                  "shields":{"em":22078,"ir":8711,"emByGroup":{}},"quantum":{"em":37164,"ir":7263,"emByGroup":{}},
                  "power":{"available":16,"usedShields":14,"usedQuantum":12,"usedByGroup":{},"overShields":false,"overQuantum":false},
                  "cooling":{"generated":68,"usedShields":36.1,"usedQuantum":30.1,"loadShields":0.531,"loadQuantum":0.443},
                  "shield":{"hp":6336,"regen":1394,"generators":2,"poolLimit":2},
                  "weapons":{"fixedDps":1944.5,"fixedSustainedDps":1186.9,"fixedAlpha":119.4,"turretDps":0,"missileDamage":21000,"missiles":6,"guns":[]},
                  "quantumDrive":{"drive":"Beacon","speed":161410900,"spoolTime":4.4,"cooldown":11.9,"range":32223415682,"fuelPerGm":18.62},
                  "armorSignals":{"em":1.13,"ir":1.13,"crossSection":1},"crossSection":{"x":6923,"y":1731,"z":8654},"parts":[],"notes":[]},
         "ports":[{"portId":"p1","hardpoint":"Hardpoint_cooler_left","group":"Cooler","minSize":1,"maxSize":1,"class":"COOL_AEGS_S01_Bracer_SCItem","name":"Bracer","stockClass":"COOL_AEGS_S01_Bracer_SCItem","changed":false,
                   "fitted":{"type":"Cooler","name":"Bracer","size":1,"grade":2,"manufacturer":"Aegis Dynamics","makerCode":"AEGS","em":1490,"ir":7260,"coolantGen":25,"powerUseMax":3}}]}
        """;

    private const string Photographed = """
        {"shot":"ScreenShot-2026-09-12_21-55-29-D6C.jpg","shotAt":"2026-09-12T21:55:29+00:00","ship":"Aegis Gladius","scope":null,
         "swaps":{"p1":"COOL_JUST_S01_Glacier_SCItem"},"applied":1,"changed":1,
         "ports":[{"slot":"Cooler 1","portId":"p1","name":"Glacier","class":"COOL_JUST_S01_Glacier_SCItem","applied":true,"changed":true,"why":null,"stockName":"Bracer"},
                  {"slot":"Weapon 1","portId":"p-gun","name":null,"class":null,"applied":false,"changed":false,"why":"read “MBA Cannon”, which names no one part","stockName":"CF-337 Panther Repeater"}]}
        """;

    private static Page Opened(string? photographed)
    {
        var page = new Page();
        page.Serve("/api/garage", Garage);
        page.Serve("/api/garage/AEGS_Gladius", Gladius);
        page.Serve("/api/garage/builds?ship=AEGS_Gladius", "[]");
        page.Serve("/api/garage/AEGS_Gladius/market", """{"enabled":false,"fetchedAt":null,"total":0,"components":0,"listings":[]}""");
        if (photographed is not null) page.Serve("/api/garage/AEGS_Gladius/photographed", photographed);
        page.Do("await loadGarage();");
        return page;
    }

    [Fact]
    public void Without_a_reading_of_the_ship_nothing_is_offered()
    {
        var page = Opened(null);

        Assert.Contains("GET /api/garage/AEGS_Gladius/photographed", page.Fetched());
        Assert.True(page.Truth("__dom.node('#garage-photo').hidden"));
        Assert.Equal("Stock fit", page.NodeText("#garage-changes"));
    }

    /// <summary>
    /// The offer is dated and counts what the screenshot settled and what it
    /// did not, port by port with the reason - and the bench stays stock
    /// until the pilot clicks.
    /// </summary>
    [Fact]
    public void A_reading_is_offered_dated_with_what_it_did_not_settle()
    {
        var page = Opened(Photographed);

        Assert.False(page.Truth("__dom.node('#garage-photo').hidden"));
        Assert.Contains("Aegis Gladius, as photographed", page.NodeText("#garage-photo-title"));
        var sub = page.NodeText("#garage-photo-sub");
        Assert.Contains("1 of 2 ports read as one part", sub);
        Assert.Contains("1 differs from stock", sub);
        Assert.Contains("1 not settled by the screenshot and left as stock", sub);
        Assert.Contains("a moment, not a state", sub);
        Assert.Contains("Weapon 1 · read “MBA Cannon”, which names no one part", page.NodeText("#garage-photo-unsettled"));

        Assert.Equal("Stock fit", page.NodeText("#garage-changes"));
        Assert.DoesNotContain("POST /api/garage/AEGS_Gladius/sheet", page.Fetched());
        Assert.False(page.Truth("__dom.node('#garage-photo-apply').disabled"));
    }

    [Fact]
    public void Starting_from_it_posts_the_swaps_and_reset_takes_it_back()
    {
        var page = Opened(Photographed);
        page.Serve("/api/garage/AEGS_Gladius/sheet", Gladius
            .Replace("\"ir\":8711", "\"ir\":8502")
            .Replace("\"name\":\"Bracer\",\"stockClass\":\"COOL_AEGS_S01_Bracer_SCItem\",\"changed\":false",
                "\"class\":\"COOL_JUST_S01_Glacier_SCItem\",\"name\":\"Glacier\",\"stockName\":\"Bracer\",\"stockClass\":\"COOL_AEGS_S01_Bracer_SCItem\",\"changed\":true"));

        page.Do("await __dom.node('#garage-photo-apply').fire('click');");

        Assert.Contains("POST /api/garage/AEGS_Gladius/sheet", page.Fetched());
        Assert.Contains("\"p1\":\"COOL_JUST_S01_Glacier_SCItem\"", page.BodyOf("/api/garage/AEGS_Gladius/sheet"));
        Assert.Equal("1 part changed", page.NodeText("#garage-changes"));
        Assert.True(page.Truth("__dom.node('#garage-photo-apply').disabled"));
        Assert.Contains("started from this photograph", page.NodeText("#garage-photo-state"));

        page.Serve("/api/garage/AEGS_Gladius/sheet", Gladius);
        page.Do("await resetGarage();");

        Assert.Equal("Stock fit", page.NodeText("#garage-changes"));
        Assert.False(page.Truth("__dom.node('#garage-photo-apply').disabled"));
        Assert.Equal("", page.NodeText("#garage-photo-state"));
    }

    /// <summary>A reading that settled nothing the bench changes is shown for what it is, with no button to press.</summary>
    [Fact]
    public void A_reading_that_settled_nothing_has_no_button_to_press()
    {
        var page = Opened(Photographed
            .Replace("\"swaps\":{\"p1\":\"COOL_JUST_S01_Glacier_SCItem\"},\"applied\":1,\"changed\":1", "\"swaps\":{},\"applied\":0,\"changed\":0")
            .Replace("\"applied\":true,\"changed\":true,\"why\":null", "\"applied\":false,\"changed\":false,\"why\":\"nothing read under it\""));

        Assert.True(page.Truth("__dom.node('#garage-photo-apply').disabled"));
        Assert.Contains("Nothing on the screenshot settled a port", page.NodeText("#garage-photo-state"));
    }
}
