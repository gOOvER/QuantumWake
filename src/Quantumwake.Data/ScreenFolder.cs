namespace Quantumwake.Data;

/// <summary>One file in the screenshots folder, as much of it as the watch needs.</summary>
public sealed record ScreenFile(string Path, long Length, DateTimeOffset LastWrite);

/// <summary>
/// Decides which screenshots in the folder are waiting to be read.
/// </summary>
/// <remarks>
/// <para>
/// Pure, so it can be tested with a folder written by hand. The service that
/// lists the real folder every couple of seconds is thirty lines in the
/// server and holds no rules of its own.
/// </para>
/// <para>
/// Everything in the folder the app has not read is read, newest first. The
/// watch used to begin from the moment it was switched on and leave the
/// archive alone; that left the only photographs of the Hermes' loadout -
/// taken the evening the watch shipped - unread for a week while the Garage
/// said nothing. Reading is the pilot's choice once, at the switch, and the
/// way out is the same switch, or invalidating a reading they do not want
/// believed; it is not a second choice per screenshot.
/// </para>
/// <para>
/// Two limits, both from the folder's behaviour. The game writes a JPEG over
/// some tens of milliseconds, and a watcher that reads on creation reads
/// half a file - so a file counts only once its last write is comfortably in
/// the past. And only as many of the newest files as the store keeps are
/// considered, because a reading of anything older would be dropped as soon
/// as it was made, and the file would be back in this list the next tick.
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

    /// <summary>The settled files the app has never read, newest first.</summary>
    /// <param name="seen">Files already read, by full path.</param>
    /// <param name="keep">How many of the newest files are worth considering - the store's bound.</param>
    public static IReadOnlyList<ScreenFile> Unread(
        IEnumerable<ScreenFile> listing,
        DateTimeOffset now,
        Func<string, bool> seen,
        int keep = ScreenReadingStore.Keep)
    {
        return [.. listing
            .Where(file => file.Length > 0)
            .Where(file => now - file.LastWrite >= Settled)
            .OrderByDescending(file => file.LastWrite)
            .Take(keep)
            .Where(file => !seen(file.Path))];
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
