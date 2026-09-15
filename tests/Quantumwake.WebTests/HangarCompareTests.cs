namespace Quantumwake.WebTests;

/// <summary>
/// Two ships side by side on the Hangar: the pair picked there or on Fleet,
/// the figures from the install, the logs, the Garage's sheet and UEX, and
/// the better one for each row lit the right way round - less mass wins,
/// more DPS wins - so a heavier ship is never marked as winning on mass.
/// </summary>
public class HangarCompareTests
{
    private const string Fleet = """
        {"available":true,"ships":[
          {"name":"Aegis Gladius","className":"AEGS_Gladius","sorties":12,"lastFlown":"2026-09-10T20:00:00Z","hours":3.5,
           "beam":16.5,"length":20,"height":5.5,"icon":true,"kind":"Spaceship"},
          {"name":"Drake Corsair","className":"DRAK_Corsair","sorties":40,"lastFlown":"2026-09-01T20:00:00Z","hours":30,
           "beam":30,"length":53,"height":25,"icon":true,"kind":"Spaceship"},
          {"name":"Greycat PTV","className":"GRIN_PTV","sorties":5,"lastFlown":"2026-09-02T20:00:00Z","hours":0.5,
           "beam":2.6,"length":4,"height":2.5,"icon":true,"kind":"Ground"}]}
        """;

    private static string Sheet(string cls, string name, int health, int mass, int cargo, double dps, int em, int available) => SheetTemplate
        .Replace("@CLS@", cls).Replace("@NAME@", name).Replace("@HEALTH@", health.ToString()).Replace("@MASS@", mass.ToString())
        .Replace("@CARGO@", cargo.ToString()).Replace("@DPS@", dps.ToString(System.Globalization.CultureInfo.InvariantCulture))
        .Replace("@EM@", em.ToString()).Replace("@AVAILABLE@", available.ToString());

    private const string SheetTemplate = """
        {"known":true,"dump":"4.10.0-LIVE.12519617",
         "ship":{"class":"@CLS@","name":"@NAME@","manufacturer":"X","role":"Fighter","career":"Combat",
                 "size":2,"crew":1,"hullMass":@MASS@,"health":@HEALTH@,"cargoScu":@CARGO@,"quantumFuel":600,"hydrogenFuel":1600,
                 "flight":{"scm":226,"boost":520,"max":1193,"pitch":68,"yaw":52,"roll":200},"powerPools":{},"stockMass":@MASS@},
         "sheet":{"class":"@CLS@","name":"@NAME@","mass":@MASS@,
                  "shields":{"em":@EM@,"ir":8711,"emByGroup":{}},"quantum":{"em":37164,"ir":7263,"emByGroup":{}},
                  "power":{"available":@AVAILABLE@,"usedShields":14,"usedQuantum":12,"usedByGroup":{},"overShields":false,"overQuantum":false},
                  "cooling":{"generated":68,"usedShields":36.1,"usedQuantum":30.1,"loadShields":0.531,"loadQuantum":0.443},
                  "shield":{"hp":6336,"regen":1394,"generators":2,"poolLimit":2},
                  "weapons":{"fixedDps":@DPS@,"fixedSustainedDps":1186.9,"fixedAlpha":119.4,"turretDps":0,"missileDamage":21000,"missiles":6,"guns":[]},
                  "quantumDrive":{"drive":"Beacon","speed":161410900,"spoolTime":4.4,"cooldown":11.9,"range":32223415682,"fuelPerGm":18.62},
                  "armorSignals":{"em":1.13,"ir":1.13,"crossSection":1},"crossSection":{"x":1,"y":1,"z":1},"parts":[],"notes":[]},
         "ports":[]}
        """;

    private const string Catalogue = """
        [{"name":"Aegis Gladius","career":"Combat","role":"Light Fighter","crew":1,"isSpaceship":true,"expeditedCost":1500,"standardClaimTime":4.2,
          "price":{"price":1200000,"terminal":"Astro Armada"},"rental":null},
         {"name":"Drake Corsair","career":"Multi","role":"Explorer","crew":4,"isSpaceship":true,"expeditedCost":9000,"standardClaimTime":15.5,
          "price":{"price":6800000,"terminal":"New Deal"},"rental":{"vehicle":"Corsair","terminal":"Vantage Rentals","price":150000}}]
        """;

    private static Page Loaded()
    {
        var page = new Page();
        page.Serve("/api/fleet/hangar", Fleet);
        page.Serve("/api/garage/AEGS_Gladius", Sheet("AEGS_Gladius", "Aegis Gladius", 6110, 55646, 0, 1944.5, 22078, 16));
        page.Serve("/api/garage/DRAK_Corsair", Sheet("DRAK_Corsair", "Drake Corsair", 22000, 500000, 72, 3271.6, 40000, 22));
        page.Serve("/api/reference/ships", Catalogue);
        page.Do("__dom.node('#hangar-mode').value = 'scale'; __dom.node('#hangar-sort').value = 'length'; __dom.node('#hangar-zoom').value = '1'; __dom.node('#hangar-layout').value = 'type'; shipPaints = {}; hangarComparison = new Set(); await loadHangar();");
        return page;
    }

    [Fact]
    public void With_no_pair_the_bar_offers_the_roster_and_the_table_stays_hidden()
    {
        var page = Loaded();

        Assert.True(page.Truth("__dom.node('#hangar-compare').hidden"));
        Assert.True(page.Truth("__dom.node('#hangar-compare-clear').hidden"));
        // Every roster ship, ground vehicles included, after the prompt.
        Assert.Equal("|Aegis Gladius|Drake Corsair|Greycat PTV", page.Text("[...__dom.node('#hangar-compare-a').options].map(o => o.value).join('|')"));
    }

    [Fact]
    public void Picking_two_ships_in_the_bar_draws_the_side_by_side_with_the_better_figure_lit()
    {
        var page = Loaded();
        page.Do("__dom.node('#hangar-compare-a').value = 'Aegis Gladius'; await __dom.node('#hangar-compare-a').fire('change'); __dom.node('#hangar-compare-b').value = 'Drake Corsair'; await __dom.node('#hangar-compare-b').fire('change');");

        Assert.Contains("GET /api/garage/AEGS_Gladius", page.Fetched());
        Assert.Contains("GET /api/garage/DRAK_Corsair", page.Fetched());
        Assert.False(page.Truth("__dom.node('#hangar-compare').hidden"));
        Assert.Equal("Aegis Gladius vs Drake Corsair", page.NodeText("#hangar-compare-title"));
        Assert.False(page.Truth("__dom.node('#hangar-compare-clear').hidden"));

        // The deck itself is narrowed to the pair, as before.
        Assert.Equal(2, (int)page.Number("__dom.node('#hangar-canvas').byClass('hangar-ship').length"));

        var cell = (string label, int side) =>
            $"(() => {{ const r = __dom.node('#hangar-compare-table tbody').descendants().filter(n => n.tagName === 'tr').find(n => n.children[0] && n.children[0].textContent === '{label}'); return r.children[{side + 1}]; }})()";

        // Hull HP: more is better, the Corsair's is lit.
        Assert.Equal("6,110", page.Text($"{cell("Hull HP", 0)}.textContent"));
        Assert.Equal("22,000", page.Text($"{cell("Hull HP", 1)}.textContent"));
        Assert.False(page.Truth($"{cell("Hull HP", 0)}.classList.contains('better')"));
        Assert.True(page.Truth($"{cell("Hull HP", 1)}.classList.contains('better')"));
        Assert.Contains("+15,890 (+260%)", page.Text($"{cell("Hull HP", 2)}.textContent"));
        // In the row's own unit: the Corsair fixture carries 72 SCU against none.
        Assert.Equal("+72 SCU", page.Text($"{cell("Cargo", 2)}.textContent"));

        // Mass: less is better, the Gladius is lit even though its number is smaller.
        Assert.True(page.Truth($"{cell("Mass, stock", 0)}.classList.contains('better')"));
        Assert.False(page.Truth($"{cell("Mass, stock", 1)}.classList.contains('better')"));

        // EM: less is better; the Gladius is quieter.
        Assert.True(page.Truth($"{cell("EM, shields up", 0)}.classList.contains('better')"));

        // Size and use carry no verdict: a longer ship is not a better one.
        Assert.Equal("20 m", page.Text($"{cell("Length", 0)}.textContent"));
        Assert.False(page.Truth($"{cell("Length", 1)}.classList.contains('better')"));
        Assert.Equal("40", page.Text($"{cell("Sorties", 1)}.textContent"));

        // Money from UEX: the cheaper buy is lit; a rental only one ship has is shown for it and dashed for the other.
        Assert.True(page.Truth($"{cell("Buy, UEX", 0)}.classList.contains('better')"));
        Assert.Equal("—", page.Text($"{cell("Rent, UEX", 0)}.textContent"));
        Assert.Equal("150,000 aUEC", page.Text($"{cell("Rent, UEX", 1)}.textContent"));

        Assert.Contains("The better figure for its row is lit", page.NodeText("#hangar-compare-note"));
    }

    [Fact]
    public void Clearing_the_pair_restores_the_whole_deck_and_hides_the_table()
    {
        var page = Loaded();
        page.Do("hangarComparison = new Set(['Aegis Gladius', 'Drake Corsair']); renderHangar(); await renderHangarComparison();");
        Assert.False(page.Truth("__dom.node('#hangar-compare').hidden"));

        page.Do("await __dom.node('#hangar-compare-clear').fire('click');");

        Assert.True(page.Truth("__dom.node('#hangar-compare').hidden"));
        Assert.Equal(3, (int)page.Number("__dom.node('#hangar-canvas').byClass('hangar-ship').length"));
        Assert.Equal("", page.Text("__dom.node('#hangar-compare-a').value"));
    }

    /// <summary>The same ship twice is one ship: the second pick is refused rather than drawn against itself.</summary>
    [Fact]
    public void The_same_ship_cannot_be_compared_with_itself()
    {
        var page = Loaded();
        page.Do("__dom.node('#hangar-compare-a').value = 'Drake Corsair'; await __dom.node('#hangar-compare-a').fire('change'); __dom.node('#hangar-compare-b').value = 'Drake Corsair'; await __dom.node('#hangar-compare-b').fire('change');");

        Assert.Equal(1, (int)page.Number("hangarComparison.size"));
        Assert.True(page.Truth("__dom.node('#hangar-compare').hidden"));
    }

    /// <summary>
    /// A hull the dataset cannot draw still gets its size and use; the note
    /// says why the rest is missing rather than leaving a column of dashes.
    /// </summary>
    [Fact]
    public void A_hull_without_a_sheet_keeps_size_and_use_and_says_why()
    {
        var page = Loaded();
        page.Serve("/api/garage/DRAK_Corsair", """{"known":false,"message":"The reference data predates the garage."}""");
        page.Do("compareSheets.clear(); hangarComparison = new Set(['Aegis Gladius', 'Drake Corsair']); renderHangar(); await renderHangarComparison();");

        var rows = page.Text("__dom.node('#hangar-compare-table tbody').descendants().filter(n => n.tagName === 'tr' && !n.classList.contains('compare-group')).map(n => n.children[0].textContent).join('|')");
        Assert.Contains("Length", rows);
        Assert.Contains("Hull HP", rows);
        Assert.Contains("Drake Corsair: the community dataset cannot draw its sheet", page.NodeText("#hangar-compare-note"));
        Assert.Equal("—", page.Text("(() => { const r = __dom.node('#hangar-compare-table tbody').descendants().filter(n => n.tagName === 'tr').find(n => n.children[0].textContent === 'Hull HP'); return r.children[2].textContent; })()"));
    }
    /// <summary>
    /// A compared pair is drawn to fit: both on one shelf whatever the zoom,
    /// and no bigger than the pictures can stand. A hull wider than it is
    /// long used to overrun its half of the deck and push the other ship
    /// under the fold, which read as the comparison showing one ship.
    /// </summary>
    [Fact]
    public void A_compared_pair_shares_one_shelf_at_a_capped_scale_whatever_the_zoom()
    {
        var page = Loaded();
        // A hull wider than long, like the Hermes, and 4x zoom asked for.
        page.Serve("/api/fleet/hangar", Fleet.Replace("\"beam\":16.5,\"length\":20", "\"beam\":73,\"length\":58"));
        page.Do("__dom.node('#hangar-zoom').value = '4'; hangarComparison = new Set(['Aegis Gladius', 'Drake Corsair']); await loadHangar();");

        var ys = page.Text("__dom.node('#hangar-canvas').byClass('hangar-ship').map(g => g.getAttribute('transform').split(' ')[1]).join('|')");
        Assert.Equal(2, ys.Split('|').Length);
        Assert.Equal(ys.Split('|')[0], ys.Split('|')[1]);

        // 73 m across at the capped scale is at most 384 px; zoom is not offered.
        var tallest = page.Number("Math.max(...__dom.node('#hangar-canvas').byClass('hangar-ship').map(g => Number(g.querySelector('image').getAttribute('height'))))");
        Assert.True(tallest <= 384.5, $"tallest {tallest}");
        Assert.True(page.Truth("__dom.node('#hangar-zoom').hidden"));
        Assert.Contains("Compared · 2", page.NodeText("#hangar-canvas"));
    }

    /// <summary>
    /// Comparing still means comparing when only one chosen hull has a
    /// top-down icon. The missing hull is called out below; it must not make
    /// the visible ship ignore the comparison cap and turn into a blur.
    /// </summary>
    [Fact]
    public void A_compared_ship_without_an_icon_keeps_its_visible_partner_compact()
    {
        var page = Loaded();
        page.Serve("/api/fleet/hangar", Fleet.Replace("\"length\":53,\"height\":25,\"icon\":true", "\"length\":53,\"height\":25,\"icon\":false"));
        page.Do("__dom.node('#hangar-zoom').value = '4'; hangarComparison = new Set(['Aegis Gladius', 'Drake Corsair']); await loadHangar();");

        Assert.Equal(1, (int)page.Number("__dom.node('#hangar-canvas').byClass('hangar-ship').length"));
        var tallest = page.Number("Math.max(...__dom.node('#hangar-canvas').byClass('hangar-ship').map(g => Number(g.querySelector('image').getAttribute('height'))))");
        Assert.True(tallest <= 384.5, $"tallest {tallest}");
        Assert.True(page.Truth("__dom.node('#hangar-zoom').hidden"));
        Assert.Contains("Compared · 1", page.NodeText("#hangar-canvas"));
        Assert.Contains("Drake Corsair", page.NodeText("#hangar-unsized"));
    }
}
