using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Quantumwake.Core.Events;
using Quantumwake.Core.State;
using Quantumwake.Data;
using Quantumwake.Server;

namespace Quantumwake.Tests;

/// <summary>
/// The Now snapshot's account of the shard the client is on.
/// </summary>
public class LiveShardTests : IDisposable
{
    private readonly SessionStore _store = new(":memory:");
    private static readonly DateTimeOffset At = new(2026, 9, 14, 14, 0, 0, TimeSpan.Zero);

    public void Dispose() => _store.Dispose();

    private LiveSessionService Live() =>
        new(new MuteHub(), new LogLibrary(_store), NullLogger<LiveSessionService>.Instance);

    /// <summary>
    /// A, leave, A again within one session is a second visit. A count cached
    /// by shard alone answered the second placement with the first count, and
    /// left out the stay that had just ended.
    /// </summary>
    [Fact]
    public void Rejoining_the_same_shard_counts_the_stay_that_just_ended()
    {
        // One earlier visit in stored history, from another file.
        var earlier = new SessionBuilder("earlier.log");
        earlier.Add(new ShardJoinEvent(At.AddDays(-1), "pub_use1b_12545750_150", "1.2.3.4", 1, "x"));
        earlier.Add(new DisconnectEvent(At.AddDays(-1).AddHours(1), "30016", "Remote Disconnect - Player requested disconnect", true, "SC_Default"));
        _store.Save(earlier.Build(), "f");

        var live = Live();

        live.OnEvent(new ShardJoinEvent(At, "pub_use1b_12545750_150", "1.2.3.4", 1, "x"));
        Assert.Equal(1, live.Current.ShardVisitsBefore);

        live.OnEvent(new DisconnectEvent(At.AddMinutes(20), "30016", "Remote Disconnect - Player requested disconnect", true, "SC_Default"));
        Assert.Null(live.Current.Shard);

        live.OnEvent(new ShardJoinEvent(At.AddMinutes(25), "pub_use1b_12545750_150", "1.2.3.4", 1, "x"));
        Assert.Equal("pub_use1b_12545750_150", live.Current.Shard);
        Assert.Equal(2, live.Current.ShardVisitsBefore);
    }

    /// <summary>The live summary the Servers list merges in carries the open stay.</summary>
    [Fact]
    public void The_live_summary_names_the_open_stay()
    {
        var live = Live();
        live.OnEvent(new ShardJoinEvent(At, "pub_use1b_12545750_199", "1.2.3.4", 1, "x"));

        var summary = live.LiveSummary;

        Assert.Equal("pub_use1b_12545750_199", summary.CurrentShard);
        Assert.Equal(ShardLeave.LogEnded, Assert.Single(summary.Shards).Ending);

        // And the list built from it says "still on it", not "log ended".
        var rows = new LogLibrary(_store).Shards(summary);
        Assert.Equal(ShardLeave.Open, Assert.Single(rows).LastEnding);
    }
}

/// <summary>Nothing here broadcasts; the service only needs a hub to be constructed.</summary>
file sealed class MuteHub : IHubContext<LiveHub>
{
    public IHubClients Clients => throw new NotSupportedException("these tests never broadcast");
    public IGroupManager Groups => throw new NotSupportedException("these tests never broadcast");
}
