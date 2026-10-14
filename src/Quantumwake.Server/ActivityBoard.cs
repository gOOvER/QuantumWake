namespace Quantumwake.Server;

/// <summary>
/// The downloads in flight, so every page can say one is under way.
/// </summary>
/// <remarks>
/// <para>
/// The log scan and the game-file read each have a status of their own; this is
/// for the rest - the community dataset, UEX prices and the UEX feeds - which
/// until now were only visible as a "downloading…" beside their switch in
/// Settings. Started from Settings and then left, a 50 MB community digest ran
/// for a minute while Market sat empty on another page with nothing saying why.
/// </para>
/// <para>
/// Counted rather than flagged per key: a manual UEX fetch can overlap the
/// automatic refresh, and the first of the two to finish must not announce the
/// download as over while the other is still running.
/// </para>
/// </remarks>
public sealed class ActivityBoard
{
    private readonly Lock _gate = new();
    private readonly List<Job> _jobs = [];

    private sealed record Job(string Key, string? Detail, DateTimeOffset StartedAt);

    /// <summary>Marks a job as running until the returned handle is disposed.</summary>
    /// <param name="key">What kind of job: <c>community</c>, <c>uex</c> or <c>uex-feed</c>. The page words it.</param>
    /// <param name="detail">Which one, where the key alone does not say - a feed's title.</param>
    public IDisposable Track(string key, string? detail = null)
    {
        var job = new Job(key, detail, DateTimeOffset.UtcNow);

        lock (_gate)
            _jobs.Add(job);

        return new Release(this, job);
    }

    /// <summary>A snapshot safe to serialise, oldest job first.</summary>
    public object[] Snapshot()
    {
        lock (_gate)
        {
            var now = DateTimeOffset.UtcNow;
            return [.. _jobs.Select(j => (object)new
            {
                key = j.Key,
                detail = j.Detail,
                startedAt = j.StartedAt,
                seconds = (int)(now - j.StartedAt).TotalSeconds
            })];
        }
    }

    private void Remove(Job job)
    {
        lock (_gate)
            _jobs.Remove(job);
    }

    private sealed class Release(ActivityBoard board, Job job) : IDisposable
    {
        private int _done;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _done, 1) == 0)
                board.Remove(job);
        }
    }
}
