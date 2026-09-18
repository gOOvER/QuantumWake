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
        page.Do("{ const row14 = __dom.node('#controls-buttons tbody').children.find(tr => tr.dataset.control === 'button6'); const search14 = row14.querySelector('.controls-action-search'); search14.value = 'self destruct'; search14.listeners.input[0](); row14.querySelectorAll('.controls-action-result').find(b => b.textContent.includes('Self destruct')).click(); }");
        Assert.False(page.Truth("__dom.node('#controls-pending').hidden"));
        Assert.Contains("1 change to apply", page.NodeText("#controls-pending-title"));
        Assert.Contains("Joystick - HOTAS Warthog button6 → Self destruct", page.NodeText("#controls-pending-list"));

        // Eject taken off button 4, from the Actions pane.
        page.Do("{ renderControlsActions(); const ejectRow = __dom.node('#controls-actions tbody').children.find(tr => tr.textContent.includes('Eject')); ejectRow.querySelectorAll('button')[0].click(); }");
        Assert.Contains("2 changes to apply", page.NodeText("#controls-pending-title"));
        Assert.Contains("Eject: unbind from Joystick - HOTAS Warthog button4", page.NodeText("#controls-pending-list"));

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

    /// <summary>
    /// The live read: a pressed button lights its row and the grid's cell,
    /// the note names it and the stick's true counts, and a stick that is
    /// not plugged in says so rather than lighting nothing quietly.
    /// </summary>
    [Fact]
    public void A_pressed_button_lights_the_row_and_the_grid_and_the_counts_come_from_windows()
    {
        var page = Opened();
        page.Serve("/api/controls/live", """
            {"available":true,"devices":[
              {"id":"a","guid":"{0402044F-0000-0000-0000-504944564944}","vendorId":1103,"productId":1026,"name":"HID-compliant game controller","buttonCount":19,"axisCount":2,"hatCount":1,"keys":["js4"],"product":"Joystick - HOTAS Warthog","ambiguous":false,"pressed":[7],"hats":["up"],"axes":[0.25,-0.5]}],
             "missing":[{"key":"js2","product":"Throttle - HOTAS Warthog","guid":"{0404044F-0000-0000-0000-504944564944}"}]}
            """);
        page.Do("__dom.node('#view-controls').classList.add('active'); controlsDevice = 'js4'; await controlsLiveTick(); controlsLiveStop(); await renderControlsDevice(); paintControlsLive();");

        var note = page.NodeText("#controls-live-note");
        Assert.Contains("Live · 19 buttons, 2 axes, 1 hat", note);
        Assert.Contains("pressed: Button 7, Hat 1 up", note);
        Assert.True(page.Truth("__dom.node('#controls-buttons tbody').children.find(tr => tr.dataset.control === 'button7').classList.contains('lit')"));
        Assert.False(page.Truth("__dom.node('#controls-buttons tbody').children.find(tr => tr.dataset.control === 'button4').classList.contains('lit')"));
        // The grid runs to Windows' count now, not a guess, and has the hat.
        Assert.Contains("Buttons · 19, as Windows reports the stick", page.NodeText("#controls-svg"));
        Assert.Equal(19 + 4, page.Count("__dom.node('#controls-svg').querySelectorAll('.controls-grid-button').length"));
        Assert.True(page.Truth("__dom.node('#controls-svg').querySelectorAll('.controls-grid-button').find(n => n.dataset.control === 'button7').classList.contains('lit')"));

        page.Do("controlsDevice = 'js2'; await renderControlsDevice(); paintControlsLive();");
        Assert.Contains("not plugged in right now", page.NodeText("#controls-live-note"));
    }

    [Fact]
    public void Under_the_bare_server_the_live_note_says_who_reads_the_sticks()
    {
        var page = Opened();
        page.Serve("/api/controls/live", """{"available":false,"reason":"The dashboard is running under the bare server; the sticks are read by QuantumWake.exe.","devices":[]}""");
        page.Do("__dom.node('#view-controls').classList.add('active'); await controlsLiveTick(); controlsLiveStop();");
        Assert.Contains("read by QuantumWake.exe", page.NodeText("#controls-live-note"));
    }

    [Fact]
    public void A_staged_binding_carries_the_mode_it_was_given()
    {
        var page = Opened();
        page.Serve("/api/controls/bindings", """{"how":"live","changed":1}""");
        page.Do("controlsDevice = 'js4'; await renderControlsDevice(); controlsPending = []; { const r6 = __dom.node('#controls-buttons tbody').children.find(tr => tr.dataset.control === 'button6'); r6.querySelector('.controls-mode-pick').value = 'hold'; const search = r6.querySelector('.controls-action-search'); search.value = 'self destruct'; search.listeners.input[0](); r6.querySelectorAll('.controls-action-result').find(b => b.textContent.includes('Self destruct')).click(); }");

        Assert.Contains("button6 → Self destruct (hold)", page.NodeText("#controls-pending-list"));
        page.Do("await controlsApplyBindings(__dom.node('#controls-pending-apply'));");
        Assert.Contains("\"activationMode\":\"hold\"", page.BodyOf("/api/controls/bindings"));
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

    // The action finder: a searchable, browsable replacement for the select
    // that used to carry every action the game knows in every control row.
    // Six sticks made that tens of thousands of options; these pin the two
    // ways in - typing, and opening the box with nothing typed - and the
    // threshold between them.
    private static Page WithPicker()
    {
        var page = Opened();
        page.Do("""
            controlsDevice = 'js4'; await renderControlsDevice();
            globalThis.__row = __dom.node('#controls-buttons tbody').children.find(tr => tr.dataset.control === 'button6');
            globalThis.__find = __row.querySelector('.controls-action-search');
            globalThis.__list = __row.querySelector('.controls-action-results');
            globalThis.__type = (text) => { __find.value = text; __find.listeners.input[0](); };
            globalThis.__press = (key) => __find.listeners.keydown[0]({ key, preventDefault() {}, stopPropagation() {} });
            """);
        return page;
    }

    [Fact]
    public void One_letter_is_not_a_search_so_the_box_still_browses()
    {
        var page = WithPicker();
        page.Do("__type('s');");

        // Two letters is the threshold; below it nothing is matched, and the
        // box shows the groups rather than going blank.
        Assert.Equal(0, page.Count("__row.querySelectorAll('.controls-action-result').length"));
        Assert.Equal(3, page.Count("__row.querySelectorAll('.controls-action-group').length"));
        Assert.False(page.Truth("__list.hidden"));
    }

    [Fact]
    public void Two_letters_matches_actions_and_names_the_group_each_is_in()
    {
        var page = WithPicker();
        page.Do("__type('eje');");

        Assert.Equal(1, page.Count("__row.querySelectorAll('.controls-action-result').length"));
        Assert.Contains("Eject", page.Text("__row.querySelector('.controls-action-result').textContent"));
        // The group is on the row too: two actions can share a label, and the
        // map is what tells them apart.
        Assert.Contains("Seat - General", page.Text("__row.querySelector('.controls-action-result').textContent"));
    }

    [Fact]
    public void A_search_matches_the_group_as_well_as_the_action()
    {
        var page = WithPicker();
        page.Do("__type('targeting');");

        // "Cycle Lock - Hostiles - Forward" carries none of that word; the map
        // it lives in does, which is how a pilot who knows the area but not
        // the name finds it.
        Assert.Equal(1, page.Count("__row.querySelectorAll('.controls-action-result').length"));
        Assert.Contains("Cycle Lock", page.Text("__row.querySelector('.controls-action-result').textContent"));
    }

    [Fact]
    public void A_search_that_matches_nothing_says_so_rather_than_emptying()
    {
        var page = WithPicker();
        page.Do("__type('quantum drive spool');");

        Assert.Equal(0, page.Count("__row.querySelectorAll('.controls-action-result').length"));
        Assert.False(page.Truth("__list.hidden"));
        Assert.Contains("No action matches", page.Text("__list.textContent"));
    }

    [Fact]
    public void An_empty_box_browses_the_groups_and_counts_what_is_in_each()
    {
        var page = WithPicker();
        page.Do("__find.listeners.focus[0]();");

        Assert.Equal(3, page.Count("__row.querySelectorAll('.controls-action-group').length"));
        // Flight - Movement holds four of the fixture's eight actions.
        Assert.Contains("4 actions", page.Text("[...__row.querySelectorAll('.controls-action-group')].find(b => b.textContent.includes('Flight - Movement')).textContent"));
    }

    [Fact]
    public void Opening_a_group_lists_its_actions_and_offers_the_way_back()
    {
        var page = WithPicker();
        page.Do("""
            __find.listeners.focus[0]();
            [...__row.querySelectorAll('.controls-action-group')].find(b => b.textContent.includes('Flight - Targeting')).listeners.click[0]();
            """);

        Assert.Equal(1, page.Count("__row.querySelectorAll('.controls-action-result').length"));
        Assert.Equal(1, page.Count("__row.querySelectorAll('.controls-action-back').length"));
        Assert.Equal(0, page.Count("__row.querySelectorAll('.controls-action-group').length"));

        page.Do("__row.querySelector('.controls-action-back').listeners.click[0]();");
        Assert.Equal(3, page.Count("__row.querySelectorAll('.controls-action-group').length"));
    }

    [Fact]
    public void An_action_chosen_while_browsing_stages_the_same_change_as_one_searched()
    {
        var page = WithPicker();
        page.Do("""
            controlsPending = [];
            __find.listeners.focus[0]();
            [...__row.querySelectorAll('.controls-action-group')].find(b => b.textContent.includes('Flight - Targeting')).listeners.click[0]();
            __row.querySelector('.controls-action-result').listeners.click[0]();
            """);

        Assert.Contains("button6 → Cycle Lock - Hostiles - Forward", page.NodeText("#controls-pending-list"));
        // Choosing closes the list and empties the box, so the row is ready
        // for the next one rather than still showing the last answer.
        Assert.True(page.Truth("__list.hidden"));
        Assert.Equal("", page.Text("__find.value"));
    }

    [Fact]
    public void Escape_closes_the_list_without_staging_anything()
    {
        var page = WithPicker();
        page.Do("controlsPending = []; __type('eje'); __press('Escape');");

        Assert.True(page.Truth("__list.hidden"));
        Assert.Equal(0, page.Count("__row.querySelectorAll('.controls-action-result').length"));
        Assert.True(page.Truth("__dom.node('#controls-pending').hidden"));
    }

    [Fact]
    public void Enter_takes_the_first_match()
    {
        var page = WithPicker();
        page.Do("controlsPending = []; __type('eje'); __press('Enter');");

        Assert.Contains("button6 → Eject", page.NodeText("#controls-pending-list"));
    }

    [Fact]
    public void The_take_off_button_is_only_on_a_control_that_has_something_on_it()
    {
        var page = Opened();
        page.Do("controlsDevice = 'js4'; await renderControlsDevice();");

        // button4 carries Eject; button6 carries nothing, so there is nothing
        // to unbind and no button offering to.
        Assert.Equal(1, page.Count("__dom.node('#controls-buttons tbody').children.find(tr => tr.dataset.control === 'button4').querySelectorAll('.controls-action-remove').length"));
        Assert.Equal(0, page.Count("__dom.node('#controls-buttons tbody').children.find(tr => tr.dataset.control === 'button6').querySelectorAll('.controls-action-remove').length"));
    }

    // controlsFitLabel: the action name shrunk to sit inside the box drawn on
    // the stick's picture. It is pinned with a fake node rather than a drawn
    // SVG because what matters is the arithmetic, and the arithmetic is what
    // went wrong - "Landing System (Toggle)" rendered three lines and spilled
    // out of the bottom of button 26 on the Warthog throttle.
    private static Page Fitter()
    {
        var page = new Page();
        page.Do("""
            globalThis.__label = (text, width) => {
              const node = { namespaceURI: 'http://www.w3.org/1999/xhtml', style: { width: width + 'px' }, parentElement: null, textContent: '' };
              controlsFitLabel(node, text);
              return node;
            };
            globalThis.__size = (text, width) => parseFloat(__label(text, width).style.fontSize);
            """);
        return page;
    }

    [Fact]
    public void A_label_that_fits_on_one_line_is_left_at_full_size()
    {
        var page = Fitter();

        Assert.Equal(10, page.Number("__size('Autoland', 78)"));
        Assert.Equal("Autoland", page.Text("__label('Autoland', 78).textContent"));
    }

    [Fact]
    public void A_label_whose_words_will_not_share_a_line_is_shrunk_until_they_do()
    {
        var page = Fitter();

        // The bug: dividing 23 characters by a 70px line says two lines, so
        // this was left at 10px and wrapped to three. No word here can be
        // broken, so it has to come down.
        Assert.True(page.Number("__size('Landing System (Toggle)', 78)") < 10,
            "a label that cannot pack into two lines at 10px must shrink");
        // ...and only as far as it needs to; the floor is 6.5.
        Assert.True(page.Number("__size('Landing System (Toggle)', 78)") >= 7);
    }

    [Fact]
    public void Shrinking_is_preferred_to_cutting_the_name_short()
    {
        var page = Fitter();

        // An ellipsis loses which action it is; a smaller font does not. The
        // name survives whole whenever two lines can hold it.
        Assert.DoesNotContain("…", page.Text("__label('Landing System (Toggle)', 78).textContent"));
        Assert.Equal("Landing System (Toggle)", page.Text("__label('Landing System (Toggle)', 78).textContent"));
        Assert.DoesNotContain("…", page.Text("__label('Activate Ping (Hold & Release)', 78).textContent"));
    }

    [Fact]
    public void The_split_is_on_whitespace_not_on_a_letter()
    {
        var page = Fitter();

        // Guards a regex that lost its backslash and split on the letter "s":
        // it made "Landing System (Toggle)" look like two short words that fit
        // and left it at full size, while labels with no lowercase s shrank.
        // Two labels of the same length, one with an s and one without, must
        // be treated the same.
        var withS = page.Number("__size('Master System Toggle', 78)");
        var without = page.Number("__size('Maxter Rextem Toggle', 78)");
        Assert.Equal(without, withS);
    }

    [Fact]
    public void A_word_too_long_for_the_line_is_cut_rather_than_left_to_spill()
    {
        var page = Fitter();

        // Nothing can wrap a single word, so once the floor is reached the
        // only honest answer is to cut it and keep the full name in the table.
        var text = page.Text("__label('Countermeasure_Decoy_Panic_Sequence', 60).textContent");
        Assert.Contains("…", text);
        Assert.Equal(6.5, page.Number("__size('Countermeasure_Decoy_Panic_Sequence', 60)"));
    }

    [Fact]
    public void A_narrow_box_shrinks_a_label_further_than_a_wide_one()
    {
        var page = Fitter();

        Assert.True(page.Number("__size('Set Master Mode to SCM', 48)") < page.Number("__size('Set Master Mode to SCM', 110)"));
    }
}
