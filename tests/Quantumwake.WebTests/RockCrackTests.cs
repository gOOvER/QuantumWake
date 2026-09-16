namespace Quantumwake.WebTests;

/// <summary>
/// The rock calculator on the Mining page: the ship decides the heads, every
/// change asks the server, and the answer is called an estimate with the
/// rule's owner named. Nothing is computed on the page.
/// </summary>
public class RockCrackTests
{
    private const string Model = """
        {"ready":true,
         "constants":{"powerCapacityPerMass":10,"decayPerMass":0.2,"optimalWindowSize":0.1,"optimalWindowMaxSize":0.5},
         "lasers":[
           {"class":"Mining_Laser_GRIN_Arbor_S1","name":"Arbor MH1 Mining Laser","size":1,"power":2340,"modifiers":{"instability":-35,"windowSize":40,"resistance":25}},
           {"class":"Mining_Laser_THCN_Helix_S1","name":"Helix I Mining Laser","size":1,"power":3900,"modifiers":{"windowSize":-40,"resistance":-30}},
           {"class":"Mining_Laser_GRIN_Arbor_S2","name":"Arbor MH2 Mining Laser","size":2,"power":2900,"modifiers":{}}],
         "modules":[{"class":"Mining_Modules_Active_Surge","name":"Surge Module","active":true,"lifetime":15,"charges":7,"powerMultiplier":1.5,"modifiers":{"resistance":-15.5,"instability":10}}],
         "gadgets":[{"class":"Mining_Gadget_SHIN_Sabir","name":"Sabir","modifiers":{"resistance":-50,"windowSize":50,"instability":15}}],
         "minerals":[{"class":"Quantainium_Raw","name":"Quantainium (Raw)","resistance":0.95,"instability":1000,"windowMidpoint":0.5,"windowRandomness":0.2,"explosionMultiplier":260}],
         "ships":[
           {"class":"ARGO_MOLE","name":"Argo MOLE","heads":[{"portId":"a","size":2,"stock":"Mining_Laser_GRIN_Arbor_S2"},{"portId":"b","size":2,"stock":"Mining_Laser_GRIN_Arbor_S2"},{"portId":"c","size":2,"stock":"Mining_Laser_GRIN_Arbor_S2"}],"flown":false},
           {"class":"MISC_Prospector","name":"MISC Prospector","heads":[{"portId":"p","size":1,"stock":"Mining_Laser_GRIN_Arbor_S1"}],"flown":true}],
         "rule":{"requiredWattsPerKg":0.36,"soloRatio":1.15,"gadgetRatio":0.7,"source":"scminer.rocks, 2026-09-15 - the community's line, not the game's"}}
        """;

    private const string Verdict = """
        {"verdict":{"powerDelivered":2340,"powerRequired":1800,"ratio":1.3,"verdict":"solo",
                    "effectiveResistancePercent":0,"effectiveInstabilityPercent":9.8,"windowPercent":14,"maxCrackableMassKg":6500,
                    "energyCapacity":50000,"energyDecayPerSecond":1000,"modifiers":{},
                    "notes":["By the game's constants the rock holds 50,000 energy and sheds 1,000 a second; the window is 14% of the gauge."]},
         "matrix":[
           {"laser":"Mining_Laser_THCN_Helix_S1","name":"Helix I Mining Laser","power":3900,"powerDelivered":3900,"ratio":2.167,"verdict":"solo","maxCrackableMassKg":10833},
           {"laser":"Mining_Laser_GRIN_Arbor_S1","name":"Arbor MH1 Mining Laser","power":2340,"powerDelivered":2340,"ratio":1.3,"verdict":"solo","maxCrackableMassKg":6500}]}
        """;

    private static Page Opened()
    {
        var page = new Page();
        page.Serve("/api/mining/model", Model);
        page.Serve("/api/mining/crack", Verdict);
        // The markup's defaults (5,000 kg, 0%, 15%) are not in the stub; set them as the page would find them.
        page.Do("__dom.node('#crack-mass').value = '5000'; __dom.node('#crack-resistance').value = '0'; __dom.node('#crack-instability').value = '15'; await loadCrackModel();");
        return page;
    }

    [Fact]
    public void The_flown_ship_comes_first_and_its_own_head_is_picked()
    {
        var page = Opened();

        Assert.False(page.Truth("__dom.node('#crack').hidden"));
        Assert.Equal("MISC_Prospector", page.Text("__dom.node('#crack-ship').value"));
        Assert.Contains("flown", page.Text("__dom.node('#crack-ship').options[0].textContent"));

        var heads = "__dom.node('#crack-heads').querySelectorAll('.crack-head')";
        Assert.Equal(1, page.Count($"{heads}.length"));
        Assert.Equal("Mining_Laser_GRIN_Arbor_S1", page.Text($"{heads}[0].querySelectorAll('.crack-laser')[0].value"));
        // Only S1 heads are offered on an S1 port.
        Assert.Equal(2, page.Count($"{heads}[0].querySelectorAll('.crack-laser')[0].options.length"));

        Assert.Contains("0.36 W per kilogram", page.NodeText("#crack-rule"));
        Assert.Contains("scminer.rocks", page.NodeText("#crack-rule"));
    }

    [Fact]
    public void The_rock_is_posted_as_scanned_and_the_verdict_is_called_an_estimate()
    {
        var page = Opened();

        var body = page.BodyOf("/api/mining/crack");
        Assert.Contains("\"massKg\":5000", body);
        Assert.Contains("\"resistance\":0", body);
        Assert.Contains("\"laser\":\"Mining_Laser_GRIN_Arbor_S1\"", body);

        var verdict = page.NodeText("#crack-verdict");
        Assert.Contains("Breaks on this fit", verdict);
        Assert.Contains("2,340 delivered against 1,800 needed", verdict);
        Assert.Contains("130%", verdict);
        Assert.Contains("estimate, community rule", verdict);
        Assert.Contains("6,500 kg", verdict);
        Assert.Contains("50,000", verdict);

        var matrix = page.NodeText("#crack-matrix tbody");
        Assert.Contains("Helix I Mining Laser", matrix);
        Assert.Contains("10,833", matrix);
        Assert.Contains("three per head because the game's count per head is not read yet", page.NodeText("#crack-matrix-note"));
    }

    [Fact]
    public void Picking_the_MOLE_gives_three_S2_heads_and_a_module_and_gadget_go_into_the_request()
    {
        var page = Opened();
        page.Do("""
            __dom.node('#crack-ship').value = 'ARGO_MOLE'; renderCrackHeads();
            const rows = __dom.node('#crack-heads').querySelectorAll('.crack-head');
            rows[0].querySelectorAll('.crack-module')[0].value = 'Mining_Modules_Active_Surge';
            __dom.node('#crack-gadget').value = 'Mining_Gadget_SHIN_Sabir';
            __dom.node('#crack-resistance').value = '40';
            await assessCrack();
            """);

        var heads = "__dom.node('#crack-heads').querySelectorAll('.crack-head')";
        Assert.Equal(3, page.Count($"{heads}.length"));
        Assert.Equal("Mining_Laser_GRIN_Arbor_S2", page.Text($"{heads}[1].querySelectorAll('.crack-laser')[0].value"));

        var body = page.BodyOf("/api/mining/crack");
        Assert.Contains("\"resistance\":40", body);
        Assert.Contains("\"modules\":[\"Mining_Modules_Active_Surge\"]", body);
        Assert.Contains("\"gadget\":\"Mining_Gadget_SHIN_Sabir\"", body);
        // Three heads posted, the other two without modules.
        Assert.Equal(3, body.Split("\"laser\":").Length - 1);
    }

    /// <summary>The mineral is for the notes: the game's own figures for it, and a warning where an overcharge is the rock gone.</summary>
    [Fact]
    public void A_mineral_adds_the_games_figures_for_it_to_the_notes()
    {
        var page = Opened();
        page.Do("__dom.node('#crack-mineral').value = 'Quantainium_Raw'; await assessCrack();");

        var verdict = page.NodeText("#crack-verdict");
        Assert.Contains("Quantainium (Raw): element resistance 0.95, instability 1000", verdict);
        Assert.Contains("an overcharge here is the rock gone", verdict);
    }

    [Fact]
    public void Before_the_install_is_read_it_says_so_instead_of_showing_a_form()
    {
        var page = new Page();
        page.Serve("/api/mining/model", """{"ready":false,"lasers":[],"modules":[],"gadgets":[],"minerals":[],"ships":[]}""");
        page.Do("gameDataState = {state:'reading'}; await loadCrackModel();");

        Assert.True(page.Truth("__dom.node('#crack').hidden"));
        Assert.Contains("Still reading", page.NodeText("#crack-unready"));
    }
}
