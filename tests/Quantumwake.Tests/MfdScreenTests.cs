using Quantumwake.Overlay;

namespace Quantumwake.Tests;

/// <summary>
/// Finding the MFD screen again after Windows renumbers the displays.
/// </summary>
/// <remarks>
/// The monitors are the ones this was found on: a layout saved on the 1366×768
/// Cougar screen as <c>DISPLAY1</c>, on a machine where <c>DISPLAY1</c> had
/// since become the 3440×1440 main monitor. The frames and their black
/// backdrop went there, over the taskbar and the tray icon that was the only
/// way back into setup.
/// </remarks>
public class MfdScreenTests
{
    private static readonly MfdMonitor[] Desk =
    [
        new(@"\\.\DISPLAY3", 3440, 374, 1920, 1080, false),
        new(@"\\.\DISPLAY1", 0, 0, 3440, 1440, true),
        new(@"\\.\DISPLAY2", 5360, 680, 1366, 768, false),
    ];

    private static MfdLayout Saved(string monitor, int? width, int? height) => new()
    {
        Enabled = true,
        Panels =
        [
            new() { Id = "left", Monitor = monitor, X = 0, Y = 157, Width = 601, Height = 591, Cougar = 1,
                MonitorWidth = width, MonitorHeight = height },
            new() { Id = "right", Monitor = monitor, X = 725, Y = 147, Width = 631, Height = 601, Cougar = 2,
                MonitorWidth = width, MonitorHeight = height },
        ],
    };

    [Fact]
    public void A_renumbered_screen_is_found_again_by_its_size()
    {
        var layout = Saved(@"\\.\DISPLAY1", 1366, 768).Validate(Desk);

        Assert.All(layout.Panels, p => Assert.Equal(@"\\.\DISPLAY2", p.Monitor));
        Assert.Equal(@"\\.\DISPLAY2", MfdLayout.Locate(layout.Panels[0], Desk)?.Id);
    }

    [Fact]
    public void A_name_that_still_fits_is_trusted()
    {
        var panel = Saved(@"\\.\DISPLAY2", 1366, 768).Panels[0];

        Assert.Equal(@"\\.\DISPLAY2", MfdLayout.Locate(panel, Desk)?.Id);
    }

    /// <summary>
    /// No screen of the size, or two: the panel stays off rather than guessing,
    /// because a wrong guess is a black backdrop over the game.
    /// </summary>
    [Fact]
    public void A_screen_that_cannot_be_told_apart_leaves_the_panel_off()
    {
        Assert.Null(MfdLayout.Locate(Saved(@"\\.\DISPLAY1", 1280, 1024).Panels[0], Desk));

        MfdMonitor[] twins = [.. Desk, new(@"\\.\DISPLAY4", 9000, 0, 1366, 768, false)];
        Assert.Null(MfdLayout.Locate(Saved(@"\\.\DISPLAY1", 1366, 768).Panels[0], twins));
    }

    /// <summary>A panel that cannot be placed keeps its placement for when the screen is back.</summary>
    [Fact]
    public void An_unplaced_panel_keeps_what_was_saved()
    {
        var layout = Saved(@"\\.\DISPLAY1", 1280, 1024).Validate(Desk);

        Assert.Equal(@"\\.\DISPLAY1", layout.Panels[0].Monitor);
        Assert.Equal(601, layout.Panels[0].Width);
    }

    /// <summary>A file from before sizes were kept has the name and nothing else.</summary>
    [Fact]
    public void A_file_without_sizes_goes_by_the_name()
    {
        Assert.Equal(@"\\.\DISPLAY1", MfdLayout.Locate(Saved(@"\\.\DISPLAY1", null, null).Panels[0], Desk)?.Id);
    }

    [Fact]
    public void Saving_records_the_size_of_each_panels_screen()
    {
        var stamped = Saved(@"\\.\DISPLAY2", null, null).Validate(Desk).Stamped(Desk);

        Assert.All(stamped.Panels, p => Assert.Equal((1366, 768), (p.MonitorWidth, p.MonitorHeight)));
    }

    [Fact]
    public void The_size_survives_the_file()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(Saved(@"\\.\DISPLAY2", 1366, 768), MfdLayout.JsonOptions);
        var read = System.Text.Json.JsonSerializer.Deserialize<MfdLayout>(json, MfdLayout.JsonOptions)!;

        Assert.Equal(1366, read.Panels[0].MonitorWidth);
        Assert.Equal(768, read.Panels[0].MonitorHeight);
    }

    [Fact]
    public void The_main_monitor_is_never_blacked_out()
    {
        Assert.False(MfdLayout.MayBlackOut(Desk[1]));
        Assert.True(MfdLayout.MayBlackOut(Desk[2]));
    }

    /// <summary>Every case names the shortcut, since the tray may be the thing covered.</summary>
    [Fact]
    public void A_move_is_explained_and_names_the_shortcut()
    {
        var note = MfdLayout.Explain(Saved(@"\\.\DISPLAY1", 1366, 768), Desk);

        Assert.Contains("renumbered", note);
        Assert.Contains("Ctrl+Alt+M", note);
    }

    [Fact]
    public void A_missing_screen_is_explained_with_its_size()
    {
        var note = MfdLayout.Explain(Saved(@"\\.\DISPLAY1", 1280, 1024), Desk);

        Assert.Contains("1280×1024", note);
        Assert.Contains("Ctrl+Alt+M", note);
    }

    /// <summary>
    /// The case on the machine this was found on: an older file, pointing at
    /// the main monitor while others exist. Nothing can tell that from a
    /// deliberate choice, so it is said rather than corrected.
    /// </summary>
    [Fact]
    public void An_older_file_on_the_main_monitor_is_questioned()
    {
        var note = MfdLayout.Explain(Saved(@"\\.\DISPLAY1", null, null), Desk);

        Assert.Contains("main screen", note);
        Assert.Contains("Ctrl+Alt+M", note);
    }

    [Fact]
    public void A_layout_where_it_was_saved_says_nothing()
    {
        Assert.Null(MfdLayout.Explain(Saved(@"\\.\DISPLAY2", 1366, 768), Desk));
        Assert.Null(MfdLayout.Explain(Saved(@"\\.\DISPLAY2", null, null), Desk));
        Assert.Null(MfdLayout.Explain(Saved(@"\\.\DISPLAY1", 1366, 768) with { Enabled = false }, Desk));
    }
}
