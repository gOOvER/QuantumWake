namespace Quantumwake.WebTests;

/// <summary>
/// The Garage sheet: a ship's numbers, grouped, each group naming where it
/// came from, and the caveats said beside the figures they qualify.
/// </summary>
public class GaragePageTests
{
    private const string Garage = """
        {"known":true,"dump":"4.10.0-LIVE.12519617",
         "mine":[{"class":"AEGS_Gladius","name":"Aegis Gladius","sorties":12,"lastFlown":"2026-09-13T00:00:00+00:00"}],
         "all":[{"class":"AEGS_Gladius","name":"Aegis Gladius","manufacturer":"Aegis Dynamics","role":"Light Fighter","size":2},
                {"class":"DRAK_Cutlass_Black","name":"Drake Cutlass Black","manufacturer":"Drake Interplanetary","role":"Medium Fighter","size":3}]}
        """;

    private const string Gladius = """
        {"known":true,"dump":"4.10.0-LIVE.12519617",
         "ship":{"class":"AEGS_Gladius","name":"Aegis Gladius","manufacturer":"Aegis Dynamics","role":"Light Fighter","career":"Combat",
                 "size":2,"crew":1,"hullMass":48552,"health":6110,"cargoScu":0,"quantumFuel":600,"hydrogenFuel":1600,
                 "flight":{"scm":226,"boost":520,"max":1193,"pitch":68,"yaw":52,"roll":200},"powerPools":{"Shield":2,"WeaponGun":4},"stockMass":55646},
         "sheet":{"class":"AEGS_Gladius","name":"Aegis Gladius","mass":55646,
                  "shields":{"em":22078,"ir":8711,"emByGroup":{"PowerPlant":13480,"Cooler":3367,"Shield":2802,"Radar":2034,"LifeSupportGenerator":354,"WeaponGun":41}},
                  "quantum":{"em":37164,"ir":7263,"emByGroup":{}},
                  "power":{"available":16,"usedShields":24.1,"usedQuantum":18.1,"usedByGroup":{},"overShields":true,"overQuantum":true},
                  "cooling":{"generated":68,"usedShields":36.1,"usedQuantum":30.1,"loadShields":0.531,"loadQuantum":0.443},
                  "shield":{"hp":6336,"regen":1394,"generators":2,"poolLimit":2},
                  "weapons":{"fixedDps":1944.5,"fixedSustainedDps":1186.9,"fixedAlpha":119.4,"turretDps":0,"missileDamage":21000,"missiles":6,
                             "guns":["CF-337 Panther Repeater","CF-337 Panther Repeater","Mantis GT-220 Gatling"]},
                  "quantumDrive":{"drive":"Beacon","speed":161410900,"spoolTime":4.4,"cooldown":11.9,"range":32223415682,"fuelPerGm":18.62},
                  "armorSignals":{"em":1.13,"ir":1.13,"crossSection":1},
                  "crossSection":{"x":6923,"y":1731,"z":8654},
                  "parts":[],"notes":["Draws 24.1 power segments with shields up; the plant provides 16. The game will brown out something."]},
         "ports":[{"portId":"p1","hardpoint":"Hardpoint_cooler_left","group":"Cooler","minSize":1,"maxSize":1,"class":"COOL_AEGS_S01_Bracer_SCItem","name":"Bracer","stockClass":"COOL_AEGS_S01_Bracer_SCItem","changed":false,
                   "fitted":{"type":"Cooler","name":"Bracer","em":1490,"ir":7260,"coolantGen":25}},
                  {"portId":"p2","hardpoint":"hardpoint_quantum_drive","group":"QuantumDrive","minSize":1,"maxSize":1,"class":"QDRV_WETK_S01_Beacon_SCItem","name":"Beacon","stockClass":"QDRV_WETK_S01_Beacon_SCItem","changed":false,
                   "fitted":{"type":"QuantumDrive","name":"Beacon","em":15000,"ir":0,"quantum":{"speed":161410900,"spoolTime":4.4}}}]}
        """;

    private static Page Opened()
    {
        var page = new Page();
        page.Serve("/api/garage", Garage);
        page.Serve("/api/garage/AEGS_Gladius", Gladius);
        page.Do("await loadGarage();");
        return page;
    }

    [Fact]
    public void The_most_flown_ship_opens_first_and_the_pickers_name_it()
    {
        var page = Opened();

        Assert.Contains("GET /api/garage/AEGS_Gladius", page.Fetched());
        Assert.Equal("Aegis Gladius", page.NodeText("#garage-title"));
        Assert.Contains("Light Fighter", page.NodeText("#garage-sub"));
        Assert.Contains("4.10.0-LIVE.12519617", page.NodeText("#garage-source"));
        Assert.Equal("AEGS_Gladius", page.Text("__dom.node('#garage-mine').value"));
    }

    [Fact]
    public void Every_group_is_drawn_and_names_its_source()
    {
        var page = Opened();
        var text = page.NodeText("#garage-sheet");

        foreach (var group in new[] { "Hull", "Flight", "Weapons", "Defence", "Signature", "Systems", "Quantum" })
            Assert.Contains(group, text);

        Assert.Contains("signature model", text);
        Assert.Contains("ships.json", text);
    }

    /// <summary>The figures a stealth pilot came for, with the scenario stated beside them.</summary>
    [Fact]
    public void Signatures_carry_their_scenario_and_their_breakdown()
    {
        var page = Opened();
        var text = page.NodeText("#garage-sheet");

        Assert.Contains("22,078", text);
        Assert.Contains("8,711", text);
        Assert.Contains("quantum drive idle", text);
        Assert.Contains("Power plant", text);
        Assert.Contains("13,480", text);
        Assert.Contains("EM ×1.13", text);
    }

    [Fact]
    public void Cross_section_is_shown_as_geometry_that_parts_do_not_move()
    {
        var page = Opened();
        var text = page.NodeText("#garage-sheet");

        Assert.Contains("6,923", text);
        Assert.Contains("parts do not change it", text);
    }

    [Fact]
    public void Quantum_and_speed_are_in_the_units_a_pilot_reads()
    {
        var page = Opened();
        var text = page.NodeText("#garage-sheet");

        Assert.Contains("161,411 km/s", text);
        Assert.Contains("32.2 Gm", text);
        Assert.Contains("Beacon", text);
    }

    /// <summary>A budget the ship cannot meet is said on the row and in the notes, not left as a number.</summary>
    [Fact]
    public void An_over_budget_ship_says_so_twice()
    {
        var page = Opened();

        Assert.Contains("Over budget", page.NodeText("#garage-sheet"));
        Assert.False(page.Truth("__dom.node('#garage-notes').hidden"));
        Assert.Contains("brown out", page.NodeText("#garage-notes"));
    }

    [Fact]
    public void The_ports_table_lists_what_is_fitted_and_what_it_does()
    {
        var page = Opened();
        var text = page.NodeText("#garage-ports");

        Assert.Contains("cooler left", text);
        Assert.Contains("Bracer", text);
        Assert.Contains("25 coolant", text);
        Assert.Contains("IR 7,260", text);
        Assert.Contains("Quantum drive", text);
    }

    [Fact]
    public void A_reference_that_predates_the_garage_asks_for_a_refresh()
    {
        var page = new Page();
        page.Serve("/api/garage", """{"known":false,"dump":null,"mine":[],"all":[]}""");
        page.Do("await loadGarage();");

        Assert.False(page.Truth("__dom.node('#garage-unavailable').hidden"));
        Assert.Contains("Refresh the community dataset", page.NodeText("#garage-unavailable"));
        Assert.DoesNotContain("GET /api/garage/", string.Join("|", page.Fetched()));
    }

    /// <summary>
    /// With a stock sheet to compare against, a figure that moved shows the old
    /// one struck beside it, coloured by whether it got better for its kind:
    /// less IR is better, less DPS is not.
    /// </summary>
    [Fact]
    public void A_changed_figure_shows_what_it_was_and_which_way_it_went()
    {
        var page = Opened();
        page.Do("""
            const swapped = JSON.parse(JSON.stringify(garageStock));
            swapped.sheet.shields.ir = 8502;
            swapped.sheet.weapons.fixedDps = 1500;
            renderGarage(swapped, garageStock);
            """);

        var rows = "__dom.node('#garage-sheet').descendants().filter(n => n.classList.contains('sheet-row'))";
        var ir = $"{rows}.find(r => r.dataset.key === 'IR, shields up')";
        Assert.Contains("8,711", page.Text($"{ir}.textContent"));
        Assert.Contains("8,502", page.Text($"{ir}.textContent"));
        Assert.True(page.Truth($"{ir}.descendants().some(n => n.classList.contains('v') && n.classList.contains('up'))"));

        var dps = $"{rows}.find(r => r.dataset.key === 'Pilot DPS')";
        Assert.True(page.Truth($"{dps}.descendants().some(n => n.classList.contains('v') && n.classList.contains('down'))"));
    }
}
