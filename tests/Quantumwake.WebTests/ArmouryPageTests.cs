namespace Quantumwake.WebTests;

/// <summary>
/// The Armoury page: guns and armour from the install, priced by UEX,
/// with every derived figure called derived and the class-wide resistance
/// said once rather than repeated as if it were a finding.
/// </summary>
public class ArmouryPageTests
{
    private const string Model = """
        {"ready":true,"itemPricesKnown":true,"picturesKnown":true,
         "counts":{"weapons":327,"plain":3,"armour":2349,"sets":3},
         "weapons":[
           {"class":"behr_rifle_ballistic_01","uuid":"02d4cd2e-fa98-4086-aee1-6b2dfce8ea27","name":"P4-AR Rifle","kind":"Rifle","weight":"Medium","size":2,"manufacturer":"Behring",
            "damage":{"physical":12,"energy":0,"distortion":0,"thermal":0,"biochemical":0,"stun":0,"total":12,"dominant":"physical"},
            "projectileSpeed":550,"projectileLifetime":2,"dropStart":40,"dropPerMetre":0.05,"dropFloor":10,"floorAt":80,"magazine":40,"magazineClass":"behr_rifle_ballistic_01_mag","mass":2.65,
            "modes":[{"name":"AUTO","kind":"Auto","roundsPerMinute":810,"pellets":1,"ammoPerShot":1,"burstShots":0,"heatPerShot":0,"condition":"","hit":{"physical":12,"total":12},"sustainedRoundsPerMinute":810,"damagePerShot":12,"damagePerSecond":162,"damagePerMagazine":480,"secondsToEmpty":2.96},
                     {"name":"SEMI","kind":"Semi","roundsPerMinute":810,"pellets":1,"ammoPerShot":1,"burstShots":0,"heatPerShot":0,"condition":"","hit":{"physical":12,"total":12},"sustainedRoundsPerMinute":810,"damagePerShot":12,"damagePerSecond":162,"damagePerMagazine":480,"secondsToEmpty":2.96}],
            "market":{"price":4138,"shops":[{"terminal":"Guns Rod's Fuel","place":"Rod's Fuel & Supplies","system":"Pyro","price":4138},{"terminal":"Live Fire ARC-L1","place":"ARC-L1","system":"Stanton","price":4200}]},
            "finishes":[{"class":"behr_rifle_ballistic_01_black01","uuid":"11111111-2222-3333-4444-555555555555","name":"P4-AR \"Blackguard\" Rifle","market":{"price":null,"shops":[]}}]},
           {"class":"volt_smg_energy_01","name":"Quartz Energy SMG","kind":"SMG","weight":"Medium","size":2,"manufacturer":"",
            "damage":{"physical":0,"energy":5,"distortion":0,"thermal":0,"biochemical":0,"stun":0,"total":5,"dominant":"energy"},
            "projectileSpeed":600,"projectileLifetime":2,"dropStart":0,"dropPerMetre":0,"dropFloor":0,"floorAt":0,"magazine":45,"magazineClass":"volt_smg_energy_01_mag","mass":1.55,
            "modes":[{"name":"BEAM","kind":"Beam","roundsPerMinute":0,"pellets":1,"ammoPerShot":0,"burstShots":0,"beamDamagePerSecond":{"energy":225,"distortion":50,"total":275},"beamFullRange":10,"beamZeroRange":25,"beamAmmoPerSecond":7.5,"heatPerShot":50,"condition":"","hit":{"energy":5,"total":5},"sustainedRoundsPerMinute":0,"damagePerShot":5,"damagePerSecond":275,"damagePerMagazine":1650,"secondsToEmpty":6}],
            "market":{"price":null,"shops":[]},"finishes":[]},
           {"class":"volt_rifle_energy_01","name":"Parallax Energy Assault Rifle","kind":"Rifle","weight":"Medium","size":2,"manufacturer":"",
            "damage":{"physical":0,"energy":13,"distortion":5,"thermal":0,"biochemical":0,"stun":3,"total":21,"dominant":"energy"},
            "projectileSpeed":600,"projectileLifetime":2,"dropStart":200,"dropPerMetre":0.03,"dropFloor":31,"floorAt":250,"magazine":80,"magazineClass":"volt_rifle_energy_01_mag","mass":2.6,
            "modes":[{"name":"AUTO","kind":"Auto","roundsPerMinute":600,"pellets":1,"ammoPerShot":1,"burstShots":0,"heatPerShot":3.5,"condition":"","hit":{"energy":13,"distortion":5,"stun":3,"total":21},"sustainedRoundsPerMinute":600,"damagePerShot":21,"damagePerSecond":210,"damagePerMagazine":1680,"secondsToEmpty":8},
                     {"name":"BEAM","kind":"Beam","roundsPerMinute":0,"pellets":1,"ammoPerShot":0,"burstShots":0,"beamDamagePerSecond":{"energy":210,"distortion":50,"total":260},"beamFullRange":65,"beamZeroRange":65,"beamAmmoPerSecond":5,"heatPerShot":17,"condition":"heat at or above 40%","hit":{"energy":13,"total":21},"sustainedRoundsPerMinute":0,"damagePerShot":21,"damagePerSecond":260,"damagePerMagazine":4160,"secondsToEmpty":16}],
            "market":{"price":null,"shops":[]},"finishes":[]}],
         "armour":[
           {"family":"Testudo","slot":"Core","weight":"Medium","kind":"Armor: Core","manufacturer":"Quirinus Tech",
            "resistances":{"macro":"MediumArmor","physical":0.7,"energy":0.7,"distortion":0.7,"thermal":0.7,"biochemical":0.7,"stun":0.55,"impact":0.6925},
            "protects":["torso"],"temperatureMin":-50,"temperatureMax":80,"radiationCapacity":26400,"radiationDissipation":145.8,"gForceResistance":-0.25,"capacityMicroScu":8000,"emSignature":6,"irSignature":0,"motionPenalty":0.5,"viewPenalty":0.5,"mass":5,
            "name":"Testudo Core",
            "pieces":[{"class":"qrt_combat_medium_core_01_01_01","uuid":"edaf0b70-e1a4-48ff-b827-37e9324014bd","name":"Testudo Core","market":{"price":3150,"shops":[{"terminal":"Cubby Area 18","place":"Area18","system":"Stanton","price":3150}]}},
                      {"class":"qrt_combat_medium_core_04_01_01","uuid":"66666666-7777-8888-9999-000000000000","name":"Testudo Core Deathblow","market":{"price":null,"shops":[]}}],
            "market":{"price":3150,"shops":[{"terminal":"Cubby Area 18","place":"Area18","system":"Stanton","price":3150}]}},
           {"family":"Lynx","slot":"Arms","weight":"Light","kind":"Armor: Arms","manufacturer":"Kastak Arms",
            "resistances":{"macro":"LightArmor","physical":0.8,"energy":0.8,"distortion":0.8,"thermal":0.8,"biochemical":0.8,"stun":0.7,"impact":0.9},
            "protects":["left_arm","right_arm"],"temperatureMin":-31,"temperatureMax":61,"radiationCapacity":26000,"radiationDissipation":145.8,"gForceResistance":-0.03,"capacityMicroScu":0,"emSignature":2,"irSignature":0,"motionPenalty":0,"viewPenalty":0,"mass":2,
            "name":"Lynx Arms","pieces":[{"class":"outlaw_legacy_armor_light_arms_01_01_01","name":"Lynx Arms","market":{"price":null,"shops":[]}}],"market":{"price":null,"shops":[]}},
           {"family":"ADP","slot":"Core","weight":"Heavy","kind":"Armor: Core","manufacturer":"Clark Defense Systems",
            "resistances":{"macro":"HeavyArmor","physical":0.6,"energy":0.6,"distortion":0.6,"thermal":0.6,"biochemical":0.6,"stun":0.4,"impact":0.65},
            "protects":["torso"],"temperatureMin":-75,"temperatureMax":105,"radiationCapacity":26800,"radiationDissipation":145.8,"gForceResistance":-0.5,"capacityMicroScu":11000,"emSignature":30,"irSignature":0,"motionPenalty":1,"viewPenalty":1,"mass":7,
            "name":"ADP Core","pieces":[{"class":"cds_armor_heavy_core_01_01_01","name":"ADP Core","market":{"price":4840,"shops":[{"terminal":"Cubby Area 18","place":"Area18","system":"Stanton","price":4840}]}}],
            "market":{"price":4840,"shops":[{"terminal":"Cubby Area 18","place":"Area18","system":"Stanton","price":4840}]}}]}
        """;

    private static Page Opened()
    {
        var page = new Page();
        page.Serve("/api/armoury", Model);
        page.Do("await loadArmoury();");
        return page;
    }

    [Fact]
    public void Guns_are_ranked_by_derived_dps_with_every_mode_and_the_price_beside()
    {
        var page = Opened();

        Assert.True(page.Truth("__dom.node('#armoury-unready').hidden"));
        var rows = "__dom.node('#armoury-guns tbody').children";
        Assert.Equal(3, page.Count($"{rows}.length"));
        // The Quartz's beam is 275 a second; the Parallax's beam 260; the P4-AR 162.
        Assert.Contains("Quartz Energy SMG", page.Text($"{rows}[0].textContent"));
        Assert.Contains("Parallax", page.Text($"{rows}[1].textContent"));
        Assert.Contains("P4-AR Rifle", page.Text($"{rows}[2].textContent"));

        var p4 = page.Text($"{rows}[2].textContent");
        Assert.Contains("12 ballistic", p4);
        Assert.Contains("AUTO 810 rpm", p4);
        Assert.Contains("SEMI 810 rpm", p4);
        Assert.Contains("162", p4);
        Assert.Contains("480", p4);
        Assert.Contains("40 m → 10", p4);
        Assert.Contains("4,138 aUEC", p4);
        Assert.Contains("Guns Rod's Fuel, Rod's Fuel & Supplies +1", p4);
        Assert.Contains("Medium holster", p4);

        // A conditional mode says when it takes over; a beam says how far it reaches.
        var parallax = page.Text($"{rows}[1].textContent");
        Assert.Contains("BEAM 260/s to 65 m", parallax);
        Assert.Contains("when heat at or above 40%", parallax);
        Assert.Contains("13 energy + 5 distortion + 3 stun", parallax);
        Assert.Contains("200 m → 31", parallax);
        Assert.Contains("none", page.Text($"{rows}[0].textContent"));

        Assert.Contains("3 of 3 guns", page.NodeText("#armoury-gun-count"));
        Assert.Contains("327 counting every finish", page.NodeText("#armoury-gun-count"));
        Assert.Contains("UEX prices 1 of them", page.NodeText("#armoury-gun-count"));
    }

    [Fact]
    public void The_kind_and_damage_filters_and_the_price_tick_narrow_the_guns()
    {
        var page = Opened();
        var rows = "__dom.node('#armoury-guns tbody').children";

        // The kinds the install has, in holster order: SMG before rifle.
        Assert.Equal("SMG|Rifle", page.Text("[...__dom.node('#armoury-gun-kind').options].slice(1).map(o => o.value).join('|')"));

        page.Do("__dom.node('#armoury-gun-kind').value = 'SMG'; renderArmouryGuns();");
        Assert.Equal(1, page.Count($"{rows}.length"));
        Assert.Contains("Quartz", page.Text($"{rows}[0].textContent"));

        page.Do("__dom.node('#armoury-gun-kind').value = ''; __dom.node('#armoury-gun-damage').value = 'physical'; renderArmouryGuns();");
        Assert.Equal(1, page.Count($"{rows}.length"));
        Assert.Contains("P4-AR", page.Text($"{rows}[0].textContent"));

        page.Do("__dom.node('#armoury-gun-damage').value = ''; __dom.node('#armoury-gun-priced').checked = true; renderArmouryGuns();");
        Assert.Equal(1, page.Count($"{rows}.length"));
        Assert.Contains("P4-AR", page.Text($"{rows}[0].textContent"));

        page.Do("__dom.node('#armoury-gun-priced').checked = false; __dom.node('#armoury-gun-sort').value = 'hit'; renderArmouryGuns();");
        Assert.Contains("Parallax", page.Text($"{rows}[0].textContent"));
    }

    [Fact]
    public void A_gun_opens_under_its_row_with_every_mode_and_its_finishes()
    {
        var page = Opened();

        page.Do("armouryOpenGun = 'behr_rifle_ballistic_01'; renderArmouryGuns();");
        var rows = "__dom.node('#armoury-guns tbody').children";
        Assert.Equal(4, page.Count($"{rows}.length"));
        Assert.Contains("open", page.Text($"{rows}[2].className"));
        Assert.Contains("armoury-expand", page.Text($"{rows}[3].className"));

        var detail = page.Text($"{rows}[3].textContent");
        Assert.Contains("Fire modes, trigger held", detail);
        Assert.Contains("810 rpm", detail);
        Assert.Contains("Projectile 550 m/s for 2 s (1,100 m before it is gone)", detail);
        Assert.Contains("loses 0.05 a metre down to 10 at 80 m", detail);
        Assert.Contains("Magazine 40 rounds", detail);
        Assert.Contains("2.65 kg", detail);
        Assert.Contains("Finishes (1)", detail);
        Assert.Contains("P4-AR \"Blackguard\" Rifle", detail);
        Assert.Contains("no terminal recorded", detail);
        Assert.Contains("same figures, its own price", detail);
    }

    [Fact]
    public void Armour_says_resistance_is_the_class_and_shows_what_actually_differs()
    {
        var page = Opened();

        var note = page.NodeText("#armoury-armour-note");
        Assert.Contains("by class, not by piece", note);
        Assert.Contains("light piece lets 80%", note);
        Assert.Contains("medium 70%", note);
        Assert.Contains("heavy 60%", note);
        Assert.Contains("stun 70 / 55 / 40%", note);

        var rows = "__dom.node('#armoury-armour tbody').children";
        Assert.Equal(3, page.Count($"{rows}.length"));
        // By set: ADP, Lynx, Testudo.
        var adp = page.Text($"{rows}[0].textContent");
        Assert.Contains("ADP", adp);
        Assert.Contains("Heavy", adp);
        Assert.Contains("60%", adp);
        Assert.Contains("40%", adp);
        Assert.Contains("-75 to 105 °C", adp);
        Assert.Contains("26,800 · 145.8/s", adp);
        Assert.Contains("11 mSCU", adp);
        Assert.Contains("4,840 aUEC", adp);
        Assert.Contains("Cubby Area 18, Area18", adp);

        var lynx = page.Text($"{rows}[1].textContent");
        Assert.Contains("Lynx", lynx);
        Assert.Contains("no terminal recorded", lynx);

        Assert.Contains("3 of 3 sets, from 2349 pieces", page.NodeText("#armoury-armour-count"));
    }

    [Fact]
    public void Armour_filters_by_slot_and_weight_and_opens_to_its_colours()
    {
        var page = Opened();
        var rows = "__dom.node('#armoury-armour tbody').children";

        page.Do("__dom.node('#armoury-armour-slot').value = 'Core'; __dom.node('#armoury-armour-weight').value = 'Medium'; renderArmouryArmour();");
        Assert.Equal(1, page.Count($"{rows}.length"));
        Assert.Contains("Testudo", page.Text($"{rows}[0].textContent"));
        // Two colours, from the cheaper's price.
        Assert.Contains("3,150 aUEC", page.Text($"{rows}[0].textContent"));

        page.Do("armouryOpenSet = 'Core|Testudo Core'; renderArmouryArmour();");
        Assert.Equal(2, page.Count($"{rows}.length"));
        var detail = page.Text($"{rows}[1].textContent");
        Assert.Contains("Takes 70% of ballistic", detail);
        Assert.Contains("55% of stun", detail);
        Assert.Contains("MediumArmor table, shared by every piece", detail);
        Assert.Contains("Covers torso", detail);
        Assert.Contains("Restriction penalty 50% movement", detail);
        Assert.Contains("what puts the piece into that state is not read", detail);
        Assert.Contains("Colours and editions (2)", detail);
        Assert.Contains("Testudo Core Deathblow", detail);

        page.Do("__dom.node('#armoury-armour-weight').value = ''; __dom.node('#armoury-armour-sort').value = 'cold'; renderArmouryArmour();");
        Assert.Contains("ADP", page.Text($"{rows}[0].textContent"));
    }

    [Fact]
    public void The_search_box_narrows_both_tables()
    {
        var page = Opened();

        page.Do("__dom.node('#armoury-search').value = 'testudo'; renderArmouryGuns(); renderArmouryArmour();");
        Assert.Equal(0, page.Count("__dom.node('#armoury-guns tbody').children.length"));
        Assert.Equal(1, page.Count("__dom.node('#armoury-armour tbody').children.length"));

        // A colour's name finds its set.
        page.Do("__dom.node('#armoury-search').value = 'deathblow'; renderArmouryArmour();");
        Assert.Equal(1, page.Count("__dom.node('#armoury-armour tbody').children.length"));
    }

    [Fact]
    public void Before_the_install_is_read_the_page_says_so_and_shows_no_table()
    {
        var page = new Page();
        page.Serve("/api/armoury", """{"ready":false,"weapons":[],"armour":[]}""");
        page.Do("await loadArmoury();");

        Assert.False(page.Truth("__dom.node('#armoury-unready').hidden"));
        Assert.Contains("not been read yet", page.NodeText("#armoury-unready"));
        Assert.True(page.Truth("__dom.node('#armoury-pane-guns').hidden"));
        Assert.True(page.Truth("__dom.node('#armoury-tabs').hidden"));
    }

    [Fact]
    public void Without_uex_the_price_column_says_what_it_needs()
    {
        var page = new Page();
        page.Serve("/api/armoury", Model.Replace("\"itemPricesKnown\":true", "\"itemPricesKnown\":false"));
        page.Do("await loadArmoury();");

        Assert.Contains("prices need UEX (Settings)", page.Text("__dom.node('#armoury-guns tbody').children[0].textContent"));
        Assert.Contains("Prices need UEX, in Settings", page.NodeText("#armoury-gun-count"));
    }

    [Fact]
    public void An_open_row_shows_the_wiki_picture_and_a_colour_chip_swaps_it()
    {
        var page = Opened();

        page.Do("armouryOpenGun = 'behr_rifle_ballistic_01'; renderArmouryGuns();");
        var gun = "__dom.node('#armoury-guns tbody').children[3]";
        Assert.Equal("/api/armoury/picture/02d4cd2e-fa98-4086-aee1-6b2dfce8ea27", page.Text($"{gun}.querySelectorAll('.armoury-picture')[0].querySelectorAll('img')[0].src"));
        Assert.Equal("P4-AR Rifle", page.Text($"{gun}.querySelectorAll('.armoury-picture')[0].querySelectorAll('img')[0].alt"));

        // A finish is the same gun photographed in its own colour.
        page.Do($"{gun}.querySelectorAll('.armoury-finish')[0].click();");
        Assert.Equal("/api/armoury/picture/11111111-2222-3333-4444-555555555555", page.Text($"{gun}.querySelectorAll('.armoury-picture')[0].querySelectorAll('img')[0].src"));

        page.Do("armouryOpenSet = 'Core|Testudo Core'; renderArmouryArmour();");
        var set = "__dom.node('#armoury-armour tbody').children[3]";
        Assert.Equal("/api/armoury/picture/edaf0b70-e1a4-48ff-b827-37e9324014bd", page.Text($"{set}.querySelectorAll('.armoury-picture')[0].querySelectorAll('img')[0].src"));
        page.Do($"{set}.querySelectorAll('.armoury-finish')[1].click();");
        Assert.Equal("/api/armoury/picture/66666666-7777-8888-9999-000000000000", page.Text($"{set}.querySelectorAll('.armoury-picture')[0].querySelectorAll('img')[0].src"));
    }

    [Fact]
    public void Without_the_community_dataset_the_picture_says_what_it_needs()
    {
        var page = new Page();
        page.Serve("/api/armoury", Model.Replace("\"picturesKnown\":true", "\"picturesKnown\":false"));
        page.Do("await loadArmoury(); armouryOpenGun = 'behr_rifle_ballistic_01'; renderArmouryGuns();");
        var gun = "__dom.node('#armoury-guns tbody').children[3]";
        Assert.Equal(0, page.Count($"{gun}.querySelectorAll('.armoury-picture img').length"));
        Assert.Contains("community dataset is on (Settings)", page.Text($"{gun}.querySelectorAll('.armoury-picture')[0].textContent"));
    }
}
