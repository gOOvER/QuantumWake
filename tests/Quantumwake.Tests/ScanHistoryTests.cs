using Quantumwake.Core.Logging;
using Quantumwake.Data;
using Quantumwake.LogSim;

namespace Quantumwake.Tests;

/// <summary>
/// The record the Log page draws of what the scans read: each file's state
/// against the folder, and the runs themselves.
/// </summary>
/// <remarks>
/// Driven through a real folder and a real scan rather than rows planted in
/// the store, because the states are a comparison between the two and a test
/// that fakes one side proves nothing about the other. The logs are the
/// simulator's, so the parser is the one the app ships.
/// </remarks>
public sealed class ScanHistoryTests : IDisposable
{
    private static readonly DateTimeOffset Start = new(2026, 8, 24, 20, 0, 0, TimeSpan.Zero);

    private readonly string _root = Path.Combine(Path.GetTempPath(), $"qw-scanhistory-{Guid.NewGuid():N}");
    private readonly SessionStore _store = new(":memory:");
    private readonly LogLibrary _library;
    private readonly GameInstall _install;

    public ScanHistoryTests()
    {
        Directory.CreateDirectory(Path.Combine(_root, "logbackups"));
        _install = new GameInstall("LIVE", _root);
        _library = new LogLibrary(_store);

        Write(Path.Combine(_root, "logbackups", "Game-old.log"), "cargo-run", Start.AddDays(-1));
        Write(_install.GameLogPath, "contract-complete", Start);
    }

    public void Dispose()
    {
        _store.Dispose();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }

    private static void Write(string path, string scenario, DateTimeOffset at)
    {
        using var writer = new LogWriter(path);
        ScenarioRunner.Run(writer, Assert.IsType<ScenarioDefinition>(ScenarioCatalogue.Find(scenario)), at);
    }

    [Fact]
    public void Nothing_scanned_lists_every_file_as_unread_and_no_runs()
    {
        var report = ScanHistory.Build(_install, _store);

        Assert.Equal(2, report.Files.Count);
        Assert.All(report.Files, f => Assert.Equal(ScanHistory.Unread, f.State));
        Assert.Empty(report.Runs);
        Assert.Equal(2, report.OnDisk);
        Assert.True(report.Bytes > 0);
    }

    [Fact]
    public void A_scan_marks_each_file_current_and_records_itself()
    {
        Assert.Equal(2, _library.Scan(_install));

        var report = ScanHistory.Build(_install, _store);

        Assert.All(report.Files, f => Assert.Equal(ScanHistory.Current, f.State));
        Assert.All(report.Files, f => Assert.NotNull(f.ScannedAt));
        Assert.All(report.Files, f => Assert.NotNull(f.StartedAt));

        var live = Assert.Single(report.Files, f => f.Live);
        Assert.Equal("Game.log", live.Name);

        var run = Assert.Single(report.Runs);
        Assert.Equal(2, run.Files);
        Assert.Equal(2, run.Parsed);
        Assert.False(run.Forced);
        Assert.True(run.FinishedAt >= run.StartedAt);
    }

    /// <summary>
    /// The live log grows while the game runs, so between scans it is the one
    /// file the page should show as read-but-stale rather than done.
    /// </summary>
    [Fact]
    public void A_log_that_grew_since_it_was_read_is_changed_until_the_next_scan()
    {
        _library.Scan(_install);

        // A later write time is the fingerprint's other half; the length alone
        // would do, but a file system that rounds timestamps must not hide it.
        File.AppendAllText(_install.GameLogPath, "<2026-08-24T21:00:00.000Z> [Notice] <Ping> nothing\n");
        File.SetLastWriteTimeUtc(_install.GameLogPath, DateTime.UtcNow.AddMinutes(1));

        var before = ScanHistory.Build(_install, _store);
        Assert.Equal(ScanHistory.Changed, Assert.Single(before.Files, f => f.Live).State);
        Assert.Equal(ScanHistory.Current, Assert.Single(before.Files, f => !f.Live).State);

        Assert.Equal(1, _library.Scan(_install));

        var after = ScanHistory.Build(_install, _store);
        Assert.All(after.Files, f => Assert.Equal(ScanHistory.Current, f.State));

        // Newest first, and the second pass read only the one file that moved.
        Assert.Equal(2, after.Runs.Count);
        Assert.Equal(1, after.Runs[0].Parsed);
        Assert.Equal(2, after.Runs[1].Parsed);
    }

    /// <summary>
    /// The game prunes its backups; the app does not. A session read from a
    /// backup that has since gone is still a session, and the page says where
    /// it came from rather than dropping it from the list.
    /// </summary>
    [Fact]
    public void A_backup_the_game_deleted_is_listed_as_gone_with_its_session()
    {
        _library.Scan(_install);
        File.Delete(Path.Combine(_root, "logbackups", "Game-old.log"));

        var report = ScanHistory.Build(_install, _store);

        var gone = Assert.Single(report.Files, f => f.State == ScanHistory.Gone);
        Assert.Equal("Game-old.log", gone.Name);
        Assert.Null(gone.Bytes);
        Assert.NotNull(gone.StartedAt);
        Assert.Equal(1, report.OnDisk);
    }

    [Fact]
    public void A_forced_rescan_is_recorded_as_one()
    {
        _library.Scan(_install);
        _library.Scan(_install, force: true);

        var runs = _store.Scans();

        Assert.True(runs[0].Forced);
        Assert.Equal(2, runs[0].Parsed);
        Assert.False(runs[1].Forced);
    }

    [Fact]
    public void No_install_lists_nothing_but_still_answers()
    {
        var report = ScanHistory.Build(null, _store);

        Assert.Null(report.Root);
        Assert.Empty(report.Files);
        Assert.Equal(0, report.OnDisk);
    }

    /// <summary>
    /// The column arrived after the table did. A database from an earlier
    /// build opens without it, and must gain it rather than be dropped - the
    /// drop is what SchemaVersion does, and a cold backfill is not the price
    /// of a timestamp.
    /// </summary>
    [Fact]
    public void An_older_database_gains_the_scanned_at_column_without_losing_its_sessions()
    {
        var path = Path.Combine(_root, "old.db");

        using (var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={path}"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                """
                PRAGMA user_version = 3;
                CREATE TABLE sessions (
                    id TEXT PRIMARY KEY, source_file TEXT NOT NULL, fingerprint TEXT NOT NULL,
                    started_at TEXT NOT NULL, ended_at TEXT NOT NULL, handle TEXT, payload TEXT NOT NULL);
                INSERT INTO sessions VALUES ('s1', 'C:\old\Game.log', 'v15:1:1',
                    '2026-08-01T00:00:00.0000000Z', '2026-08-01T01:00:00.0000000Z', 'nekron',
                    '{"id":"s1","sourceFile":"C:\\old\\Game.log","startedAt":"2026-08-01T00:00:00Z","endedAt":"2026-08-01T01:00:00Z"}');
                """;
            command.ExecuteNonQuery();
        }

        using var store = new SessionStore(path);

        Assert.Equal(1, store.Count());
        var row = Assert.Single(store.Ingested());
        Assert.Null(row.ScannedAt);
        Assert.Equal("nekron", row.Handle);
    }
}
