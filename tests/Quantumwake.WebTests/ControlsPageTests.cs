namespace Quantumwake.WebTests;

/// <summary>
/// The Controls page: the sticks the game's profile knows and what is
/// bound to each, the actions the other way round, the versions kept, and
/// the pictures the bindings are drawn on.
/// </summary>
/// <remarks>
/// The model is cut from this install's own profile on 2026-09-17: the
/// Warthog stick as js4 with its pitch and yaw axes and two buttons, the
/// throttle as js2 with a chord, and a cleared default. The picture itself
/// is drawn by the browser's parser, which the harness lacks; the mapping
/// from a template's placeholder to a control is what can be pinned here.
/// </remarks>
public class ControlsPageTests
{
    private const string Model = """
        {"ready":true,"profilePath":"C:\\SC\\LIVE\\user\\client\\0\\Profiles\\default\\actionmaps.xml","profileFound":true,"problem":null,
         "profile":{"name":"default","label":null,
          "devices":[
           {"key":"kb1","type":"keyboard","instance":1,"product":"Keyboard","guid":"{6F1D2B61-D5A0-11CF-BFC7-444553540000}","usb":null,"deadzones":{},"curves":[],"bindings":1,"template":null,"layouts":[]},
           {"key":"js2","type":"joystick","instance":2,"product":"Throttle - HOTAS Warthog","guid":"{0404044F-0000-0000-0000-504944564944}","usb":{"vendor":1103,"product":1028},
            "deadzones":{"z":0.0495},"curves":[{"option":"flight_strafe_forward","exponent":2,"inverted":false}],"bindings":2,
            "template":{"key":"repo:Thrustmaster/Thrustmaster Warthog - Throttle.svg","name":"Thrustmaster Warthog - Throttle","maker":"Thrustmaster"},"layouts":["layout_hotas_warthog.xml"]},
           {"key":"js4","type":"joystick","instance":4,"product":"Joystick - HOTAS Warthog","guid":"{0402044F-0000-0000-0000-504944564944}","usb":{"vendor":1103,"product":1026},
            "deadzones":{"x":0.0297},"curves":[{"option":"flight_move_pitch","exponent":1.5,"inverted":false}],"bindings":4,"template":null,"layouts":["layout_hotas_warthog.xml"]},
           {"key":"js7","type":"joystick","instance":7,"product":null,"guid":null,"usb":null,"deadzones":{},"curves":[],"bindings":0,"template":null,"layouts":[]}],
          "bindings":[
           {"actionMap":"spaceship_movement","action":"v_pitch","input":{"raw":"js4_y","device":"js","instance":4,"kind":"axis","index":0,"name":"y","control":"y","label":"y axis","deviceKey":"js4","modifiers":[]},"activationMode":"","multiTap":null,"label":"Pitch","description":"","map":"Flight - Movement","category":"FLIGHT","known":true},
           {"actionMap":"spaceship_movement","action":"v_yaw","input":{"raw":"js4_x","device":"js","instance":4,"kind":"axis","index":0,"name":"x","control":"x","label":"x axis","deviceKey":"js4","modifiers":[]},"activationMode":"","multiTap":null,"label":"Yaw","description":"","map":"Flight - Movement","category":"FLIGHT","known":true},
           {"actionMap":"spaceship_targeting_advanced","action":"v_target_cycle_hostile_fwd","input":{"raw":"js4_button7","device":"js","instance":4,"kind":"button","index":7,"name":"","control":"button7","label":"button 7","deviceKey":"js4","modifiers":[]},"activationMode":"tap","multiTap":null,"label":"Cycle Lock - Hostiles - Forward","description":"","map":"Flight - Targeting","category":"FLIGHT","known":true},
           {"actionMap":"seat_general","action":"v_eject","input":{"raw":"js4_button4","device":"js","instance":4,"kind":"button","index":4,"name":"","control":"button4","label":"button 4","deviceKey":"js4","modifiers":[]},"activationMode":"delayed_press_long","multiTap":null,"label":"Eject","description":"Leave the seat","map":"Seat - General","category":"SEAT","known":true},
           {"actionMap":"spaceship_movement","action":"v_afterburner","input":{"raw":"js2_button5+js2_button11","device":"js","instance":2,"kind":"button","index":11,"name":"","control":"button11","label":"button 5 + button 11","deviceKey":"js2","modifiers":["js2_button5"]},"activationMode":"hold","multiTap":null,"label":"Afterburner","description":"","map":"Flight - Movement","category":"FLIGHT","known":true},
           {"actionMap":"spaceship_movement","action":"v_afterburner","input":{"raw":"kb1_lshift","device":"kb","instance":1,"kind":"key","index":0,"name":"lshift","control":"lshift","label":"lshift","deviceKey":"kb1","modifiers":[]},"activationMode":"hold","multiTap":null,"label":"Afterburner","description":"","map":"Flight - Movement","category":"FLIGHT","known":true},
           {"actionMap":"spaceship_movement","action":"v_throttle","input":{"raw":"js2_z","device":"js","instance":2,"kind":"axis","index":0,"name":"z","control":"z","label":"z axis","deviceKey":"js2","modifiers":[]},"activationMode":"","multiTap":null,"label":"Throttle","description":"","map":"Flight - Movement","category":"FLIGHT","known":true},
           {"actionMap":"seat_general","action":"v_light_amplification_toggle","input":{"raw":"js1_ ","device":"js","instance":1,"kind":"none","index":0,"name":"","control":"","label":"unbound","deviceKey":"js1","modifiers":[]},"activationMode":"","multiTap":null,"label":"Light amplification","description":"","map":"Seat - General","category":"SEAT","known":true}]},
         "catalogue":{
          "maps":[{"name":"seat_general","label":"Seat - General","category":"SEAT"},{"name":"spaceship_movement","label":"Flight - Movement","category":"FLIGHT"},{"name":"spaceship_targeting_advanced","label":"Flight - Targeting","category":"FLIGHT"}],
          "actions":[
           {"actionMap":"seat_general","name":"v_eject","label":"Eject","description":"Leave the seat","keyboard":"lalt+u","mouse":"","gamepad":"","joystick":"","activationMode":"delayed_press_long","optionGroup":""},
           {"actionMap":"seat_general","name":"v_light_amplification_toggle","label":"Light amplification","description":"","keyboard":"n","mouse":"","gamepad":"","joystick":"button5","activationMode":"","optionGroup":""},
           {"actionMap":"seat_general","name":"v_self_destruct","label":"Self destruct","description":"","keyboard":"backspace","mouse":"","gamepad":"","joystick":"","activationMode":"delayed_press_long","optionGroup":""},
           {"actionMap":"spaceship_movement","name":"v_pitch","label":"Pitch","description":"","keyboard":"","mouse":"maxis_y","gamepad":"thumbly","joystick":"y","activationMode":"","optionGroup":"flight_move_pitch"},
           {"actionMap":"spaceship_movement","name":"v_yaw","label":"Yaw","description":"","keyboard":"","mouse":"maxis_x","gamepad":"thumbrx","joystick":"x","activationMode":"","optionGroup":"flight_move_yaw"},
           {"actionMap":"spaceship_movement","name":"v_afterburner","label":"Afterburner","description":"","keyboard":"lshift","mouse":"","gamepad":"","joystick":"","activationMode":"hold","optionGroup":""},
           {"actionMap":"spaceship_movement","name":"v_throttle","label":"Throttle","description":"","keyboard":"","mouse":"","gamepad":"","joystick":"","activationMode":"","optionGroup":""},
           {"actionMap":"spaceship_targeting_advanced","name":"v_target_cycle_hostile_fwd","label":"Cycle Lock - Hostiles - Forward","description":"","keyboard":"","mouse":"","gamepad":"","joystick":"","activationMode":"tap","optionGroup":""}]},
         "layouts":[{"file":"layout_hotas_warthog.xml","name":"Thrustmaster Warthog HOTAS (no rudder pedals)","label":"@ui_input_TM_Warthog_HOTAS","devices":[{"key":"js1","product":"Joystick - HOTAS Warthog","guid":"{0402044F-0000-0000-0000-504944564944}"}],"bindings":73}],
         "templates":{"enabled":true,"fetchedAt":"2026-09-17T20:00:00Z","folder":null,"project":"https://github.com/Rexeh/joystick-diagrams",
          "available":[{"key":"repo:Thrustmaster/Thrustmaster Warthog - Joystick.svg","name":"Thrustmaster Warthog - Joystick","maker":"Thrustmaster"},{"key":"repo:Thrustmaster/Thrustmaster Warthog - Throttle.svg","name":"Thrustmaster Warthog - Throttle","maker":"Thrustmaster"},{"key":"repo:VKB Sim/VKB-Sim Gladiator NXT R.svg","name":"VKB-Sim Gladiator NXT R","maker":"VKB Sim"}],
          "assignments":{}},
         "backups":{"id":"20260916-235612-417-a1b2c3d4","takenAt":"2026-09-16T23:56:12Z","writtenAt":"2026-09-16T23:56:10Z","bytes":17823},
         "backupCount":3}
        """;

    private static Page Opened(string pane = "devices")
    {
        var page = new Page();
        page.Serve("/api/controls", Model);
        page.Serve("/api/controls/backups", "[]");
        page.Do($"showControlsPane('{pane}'); await loadControls();");
        return page;
    }

    private static string Active(Page page, string id) =>
        page.Text($"__dom.node('#{id}').classList.contains('active') ? 'on' : 'off'");

    [Fact]
    public void The_status_line_counts_sticks_bindings_actions_and_versions()
    {
        var status = Opened().NodeText("#controls-status");

        Assert.Contains("2 sticks and 6 joystick bindings", status);
        Assert.Contains("8 actions the game can bind", status);
        Assert.Contains("3 versions kept", status);
    }

    [Fact]
    public void The_sticks_are_the_devices_with_a_product_and_the_first_is_open()
    {
        var page = Opened();

        var strip = page.NodeText("#controls-devices");
        Assert.Contains("Throttle - HOTAS Warthog", strip);
        Assert.Contains("Joystick - HOTAS Warthog", strip);
        Assert.DoesNotContain("js7", strip);
        Assert.DoesNotContain("Keyboard", strip);
        Assert.Contains("Throttle - HOTAS Warthog · js2", page.NodeText("#controls-device-title"));
        Assert.Contains("USB 044f:0404", page.NodeText("#controls-device-note"));
    }

    /// <summary>
    /// Every bound control on the open stick, a chord with its modifier, the
    /// mode in words, and a control the picture would name but nothing binds.
    /// </summary>
    [Fact]
    public void The_button_table_shows_what_each_control_does()
    {
        var page = Opened();
        page.Do("controlsDevice = 'js4'; await renderControlsDevice();");

        var table = page.NodeText("#controls-buttons tbody");
        Assert.Contains("Button 4", table);
        Assert.Contains("Eject", table);
        Assert.Contains("long press", table);
        Assert.Contains("Button 7", table);
        Assert.Contains("Cycle Lock - Hostiles - Forward", table);
        Assert.Contains("y axis", table);
        Assert.Contains("Pitch", table);
        Assert.Contains("Flight - Movement · FLIGHT", table);
        Assert.Contains("x axis", page.NodeText("#controls-axes"));
        Assert.Contains("move pitch", page.NodeText("#controls-axes"));

        page.Do("controlsDevice = 'js2'; await renderControlsDevice();");
        // The bound-to cells alone: the change picker lists every action, Eject included.
        var throttle = page.Text("__dom.node('#controls-buttons tbody').children.map(tr => tr.children.slice(0, 3).map(td => td.textContent).join(' ')).join('|')");
        Assert.Contains("Button 11", throttle);
        Assert.Contains("button 5 + Afterburner", throttle);
        Assert.Contains("hold", throttle);
        Assert.DoesNotContain("Eject", throttle);
    }

    /// <summary>
    /// The fix is where the gap is: with the library off, the picture slot
    /// offers to fetch it; with it on and no match, a picker of the library.
    /// </summary>
    [Fact]
    public void A_stick_without_a_picture_offers_the_fix_in_place_and_one_with_names_its_source()
    {
        var page = Opened();
        page.Do("controlsDevice = 'js4'; await renderControlsDevice();");
        var note = page.NodeText("#controls-picture-note");
        Assert.Contains("No picture matched to this stick", note);
        Assert.Equal(1, page.Count("__dom.node('#controls-picture-note').querySelectorAll('select').length"));
        Assert.Contains("VKB-Sim Gladiator NXT R", page.Text("__dom.node('#controls-picture-note').querySelectorAll('select')[0].descendants().map(n => n.textContent).join('|')"));

        page.Do("controlsDevice = 'js2'; await renderControlsDevice();");
        Assert.Contains("Thrustmaster Warthog - Throttle", page.NodeText("#controls-picture-note"));
    }

    /// <summary>
    /// No picture: the Windows panel's numbered buttons stand in, to the
    /// highest number the profile mentions rounded to a row, lit where
    /// bound, with the hats as crosses and the axes as bars.
    /// </summary>
    [Fact]
    public void Without_a_picture_the_numbered_grid_stands_in()
    {
        var page = Opened();
        page.Do("controlsDevice = 'js4'; await renderControlsDevice();");

        var grid = "__dom.node('#controls-svg')";
        Assert.Equal(8, page.Count($"{grid}.querySelectorAll('.controls-grid-button').length"));
        Assert.Equal(2, page.Count($"{grid}.querySelectorAll('.controls-grid-button').filter(n => n.classList.contains('bound')).length"));
        Assert.Contains("to 8, the highest your profile mentions being 7", page.NodeText("#controls-svg"));
        Assert.Contains("Eject", page.Text($"{grid}.querySelectorAll('.controls-grid-button').filter(n => n.classList.contains('bound'))[0].textContent"));
        Assert.Equal(2, page.Count($"{grid}.querySelectorAll('.controls-grid-axis').length"));
        Assert.Contains("Pitch", page.NodeText("#controls-svg"));
        Assert.Contains("stand-in until a picture is chosen", page.NodeText("#controls-svg"));

        // The table's rows still highlight the grid's cells.
        Assert.Equal("button7", page.Text($"{grid}.querySelectorAll('.controls-grid-button').filter(n => n.classList.contains('bound'))[1].dataset.control"));
    }

    [Fact]
    public void With_the_library_off_the_picture_slot_has_the_fetch_button_and_it_fetches()
    {
        var page = new Page();
        page.Serve("/api/controls", Model.Replace("\"enabled\":true", "\"enabled\":false").Replace("\"available\":[{", "\"available\":[],\"was\":[{"));
        page.Serve("/api/controls/backups", "[]");
        page.Serve("/api/controls/templates/enable", """{"enabled":true,"templates":45}""");
        page.Do("showControlsPane('devices'); await loadControls(); controlsDevice = 'js4'; await renderControlsDevice();");

        var note = page.NodeText("#controls-picture-note");
        Assert.Contains("the library is off", note);
        Assert.Contains("Fetch the pictures", note);

        page.Do("__dom.node('#controls-picture-note').querySelectorAll('button')[0].click();");
        Assert.Contains("POST /api/controls/templates/enable", page.Fetched());
    }

    [Theory]
    [InlineData("Button_7", "button7")]
    [InlineData("button_12", "button12")]
    [InlineData("POV_1_U", "hat1_up")]
    [InlineData("POV_2_L", "hat2_left")]
    [InlineData("AXIS_X", "x")]
    [InlineData("AXIS_RZ", "rotz")]
    [InlineData("AXIS_SLIDER_1", "slider1")]
    [InlineData("Created by Joystick Diagrams", null)]
    [InlineData("TEMPLATE_NAME", null)]
    public void A_templates_placeholder_names_the_games_control(string placeholder, string? control)
    {
        var page = new Page();
        var got = page.Text($"controlsPlaceholderToControl({Page.Quote(placeholder)}) ?? 'none'");
        Assert.Equal(control ?? "none", got);
    }

    [Fact]
    public void The_actions_table_lists_every_action_with_what_is_on_it_and_filters_to_the_unbound()
    {
        var page = Opened("actions");

        var table = page.NodeText("#controls-actions tbody");
        Assert.Contains("Eject", table);
        Assert.Contains("Joystick - HOTAS Warthog", table);
        Assert.Contains("button 4", table);
        Assert.Contains("Self destruct", table);
        Assert.Contains("default: backspace", table);
        Assert.Contains("cleared - the default was taken off", table);
        Assert.Contains("8 of 8 actions", page.NodeText("#controls-actions-count"));

        page.Do("__dom.node('#controls-unbound').checked = true; renderControlsActions();");
        var unbound = page.NodeText("#controls-actions tbody");
        Assert.Contains("Self destruct", unbound);
        Assert.Contains("Light amplification", unbound);
        Assert.DoesNotContain("Eject", unbound);
        Assert.Contains("2 of 8 actions on no stick", page.NodeText("#controls-actions-count"));

        page.Do("__dom.node('#controls-unbound').checked = false; __dom.node('#controls-category').value = 'SEAT'; renderControlsActions();");
        Assert.Contains("3 of 3 actions in SEAT", page.NodeText("#controls-actions-count"));
    }

    [Fact]
    public void The_backups_list_the_versions_and_a_diff_reads_in_words()
    {
        var page = new Page();
        page.Serve("/api/controls", Model);
        page.Serve("/api/controls/backups", """
            [{"id":"20260916-235612-417-a1b2c3d4","takenAt":"2026-09-16T23:56:12Z","writtenAt":"2026-09-16T23:56:10Z","bytes":17823,"hash":"a1b2c3d4"},
             {"id":"20260910-120000-000-deadbeef","takenAt":"2026-09-10T12:00:00Z","writtenAt":"2026-09-10T11:59:00Z","bytes":16188,"hash":"deadbeef"}]
            """);
        page.Serve("/api/controls/backups/20260910-120000-000-deadbeef/diff/20260916-235612-417-a1b2c3d4", """
            [{"actionMap":"seat_general","action":"v_eject","before":["js6_button7"],"after":["js4_button4"],"label":"Eject","map":"Seat - General","beforeLabels":["button 7"],"afterLabels":["button 4"]},
             {"actionMap":"spaceship_movement","action":"v_throttle","before":[],"after":["js2_z"],"label":"Throttle","map":"Flight - Movement","beforeLabels":[],"afterLabels":["z axis"]}]
            """);
        page.Do("showControlsPane('backups'); await loadControls(); await loadControlsBackups();");

        var list = page.NodeText("#controls-backups tbody");
        Assert.Contains("latest", list);
        Assert.Contains("17 KB", list);
        Assert.Contains("first kept", list);
        Assert.Contains("2 versions kept", page.NodeText("#controls-backups-note"));

        page.Do("controlsDiffPick = ['20260916-235612-417-a1b2c3d4', '20260910-120000-000-deadbeef']; await showControlsDiff();");
        Assert.False(page.Truth("__dom.node('#controls-diff').hidden"));
        Assert.Contains("2 actions changed", page.NodeText("#controls-diff-count"));
        var diff = page.NodeText("#controls-diff-table tbody");
        Assert.Contains("Eject", diff);
        Assert.Contains("js6_button7", diff);
        Assert.Contains("js4_button4", diff);
        Assert.Contains("nothing", diff);
    }

    /// <summary>
    /// Every version has its way out and its way back: a download link, an
    /// export, and a restore that asks first and reports the server's refusal
    /// in its own words when the game is running.
    /// </summary>
    [Fact]
    public void A_version_can_be_downloaded_exported_or_restored_and_a_running_game_refuses_the_restore()
    {
        var page = new Page();
        page.Serve("/api/controls", Model);
        page.Serve("/api/controls/backups", """
            [{"id":"20260916-235612-417-a1b2c3d4","takenAt":"2026-09-16T23:56:12Z","writtenAt":"2026-09-16T23:56:10Z","bytes":17823,"hash":"a1b2c3d4"}]
            """);
        page.Fail("/api/controls/restore", 409, """{"message":"Star Citizen is running. Close the game first."}""");
        page.Do("showControlsPane('backups'); await loadControls(); await loadControlsBackups(); globalThis.confirm = () => true;");

        var row = "__dom.node('#controls-backups tbody').children[0]";
        Assert.Contains("Restore", page.Text($"{row}.textContent"));
        Assert.Contains("Export", page.Text($"{row}.textContent"));
        Assert.Equal("/api/controls/backups/20260916-235612-417-a1b2c3d4/file", page.Text($"{row}.querySelectorAll('a')[0].href"));

        page.Do($"await controlsRestore(controlsBackups[0], {row}.querySelectorAll('button')[0]);");
        Assert.Contains("POST /api/controls/restore", page.Fetched());
        Assert.Contains("Star Citizen is running", page.NodeText("#controls-backups-note"));
    }

    [Fact]
    public void A_restore_the_pilot_declines_sends_nothing()
    {
        var page = Opened("backups");
        page.Do("controlsBackups = [{id:'x', writtenAt:'2026-09-16T23:56:10Z', takenAt:'2026-09-16T23:56:12Z', bytes:1, hash:'x'}]; globalThis.confirm = () => false; await controlsRestore(controlsBackups[0], __dom.node('#controls-keep'));");
        Assert.DoesNotContain("POST /api/controls/restore", page.Fetched());
    }

    /// <summary>
    /// The axes editor: one row per axis and option group, the profile's
    /// curve and dead zone in the fields, and Apply sending the group's
    /// exponent, invert and the axis's dead zone as a fraction.
    /// </summary>
    [Fact]
    public void The_axes_editor_shows_the_curves_and_sends_what_was_set()
    {
        var page = Opened();
        page.Serve("/api/controls/axes", """{"how":"live","path":"x","keptBefore":"a","now":"b"}""");
        page.Do("controlsDevice = 'js4'; await renderControlsDevice();");

        var rows = "__dom.node('#controls-axes').querySelectorAll('tr').filter(tr => tr.dataset.axis)";
        Assert.Equal(2, page.Count($"{rows}.length"));
        Assert.Equal("1.5", page.Text($"{rows}.find(tr => tr.dataset.axis === 'y').querySelector('.controls-exponent').value"));
        Assert.Equal("flight_move_pitch", page.Text($"{rows}.find(tr => tr.dataset.axis === 'y').dataset.group"));
        Assert.Equal("3", page.Text($"{rows}.find(tr => tr.dataset.axis === 'x').querySelector('.controls-deadzone').value"));
        Assert.Contains("Apply to the profile", page.NodeText("#controls-axes"));

        page.Do($"{{ const rx = {rows}.find(tr => tr.dataset.axis === 'x'); rx.querySelector('.controls-exponent').value = '2.25'; rx.querySelector('.controls-invert').checked = true; rx.querySelector('.controls-deadzone').value = '5'; await controlsApplyAxes(controlsSticks().find(d => d.key === 'js4'), __dom.node('#controls-keep')); }}");
        var sent = page.BodyOf("/api/controls/axes");
        Assert.Contains("\"instance\":4", sent);
        Assert.Contains("{\"option\":\"flight_move_yaw\",\"exponent\":2.25,\"inverted\":true}", sent);
        Assert.Contains("\"x\":0.05", sent);
        Assert.Contains("\"how\":\"live\"", sent);
    }

    /// <summary>
    /// Mapping: a control given an action from the stick's table, an action
    /// taken off a stick from the Actions pane, both staged into one list
    /// with the clash the game would flag named, and applied as one write.
    /// </summary>
    [Fact]
    public void Binding_changes_stage_from_either_pane_and_apply_together()
    {
        var page = Opened();
        page.Serve("/api/controls/bindings", """{"how":"live","path":"x","changed":2,"keptBefore":"a","now":"b"}""");
        page.Do("controlsDevice = 'js4'; await renderControlsDevice(); controlsPending = [];");
        Assert.True(page.Truth("__dom.node('#controls-pending').hidden"));

        // Button 6 (nothing on it) gets Self destruct, from the stick's table.
        page.Do("{ const row14 = __dom.node('#controls-buttons tbody').children.find(tr => tr.dataset.control === 'button6'); const pick14 = row14.querySelector('.controls-action-pick'); pick14.value = 'seat_general/v_self_destruct'; pick14.listeners.change[0](); }");
        Assert.False(page.Truth("__dom.node('#controls-pending').hidden"));
        Assert.Contains("1 change to apply", page.NodeText("#controls-pending-title"));
        Assert.Contains("Joystick - HOTAS Warthog button6 → Self destruct", page.NodeText("#controls-pending-list"));

        // Eject taken off button 4, from the Actions pane.
        page.Do("{ renderControlsActions(); const ejectRow = __dom.node('#controls-actions tbody').children.find(tr => tr.textContent.includes('Eject')); ejectRow.querySelectorAll('button')[0].click(); }");
        Assert.Contains("2 changes to apply", page.NodeText("#controls-pending-title"));
        Assert.Contains("Eject: take off Joystick - HOTAS Warthog button4", page.NodeText("#controls-pending-list"));

        // Staging a second action on the same control replaces the first, not adds.
        page.Do("controlsStage({actionMap:'seat_general', action:'v_light_amplification_toggle', input:'js4_button6', label:'Light amplification'});");
        Assert.Contains("3 changes", page.NodeText("#controls-pending-title"));
        page.Do("controlsStage({actionMap:'seat_general', action:'v_light_amplification_toggle', input:'js4_button15', label:'Light amplification'});");
        Assert.Contains("3 changes", page.NodeText("#controls-pending-title"));
        Assert.DoesNotContain("button6 → Light", page.NodeText("#controls-pending-list"));

        // A clash the game would flag: Yaw's group already has Pitch on... no; the same control in the same group.
        page.Do("controlsStage({actionMap:'spaceship_targeting_advanced', action:'v_target_cycle_hostile_fwd', input:'js4_button4', label:'Cycle Lock - Hostiles - Forward'});");
        page.Do("controlsStage({actionMap:'seat_general', action:'v_self_destruct', input:'js4_button4', label:'Self destruct'});");
        Assert.Contains("also Eject in the same group", page.NodeText("#controls-pending-list"));

        page.Do("await controlsApplyBindings(__dom.node('#controls-pending-apply'));");
        var sent = page.BodyOf("/api/controls/bindings");
        Assert.Contains("\"action\":\"v_self_destruct\",\"input\":\"js4_button4\",\"remove\":false", sent);
        Assert.Contains("\"action\":\"v_eject\",\"input\":\"js4_button4\",\"remove\":true", sent);
        Assert.Contains("\"how\":\"live\"", sent);
        Assert.True(page.Truth("__dom.node('#controls-pending').hidden"));
    }

    [Fact]
    public void With_the_game_running_the_apply_becomes_an_import_file()
    {
        var page = new Page();
        page.Serve("/api/controls", Model.Replace("\"ready\":true,", "\"ready\":true,\"gameRunning\":true,"));
        page.Serve("/api/controls/backups", "[]");
        page.Do("await loadControls(); controlsStage({actionMap:'seat_general', action:'v_self_destruct', input:'js4_button6', label:'Self destruct'});");

        Assert.Contains("Apply as an import file", page.NodeText("#controls-pending-apply"));
        Assert.Contains("Star Citizen is running", page.NodeText("#controls-pending-note"));
        page.Do("controlsPending = [];");
    }

    /// <summary>
    /// The export asks for the sticks as they are now; a stick left where it
    /// was sends nothing, and the game's folder is written only on the
    /// second, explicit press.
    /// </summary>
    [Fact]
    public void The_export_sends_only_the_sticks_that_moved_and_installs_only_when_asked()
    {
        var page = Opened("backups");
        page.Serve("/api/controls/export", """{"name":"quantumwake","path":"C:\\data\\controls\\exports\\quantumwake.xml","installed":false,"mappings":null,"command":"pp_rebindkeys quantumwake"}""");
        page.Do("controlsExportSource = '20260916-235612-417-a1b2c3d4'; renderControlsExport();");

        Assert.Contains("from the copy 20260916", page.NodeText("#controls-export-source"));
        Assert.Equal(2, page.Count("__dom.node('#controls-retarget').querySelectorAll('select').length"));

        page.Do("const s = __dom.node('#controls-retarget').querySelectorAll('select'); s[0].value = '3'; await controlsWriteExport(false);");
        var sent = page.BodyOf("/api/controls/export");
        Assert.Contains("\"source\":\"20260916-235612-417-a1b2c3d4\"", sent);
        Assert.Contains("\"retarget\":{\"2\":3}", sent);
        Assert.Contains("\"install\":false", sent);
        Assert.Contains("Not yet where the game reads", page.NodeText("#controls-export-result"));
        Assert.False(page.Truth("__dom.node('#controls-export-install').hidden"));
    }

    [Fact]
    public void Pictures_say_where_they_come_from_and_each_stick_gets_a_choice()
    {
        var page = Opened("pictures");

        Assert.Contains("on · 3 in the library", page.NodeText("#controls-templates-status"));
        Assert.True(page.Truth("__dom.node('#controls-templates-enable').hidden"));
        Assert.False(page.Truth("__dom.node('#controls-templates-disable').hidden"));
        var assign = page.NodeText("#controls-assign");
        Assert.Contains("Throttle - HOTAS Warthog (js2)", assign);
        Assert.Contains("Joystick - HOTAS Warthog (js4)", assign);
        Assert.Equal(2, page.Count("__dom.node('#controls-assign').querySelectorAll('select').length"));
    }

    [Fact]
    public void With_the_library_off_the_page_says_so_and_offers_to_fetch_it()
    {
        var page = new Page();
        page.Serve("/api/controls", Model.Replace("\"enabled\":true", "\"enabled\":false"));
        page.Serve("/api/controls/backups", "[]");
        page.Do("showControlsPane('pictures'); await loadControls();");

        Assert.False(page.Truth("__dom.node('#controls-templates-enable').hidden"));
        Assert.Contains("off", page.NodeText("#controls-templates-status"));
    }

    [Fact]
    public void Before_the_install_is_read_the_page_says_so()
    {
        var page = new Page();
        page.Serve("/api/controls", """{"ready":false,"profileFound":false,"catalogue":{"maps":[],"actions":[]},"layouts":[],"templates":{"enabled":false,"available":[],"assignments":{}},"backupCount":0}""");
        page.Do("await loadControls();");

        Assert.False(page.Truth("__dom.node('#controls-unready').hidden"));
        Assert.True(page.Truth("__dom.node('#controls-pane-devices').hidden"));
    }
}
