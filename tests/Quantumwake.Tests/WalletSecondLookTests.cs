using Quantumwake.Data;

namespace Quantumwake.Tests;

/// <summary>
/// The second look at the balance: when it is taken, where it looks, and what
/// it takes to be believed.
/// </summary>
/// <remarks>
/// The reader is a delegate here, fed by hand, because what is under test is
/// the policy - the engine's own behaviour on the four frames that shaped it
/// is measured in <c>Quantumwake.OcrTests</c>, where there is an engine.
/// </remarks>
public class WalletSecondLookTests
{
    /// <summary>The map frame: bar read, balance did not - the case that started this.</summary>
    private static readonly ScreenTextLine[] BarWithoutBalance = ScreenFrameFixtures.Map;

    private static readonly ScreenTextLine Figure = new("3,958,160", 990, 1294, 25);

    [Fact]
    public async Task A_frame_with_no_bar_is_not_looked_at_again()
    {
        var calls = 0;

        var lines = await WalletSecondLook.TakeAsync(ScreenFrameFixtures.Looting, (_, _, _) =>
        {
            calls++;
            return Task.FromResult<IReadOnlyList<ScreenTextLine>>([Figure]);
        });

        Assert.Equal(0, calls);
        Assert.Same(ScreenFrameFixtures.Looting, lines);
    }

    [Fact]
    public async Task A_balance_that_read_the_first_time_is_not_looked_for_again()
    {
        ScreenTextLine[] readFirstTime = [Figure, .. BarWithoutBalance];
        var calls = 0;

        var lines = await WalletSecondLook.TakeAsync(readFirstTime, (_, _, _) =>
        {
            calls++;
            return Task.FromResult<IReadOnlyList<ScreenTextLine>>([Figure]);
        });

        Assert.Equal(0, calls);
        Assert.Same(readFirstTime, lines);
    }

    /// <summary>
    /// The panel is found off the bar, in bar heights: left of HOME, around
    /// the bar's row. On the fixture HOME is at x=1283 and the row at y~1328
    /// with 12-14 px words, and the figure on the real frame sits at
    /// (959..990, 1294) - inside what is asked for, and the bar's first tile
    /// is not.
    /// </summary>
    [Fact]
    public void The_panel_asked_for_lies_left_of_the_bar_and_around_its_row()
    {
        var panel = ScreenFrames.WalletPanel(BarWithoutBalance)!;
        var home = BarWithoutBalance.Single(line => line.Text == "HOME").Left;

        Assert.NotNull(panel);
        Assert.True(panel.Left + panel.Width <= home, $"the patch reaches into the bar: {panel}");
        Assert.True(panel.Left <= 959, $"the patch starts right of the currency glyph: {panel}");
        Assert.True(panel.Top <= 1294, $"the patch starts below the figure: {panel}");
        Assert.True(panel.Top + panel.Height >= 1331 + 13, $"the patch ends above the handle: {panel}");
    }

    [Fact]
    public void The_panel_never_reaches_off_the_left_or_top_of_the_frame()
    {
        // A bar read near the corner of a small frame.
        ScreenTextLine[] cornered = [.. BarWithoutBalance.Select(line =>
            line with { Left = Math.Max(0, line.Left - 1200), Top = Math.Max(0, line.Top - 1300) })];

        var panel = ScreenFrames.WalletPanel(cornered)!;

        Assert.NotNull(panel);
        Assert.True(panel.Left >= 0);
        Assert.True(panel.Top >= 0);
        Assert.True(panel.Width > 0);
    }

    [Fact]
    public async Task Two_rungs_agreeing_put_the_balance_on_the_frame()
    {
        var asked = new List<ScreenTreatment>();

        var lines = await WalletSecondLook.TakeAsync(BarWithoutBalance, (_, treatment, _) =>
        {
            asked.Add(treatment);
            return Task.FromResult<IReadOnlyList<ScreenTextLine>>([Figure, new("NEKRON", 981, 1324, 13)]);
        });

        Assert.Equal(WalletSecondLook.Treatments.Take(2), asked);

        var wallet = ScreenFrames.Read(lines, [], []).Wallet!;
        Assert.Equal(3_958_160, wallet.Balance);
        Assert.Null(wallet.Trouble);
    }

    /// <summary>
    /// Binarising the crop once read 3,958,160 as 4,958,160. A lone read is a
    /// lone read whatever produced it, and a wrong balance is worse than none,
    /// so a figure with one vote never becomes the wallet.
    /// </summary>
    [Fact]
    public async Task One_rung_alone_is_not_believed()
    {
        var rung = 0;

        var lines = await WalletSecondLook.TakeAsync(BarWithoutBalance, (_, _, _) =>
        {
            rung++;
            return Task.FromResult<IReadOnlyList<ScreenTextLine>>(
                rung == 1 ? [new("4,958,160", 990, 1294, 25)] : []);
        });

        Assert.Equal(WalletSecondLook.Treatments.Count, rung);

        var wallet = ScreenFrames.Read(lines, [], []).Wallet!;
        Assert.Null(wallet.Balance);
        Assert.Equal(ScreenFrames.WalletTrouble, wallet.Trouble);
    }

    [Fact]
    public async Task A_rung_that_disagrees_is_outvoted_rather_than_averaged()
    {
        var rung = 0;

        var lines = await WalletSecondLook.TakeAsync(BarWithoutBalance, (_, _, _) =>
        {
            rung++;
            return Task.FromResult<IReadOnlyList<ScreenTextLine>>(rung switch
            {
                1 => [new("4,958,160", 990, 1294, 25)],
                2 => [new("3,958.160", 990, 1294, 25)],
                _ => [new("3,958,160", 990, 1294, 25)],
            });
        });

        Assert.Equal(3, rung);
        Assert.Equal(3_958_160, ScreenFrames.Read(lines, [], []).Wallet!.Balance);
    }

    [Fact]
    public async Task The_same_digits_twice_in_one_rung_are_one_vote()
    {
        var rung = 0;

        var lines = await WalletSecondLook.TakeAsync(BarWithoutBalance, (_, _, _) =>
        {
            rung++;
            return Task.FromResult<IReadOnlyList<ScreenTextLine>>(
                rung == 1 ? [Figure, Figure] : []);
        });

        Assert.Null(ScreenFrames.Read(lines, [], []).Wallet!.Balance);
    }

    [Fact]
    public async Task A_reader_that_throws_on_the_second_look_costs_only_the_second_look()
    {
        var lines = await WalletSecondLook.TakeAsync(BarWithoutBalance,
            (_, _, _) => throw new IOException("half-written"));

        Assert.Same(BarWithoutBalance, lines);
        Assert.Equal(ScreenKind.Map, ScreenFrames.Read(lines, [], []).Kind);
    }

    [Fact]
    public async Task The_added_line_is_kept_with_the_frame_so_the_reading_shows_what_was_read()
    {
        var lines = await WalletSecondLook.TakeAsync(BarWithoutBalance,
            (_, _, _) => Task.FromResult<IReadOnlyList<ScreenTextLine>>([Figure]));

        Assert.Equal(BarWithoutBalance.Length + 1, lines.Count);
        Assert.Contains("3,958,160", ScreenFrames.Read(lines, [], []).Lines);
    }
}
