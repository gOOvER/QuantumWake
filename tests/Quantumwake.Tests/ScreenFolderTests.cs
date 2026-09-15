using Quantumwake.Data;

namespace Quantumwake.Tests;

/// <summary>
/// Which screenshots the folder watch reads, and which it leaves alone.
/// </summary>
/// <remarks>
/// The rule reversed in 0.13.31: everything the app has not read is read,
/// the archive included, newest first. The watch used to start from the
/// moment it was switched on, and the only photographs of the Hermes'
/// loadout - taken the evening the watch shipped - went unread for a week.
/// </remarks>
public class ScreenFolderTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 7, 19, 30, 30, TimeSpan.Zero);

    private static ScreenFile Shot(string name, TimeSpan ago, long length = 800_000) =>
        new($@"E:\rsi\StarCitizen\LIVE\screenshots\{name}", length, Now - ago);

    [Fact]
    public void A_file_still_being_written_waits_until_it_has_settled()
    {
        var fresh = Shot("ScreenShot-a.jpg", TimeSpan.FromMilliseconds(300));
        var settled = Shot("ScreenShot-b.jpg", TimeSpan.FromSeconds(5));

        var unread = ScreenFolder.Unread([fresh, settled], Now, _ => false);

        Assert.Equal([settled], unread);
    }

    [Fact]
    public void The_archive_is_read_too_newest_first()
    {
        var archive = Shot("ScreenShot-old.jpg", TimeSpan.FromDays(3));
        var older = Shot("ScreenShot-older.jpg", TimeSpan.FromDays(9));
        var since = Shot("ScreenShot-new.jpg", TimeSpan.FromSeconds(30));

        var unread = ScreenFolder.Unread([older, since, archive], Now, _ => false);

        Assert.Equal([since, archive, older], unread);
    }

    [Fact]
    public void A_file_already_read_is_not_read_again()
    {
        var shot = Shot("ScreenShot-a.jpg", TimeSpan.FromSeconds(30));

        var unread = ScreenFolder.Unread([shot], Now, path => path.EndsWith("a.jpg"));

        Assert.Empty(unread);
    }

    [Fact]
    public void An_empty_file_is_not_a_screenshot_yet()
    {
        var empty = Shot("ScreenShot-a.jpg", TimeSpan.FromSeconds(30), length: 0);

        Assert.Empty(ScreenFolder.Unread([empty], Now, _ => false));
    }

    /// <summary>
    /// The store keeps three hundred readings. A reading of the three hundred
    /// and first newest file would be dropped the moment it was made and the
    /// file would be back in the list next tick, for ever - so files older
    /// than the newest three hundred are not offered at all.
    /// </summary>
    [Fact]
    public void Only_as_many_of_the_newest_files_as_the_store_keeps_are_considered()
    {
        var files = Enumerable.Range(0, 6).Select(i => Shot($"ScreenShot-{i}.jpg", TimeSpan.FromMinutes(i + 1))).ToList();

        var unread = ScreenFolder.Unread(files, Now, path => path.EndsWith("0.jpg") || path.EndsWith("1.jpg"), keep: 4);

        Assert.Equal([files[2], files[3]], unread);
    }

    [Theory]
    [InlineData("ScreenShot-2026-09-07_21-30-17-B52.jpg", true)]
    [InlineData("shot.PNG", true)]
    [InlineData("shot.jpeg", true)]
    [InlineData("Thumbs.db", false)]
    [InlineData("notes.txt", false)]
    public void Only_image_files_count(string name, bool counts)
    {
        Assert.Equal(counts, ScreenFolder.IsScreenshot(name));
    }
}
