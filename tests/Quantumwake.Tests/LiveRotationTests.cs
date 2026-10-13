using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Quantumwake.Core.Logging;
using Quantumwake.Data;
using Quantumwake.LogSim;
using Quantumwake.Server;

namespace Quantumwake.Tests;

/// <summary>
/// What becomes of a session when the game is relaunched while the app runs.
/// </summary>
/// <remarks>
/// <para>
/// Every session read from <c>Game.log</c> is stored under the same id, so the
/// store only keeps a session for good once it is read again from the backup
/// the game rotates it into. On 2026-10-12 the app started a minute before the
/// game, the game was launched twice, and the second rotation overwrote the
/// first session's only copy: a whole evening of Discovery Month gone from the
/// event totals until the app was next restarted.
/// </para>
/// <para>
/// Driven through a real folder and the simulator's logs, the same way
/// <see cref="ScanHistoryTests"/> is, because the bug lives between the
/// folder and the store.
/// </para>
/// </remarks>
public sealed class LiveRotationTests : IDisposable
{
    private static readonly DateTimeOffset Evening = new(2026, 10, 12, 1, 35, 0, TimeSpan.Zero);

    private readonly string _root = Path.Combine(Path.GetTempPath(), $"qw-rotation-{Guid.NewGuid():N}");
    private readonly SessionStore _store = new(":memory:");
    private readonly LogLibrary _library;
    private readonly GameInstall _install;
    private readonly LiveSessionService _live;

    public LiveRotationTests()
    {
        Directory.CreateDirectory(Path.Combine(_root, "logbackups"));
        _install = new GameInstall("LIVE", _root);
        _library = new LogLibrary(_store);
        _live = new LiveSessionService(new SilentHub(), _library, NullLogger<LiveSessionService>.Instance, _install);

        // The app starts while Game.log still holds last night's session.
        Write(_install.GameLogPath, "contract-complete", Evening);
        _library.Scan(_install);
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

    /// <summary>The game moves the old log into the backups and starts a new one.</summary>
    private string Relaunch(DateTimeOffset at, bool backupWritten = true)
    {
        var backup = Path.Combine(_root, "logbackups", $"Game Build(1) {at:dd MMM yy (HH mm ss)}.log");
        if (backupWritten) File.Move(_install.GameLogPath, backup);
        else File.Delete(_install.GameLogPath);

        Write(_install.GameLogPath, "cargo-run", at.AddHours(24));
        return backup;
    }

    [Fact]
    public void A_rotation_reads_the_backup_the_session_went_into()
    {
        var backup = Relaunch(Evening);

        _live.OnRotated();

        var saved = Assert.Single(_store.All(), s => s.SourceFile == backup);
        Assert.Equal(Evening.Date, saved.StartedAt.UtcDateTime.Date);
    }

    /// <summary>The incident itself: two relaunches in one sitting.</summary>
    [Fact]
    public void A_second_rotation_does_not_cost_the_first_session()
    {
        var first = Relaunch(Evening);
        _live.OnRotated();

        var second = Relaunch(Evening.AddHours(24));
        _live.OnRotated();

        var files = _store.All().Select(s => s.SourceFile).ToList();
        Assert.Contains(first, files);
        Assert.Contains(second, files);
    }

    /// <summary>
    /// Nothing to read yet - the game has not finished moving the old log. The
    /// stored copy is then the only one, and re-reading the new Game.log over
    /// it would lose the session the other way round.
    /// </summary>
    [Fact]
    public void A_rotation_with_no_backup_yet_keeps_the_stored_session()
    {
        var before = Assert.Single(_store.All());

        Relaunch(Evening, backupWritten: false);
        _live.OnRotated();

        var after = Assert.Single(_store.All());
        Assert.Equal(before.StartedAt, after.StartedAt);
    }
}

/// <summary>The hub the service needs to be constructed; nothing here broadcasts.</summary>
file sealed class SilentHub : IHubContext<LiveHub>
{
    public IHubClients Clients => throw new NotSupportedException("these tests never broadcast");
    public IGroupManager Groups => throw new NotSupportedException("these tests never broadcast");
}
