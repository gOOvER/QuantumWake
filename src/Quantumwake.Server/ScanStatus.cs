namespace Quantumwake.Server;

/// <summary>
/// Progress of the log scan, so the UI can say what is happening.
/// </summary>
/// <remarks>
/// A cold backfill reads 400 MB across 145 files and takes about half a minute.
/// Without this the dashboard just sits empty while it runs, with no way to tell
/// a slow first start from a broken one.
/// </remarks>
public sealed class ScanStatus
{
    private readonly Lock _gate = new();

    private int _done;
    private int _total;
    private int _parsed;
    private string? _file;
    private bool _running;
    private DateTimeOffset? _startedAt;
    private DateTimeOffset? _finishedAt;

    /// <summary>
    /// Whether any scan has begun since the server started.
    /// </summary>
    /// <remarks>
    /// The startup scan waits behind the game-file read, which takes about half
    /// a minute after a patch. For that half minute the scan is not running and
    /// has not run, every page built on the logs is empty, and an idle status
    /// alone cannot tell that from a scan already finished.
    /// </remarks>
    public bool HasStarted
    {
        get
        {
            lock (_gate)
                return _startedAt is not null;
        }
    }

    /// <summary>A snapshot safe to serialise.</summary>
    public object Snapshot()
    {
        lock (_gate)
        {
            var elapsed = _startedAt is null
                ? TimeSpan.Zero
                : (_finishedAt ?? DateTimeOffset.UtcNow) - _startedAt.Value;

            return new
            {
                running = _running,
                done = _done,
                total = _total,

                // Files newly parsed rather than served from cache - on a warm
                // start this stays near zero even as `done` climbs.
                parsed = _parsed,
                file = _file,
                percent = _total == 0 ? 0 : (int)Math.Round(_done * 100.0 / _total),
                elapsedSeconds = (int)elapsed.TotalSeconds
            };
        }
    }

    public void Begin()
    {
        lock (_gate)
        {
            _running = true;
            _done = 0;
            _total = 0;
            _parsed = 0;
            _file = null;
            _startedAt = DateTimeOffset.UtcNow;
            _finishedAt = null;
        }
    }

    public void Report(int done, int total, string file, bool cached)
    {
        lock (_gate)
        {
            _done = done;
            _total = total;
            _file = file;

            if (!cached)
                _parsed++;
        }
    }

    public void Finish()
    {
        lock (_gate)
        {
            _running = false;
            _finishedAt = DateTimeOffset.UtcNow;
            _file = null;
        }
    }
}
