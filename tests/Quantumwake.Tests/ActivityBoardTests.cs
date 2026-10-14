using System.Text.Json;
using Quantumwake.Server;

namespace Quantumwake.Tests;

public class ActivityBoardTests
{
    private static string[] Keys(ActivityBoard board) =>
        [.. JsonSerializer.SerializeToElement(board.Snapshot())
            .EnumerateArray()
            .Select(job => job.GetProperty("key").GetString()!)];

    [Fact]
    public void A_job_is_listed_while_it_runs_and_gone_once_it_ends()
    {
        var board = new ActivityBoard();

        using (board.Track("community"))
            Assert.Equal(["community"], Keys(board));

        Assert.Empty(Keys(board));
    }

    /// <summary>
    /// A UEX fetch from Settings can overlap the automatic refresh. The first
    /// to finish must not announce the prices as fetched while the other is
    /// still on its way.
    /// </summary>
    [Fact]
    public void Two_fetches_of_the_same_thing_are_counted_separately()
    {
        var board = new ActivityBoard();

        var manual = board.Track("uex");
        var automatic = board.Track("uex");

        manual.Dispose();
        Assert.Equal(["uex"], Keys(board));

        // Disposing twice is a using block plus a finally, not a second job ending.
        manual.Dispose();
        Assert.Equal(["uex"], Keys(board));

        automatic.Dispose();
        Assert.Empty(Keys(board));
    }

    [Fact]
    public void A_feed_carries_its_title_for_the_page_to_name()
    {
        var board = new ActivityBoard();

        using var job = board.Track("uex-feed", "Fuel prices");

        var only = JsonSerializer.SerializeToElement(board.Snapshot())[0];
        Assert.Equal("Fuel prices", only.GetProperty("detail").GetString());
    }

    /// <summary>
    /// The startup scan waits for the game files; until it begins, an idle
    /// status must not read as a scan already done.
    /// </summary>
    [Fact]
    public void A_scan_that_has_never_begun_is_not_one_that_has_finished()
    {
        var scan = new ScanStatus();
        Assert.False(scan.HasStarted);

        scan.Begin();
        scan.Finish();
        Assert.True(scan.HasStarted);
    }
}
