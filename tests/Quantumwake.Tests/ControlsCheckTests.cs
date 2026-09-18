namespace Quantumwake.Tests;

using Quantumwake.Core.Controls;

/// <summary>
/// What a stick is missing against the layouts the game ships for it.
/// </summary>
/// <remarks>
/// The numbers these are cut from are this install's, measured 2026-09-18:
/// the Warthog layout binds 73 actions, 29 are not on the pilot's sticks, 16
/// of those were cleared on purpose, 12 name actions the patch renamed, and
/// one is a gap. Each filter has a test because removing any one of them
/// takes the answer from 1 to something between 13 and 29, all of it wrong.
/// </remarks>
public class ControlsCheckTests
{
    private const string Guid4 = "{0402044F-0000-0000-0000-504944564944}";

    private static ControlDevice Stick(int instance, string product, string guid) =>
        new("joystick", instance, product, guid, new Dictionary<string, double>(), []);

    private static ControlBinding Bind(string action, string raw, string map = "spaceship_movement") =>
        new(map, action, ControlInput.Parse(raw), null, null);

    private static ControlCatalogue Catalogue(params string[] actions) =>
        new([new ActionMapInfo("spaceship_movement", "Flight - Movement", "FLIGHT")],
            actions.Select(a => new ActionInfo("spaceship_movement", a, a.Replace('_', ' '), "", "", "", "", "", "", "")).ToList());

    private static ControlProfile Mine(params ControlBinding[] bindings) =>
        new("default", null, [Stick(4, "Joystick - HOTAS Warthog", Guid4)], bindings);

    // The reference names the same product under its own instance number - the
    // game's Warthog layout calls it js1 where this pilot's profile calls it js4.
    private static (string, ControlProfile) Layout(params ControlBinding[] bindings) =>
        ("layout_hotas_warthog.xml", new ControlProfile("warthog", null, [Stick(1, "Joystick - HOTAS Warthog", Guid4)], bindings));

    [Fact]
    public void An_action_the_layout_binds_and_the_pilot_does_not_is_a_gap()
    {
        var result = ControlsCheck.Against(
            Mine(Bind("v_pitch", "js4_y")),
            [Layout(Bind("v_pitch", "js1_y"), Bind("v_eject", "js1_button4"))],
            Catalogue("v_pitch", "v_eject"));

        var gap = Assert.Single(result.Gaps);
        Assert.Equal("v_eject", gap.Action);
        Assert.Equal("js4", gap.DeviceKey);
        Assert.Equal(1, result.Counts.Bound);
    }

    [Fact]
    public void The_suggestion_is_moved_onto_the_pilots_own_instance_number()
    {
        var result = ControlsCheck.Against(
            Mine(),
            [Layout(Bind("v_eject", "js1_button4"))],
            Catalogue("v_eject"));

        // The layout says js1_button4; this pilot's Warthog is js4, and a
        // suggestion naming js1 would land on their MFD.
        Assert.Equal("js4_button4", Assert.Single(result.Gaps).Suggested);
    }

    [Fact]
    public void A_default_the_pilot_took_away_on_purpose_is_not_a_gap()
    {
        // 16 of this install's 29 differences are these. The profile writes a
        // removed default as jsN_ with no control, which is a decision, and
        // asking for it back says the pilot was wrong.
        var result = ControlsCheck.Against(
            Mine(Bind("v_roll_left", "js4_")),
            [Layout(Bind("v_roll_left", "js1_button2"))],
            Catalogue("v_roll_left"));

        Assert.Empty(result.Gaps);
        Assert.Equal(1, result.Counts.Cleared);
    }

    [Fact]
    public void An_action_this_patch_no_longer_defines_is_not_a_gap()
    {
        // 12 of the 29. The game renamed v_ifcs_toggle_vector_decoupling to
        // v_ifcs_vector_decoupling_toggle and left the shipped layout alone,
        // so the layout asks for a name nothing answers to any more.
        var result = ControlsCheck.Against(
            Mine(Bind("v_ifcs_vector_decoupling_toggle", "js4_button4")),
            [Layout(Bind("v_ifcs_toggle_vector_decoupling", "js1_button4"))],
            Catalogue("v_ifcs_vector_decoupling_toggle"));

        Assert.Empty(result.Gaps);
        Assert.Equal(1, result.Counts.Undefined);
    }

    [Fact]
    public void An_empty_catalogue_does_not_throw_every_suggestion_away()
    {
        // Before the game data is read the catalogue is empty, and a filter
        // that cannot run must not answer "everything is stale".
        var result = ControlsCheck.Against(
            Mine(),
            [Layout(Bind("v_eject", "js1_button4"))],
            ControlCatalogue.Empty);

        Assert.Single(result.Gaps);
        Assert.Equal(0, result.Counts.Undefined);
    }

    [Fact]
    public void A_dismissed_gap_stays_dismissed()
    {
        var result = ControlsCheck.Against(
            Mine(),
            [Layout(Bind("v_eject", "js1_button4"))],
            Catalogue("v_eject"),
            new HashSet<string> { "js4/spaceship_movement/v_eject" });

        Assert.Empty(result.Gaps);
        Assert.Equal(1, result.Counts.Dismissed);
    }

    [Fact]
    public void An_action_bound_on_another_stick_is_not_missing_from_this_one()
    {
        // The layout puts it on the stick and the pilot put it on the throttle.
        // That is a different arrangement, not a gap.
        var mine = new ControlProfile("default", null,
            [Stick(4, "Joystick - HOTAS Warthog", Guid4), Stick(2, "Throttle - HOTAS Warthog", "{0404044F-0000-0000-0000-504944564944}")],
            [Bind("v_eject", "js2_button7")]);

        Assert.Empty(ControlsCheck.Against(mine, [Layout(Bind("v_eject", "js1_button4"))], Catalogue("v_eject")).Gaps);
    }

    [Fact]
    public void A_reference_for_a_stick_the_pilot_does_not_own_is_ignored()
    {
        var other = ("layout_t16000m.xml", new ControlProfile("t16000", null,
            [Stick(1, "T.16000M", "{B10A044F-0000-0000-0000-504944564944}")], new[] { Bind("v_eject", "js1_button4") }));

        var result = ControlsCheck.Against(Mine(), [other], Catalogue("v_eject"));

        Assert.Empty(result.Gaps);
        Assert.Empty(result.Sources);
        Assert.Equal(0, result.Counts.Reference);
    }

    [Fact]
    public void The_list_leads_with_the_maps_the_pilot_actually_flies_in()
    {
        var catalogue = new ControlCatalogue(
            [new ActionMapInfo("spaceship_movement", "Flight - Movement", "FLIGHT"),
             new ActionMapInfo("vehicle_ground", "Ground Vehicle", "")],
            [new ActionInfo("spaceship_movement", "v_eject", "Eject", "", "", "", "", "", "", ""),
             new ActionInfo("vehicle_ground", "v_horn", "Horn", "", "", "", "", "", "", "")]);

        // Thirty bindings in flight, none on the ground: a flight gap is the
        // one worth reading first.
        var mine = new ControlProfile("default", null, [Stick(4, "Joystick - HOTAS Warthog", Guid4)],
            Enumerable.Range(1, 30).Select(i => Bind($"v_filler{i}", $"js4_button{i}")).ToList());

        var result = ControlsCheck.Against(
            mine,
            [Layout(new ControlBinding("vehicle_ground", "v_horn", ControlInput.Parse("js1_button9"), null, null),
                    Bind("v_eject", "js1_button4"))],
            catalogue);

        Assert.Equal(["Eject", "Horn"], result.Gaps.Select(g => g.Label));
    }

    [Fact]
    public void The_same_action_in_two_references_is_reported_once()
    {
        var result = ControlsCheck.Against(
            Mine(),
            [Layout(Bind("v_eject", "js1_button4")),
             ("layout_hotas_warthog_pedals.xml", new ControlProfile("pedals", null,
                 [Stick(1, "Joystick - HOTAS Warthog", Guid4)], new[] { Bind("v_eject", "js1_button4") }))],
            Catalogue("v_eject"));

        Assert.Single(result.Gaps);
        Assert.Equal(2, result.Sources.Count);
    }

    [Fact]
    public void The_counts_add_up_so_a_short_list_reads_as_a_short_list()
    {
        var result = ControlsCheck.Against(
            Mine(Bind("v_pitch", "js4_y"), Bind("v_roll_left", "js4_")),
            [Layout(Bind("v_pitch", "js1_y"), Bind("v_roll_left", "js1_button2"),
                    Bind("v_gone", "js1_button3"), Bind("v_eject", "js1_button4"))],
            Catalogue("v_pitch", "v_roll_left", "v_eject"));

        var c = result.Counts;
        Assert.Equal(4, c.Reference);
        Assert.Equal(1, c.Bound);
        Assert.Equal(1, c.Cleared);
        Assert.Equal(1, c.Undefined);
        Assert.Equal(1, c.Gaps);
        Assert.Equal(c.Reference, c.Bound + c.Cleared + c.Undefined + c.Dismissed + c.Gaps);
    }
}
