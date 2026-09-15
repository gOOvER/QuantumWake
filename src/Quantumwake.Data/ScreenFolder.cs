namespace Quantumwake.Data;

/// <summary>One file in the screenshots folder, as much of it as the watch needs.</summary>
public sealed record ScreenFile(string Path, long Length, DateTimeOffset LastWrite);

/// <summary>
/// Decides which screenshots in the folder are new and finished being written.
/// </summary>
/// <remarks>
/// <para>
/// Pure, so it can be tested with a folder written by hand. The service that
/// lists the real folder every couple of seconds is thirty lines in the
/// server and holds no rules of its own.
/// </para>
/// <para>
/// Two rules, both from the folder's behaviour. The game writes a JPEG over
/// some tens of milliseconds, and a watcher that reads on creation reads half
/// a file - so a file counts only once its last write is comfortably in the
/// past. And nothing that was already there when the watch was switched on is
/// read: the pilot enabled reading their screenshots, not their archive, and
/// the button for the newest one is still there for that. The archive is read
/// only when asked for by name - <see cref="Unread"/> - because a loadout
/// photographed the evening before the app was first run is still the only
/// photograph of that ship.
/// </para>
/// </remarks>
public static class ScreenFolder
{
    /// <summary>How long a file must have gone unwritten before it is read.</summary>
    /// <remarks>
    /// Generous against the write itself, which is quick, and against a file
    /// time that is rounded: NTFS keeps a hundred nanoseconds but a copy from
    /// elsewhere may carry whole seconds.
    /// </remarks>
    public static readonly TimeSpan Settled = TimeSpan.FromSeconds(2);

    /// <summary>The files worth reading now, oldest first.</summary>
    /// <param name="baseline">When the watch began; nothing written before it counts.</param>
    /// <param name="seen">Files already read, by full path.</param>
    public static IReadOnlyList<ScreenFile> Ready(
        IEnumerable<ScreenFile> listing,
        DateTimeOffset baseline,
        DateTimeOffset now,
        Func<string, bool> seen)
    {
        return [.. listing
            .Where(file => file.Length > 0)
            .Where(file => file.LastWrite >= baseline)
            .Where(file => now - file.LastWrite >= Settled)
            .Where(file => !seen(file.Path))
            .OrderBy(file => file.LastWrite)];
    }

    /// <summary>
    /// The archive: every settled screenshot the app has never read, newest
    /// first. What the pilot gets when they ask for the older ones by name.
    /// </summary>
    /// <remarks>
    /// Newest first rather than oldest, unlike <see cref="Ready"/>: a pilot
    /// asking for the archive wants last night's loadout before last month's,
    /// and a bounded read has to start at the useful end.
    /// </remarks>
    public static IReadOnlyList<ScreenFile> Unread(
        IEnumerable<ScreenFile> listing,
        DateTimeOffset now,
        Func<string, bool> seen)
    {
        return [.. listing
            .Where(file => file.Length > 0)
            .Where(file => now - file.LastWrite >= Settled)
            .Where(file => !seen(file.Path))
            .OrderByDescending(file => file.LastWrite)];
    }

    /// <summary>Whether a file is one the game writes as a screenshot.</summary>
    public static bool IsScreenshot(string path)
    {
        var extension = System.IO.Path.GetExtension(path);
        return extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".png", StringComparison.OrdinalIgnoreCase);
    }
}
