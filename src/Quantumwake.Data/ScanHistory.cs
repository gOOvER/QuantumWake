using Quantumwake.Core.Logging;

namespace Quantumwake.Data;

/// <summary>
/// The scan's own record: every log file the install has, what the store
/// last read of each, and the runs that read them.
/// </summary>
/// <remarks>
/// Built from two lists that disagree in useful ways. The folder says what is
/// there now; the store says what was summarised and at which fingerprint. A
/// file in both at the same fingerprint is done. One in both at different
/// fingerprints - always the live Game.log while the game runs - is stale
/// until the next scan. One only in the folder has never been read, and one
/// only in the store was read from a backup the game has since deleted, which
/// is why the app still knows about sessions the folder cannot show.
/// </remarks>
public static class ScanHistory
{
    /// <summary>The file on disk is the one that was summarised.</summary>
    public const string Current = "current";

    /// <summary>The file has grown or been replaced since it was summarised.</summary>
    public const string Changed = "changed";

    /// <summary>In the folder, never scanned.</summary>
    public const string Unread = "unread";

    /// <summary>Summarised, but no longer in the folder.</summary>
    public const string Gone = "gone";

    public static ScanHistoryReport Build(GameInstall? install, SessionStore store, int runs = 30)
    {
        // One row per file: a rotated log holds exactly one session, but a
        // forced rescan can leave two rows for the same path if the session id
        // changed, and the newest read is the one the page should describe.
        var ingested = store.Ingested()
            .GroupBy(i => i.SourceFile, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(i => i.ScannedAt ?? DateTimeOffset.MinValue).First(),
                StringComparer.OrdinalIgnoreCase);

        var files = new List<LogFileRow>();

        if (install is not null)
        {
            foreach (var path in LogLibrary.LogFiles(install))
            {
                var info = new FileInfo(path);
                if (!info.Exists)
                    continue;

                ingested.Remove(path, out var row);
                var state = row is null ? Unread
                    : row.Fingerprint == SessionStore.Fingerprint(info) ? Current
                    : Changed;

                files.Add(new LogFileRow(
                    info.Name, path,
                    Live: string.Equals(path, install.GameLogPath, StringComparison.OrdinalIgnoreCase),
                    Bytes: info.Length,
                    WrittenAt: new DateTimeOffset(info.LastWriteTimeUtc, TimeSpan.Zero),
                    State: state,
                    ScannedAt: row?.ScannedAt,
                    SessionId: row?.SessionId,
                    StartedAt: row?.StartedAt,
                    EndedAt: row?.EndedAt,
                    Handle: row?.Handle));
            }
        }

        foreach (var row in ingested.Values)
        {
            files.Add(new LogFileRow(
                Path.GetFileName(row.SourceFile), row.SourceFile,
                Live: false, Bytes: null, WrittenAt: null, State: Gone,
                ScannedAt: row.ScannedAt, SessionId: row.SessionId,
                StartedAt: row.StartedAt, EndedAt: row.EndedAt, Handle: row.Handle));
        }

        // Newest first, by when the game wrote it; a file that is gone has only
        // the session it held to say where it belongs.
        files.Sort((a, b) => (b.WrittenAt ?? b.EndedAt ?? DateTimeOffset.MinValue)
            .CompareTo(a.WrittenAt ?? a.EndedAt ?? DateTimeOffset.MinValue));

        return new ScanHistoryReport(
            install?.RootPath,
            store.Scans(runs),
            files,
            files.Count(f => f.State != Gone),
            files.Sum(f => f.Bytes ?? 0));
    }
}

/// <param name="Root">The install the files belong to, or null when none was found.</param>
/// <param name="OnDisk">Files the folder has now, whatever the store says about them.</param>
/// <param name="Bytes">Their total size, which is what a cold rescan would read.</param>
public sealed record ScanHistoryReport(
    string? Root,
    IReadOnlyList<ScanRun> Runs,
    IReadOnlyList<LogFileRow> Files,
    int OnDisk,
    long Bytes);

/// <param name="Live">The Game.log the game is writing to, as opposed to a rotated backup.</param>
/// <param name="State">One of the <see cref="ScanHistory"/> constants.</param>
/// <param name="ScannedAt">Null when never read, or read before the stamp was kept.</param>
public sealed record LogFileRow(
    string Name,
    string Path,
    bool Live,
    long? Bytes,
    DateTimeOffset? WrittenAt,
    string State,
    DateTimeOffset? ScannedAt,
    string? SessionId,
    DateTimeOffset? StartedAt,
    DateTimeOffset? EndedAt,
    string? Handle);
