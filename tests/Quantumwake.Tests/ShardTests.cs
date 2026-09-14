using Quantumwake.Core.Events;
using Quantumwake.Core.Logging;
using Quantumwake.Core.Parsing;
using Quantumwake.Core.State;

namespace Quantumwake.Tests;

/// <summary>
/// Which server a session was on, and how it left.
/// </summary>
/// <remarks>
/// Fixtures are real lines from this install's backups. The join is the only
/// line that names a shard; the leaves are the disconnect and quit lines that
/// already existed and were only ever counted.
/// </remarks>
public class ShardTests
{
    private static readonly DateTimeOffset T0 = new(2026, 4, 27, 1, 51, 16, TimeSpan.Zero);

    private static T ParseOne<T>(string raw) where T : GameEvent
    {
        Assert.True(LogEnvelope.TryParse(raw, out var line));
        var parser = new LogEventParser();
        var ev = parser.Parse(line);
        Assert.Equal(0, parser.UnmatchedKnownTags);
        return Assert.IsType<T>(ev);
    }

    [Fact]
    public void Reads_the_shard_from_the_join_line()
    {
        var ev = ParseOne<ShardJoinEvent>(
            "<2026-04-27T01:51:16.504Z> [Notice] <Join PU> address[34.86.98.241] port[64353] " +
            "shard[pub_use1b_11704877_010] locationId[562954248454145] [Team_GameServices][GIM][Matchmaking]");

        Assert.Equal("pub_use1b_11704877_010", ev.Shard);
        Assert.Equal("34.86.98.241", ev.Address);
        Assert.Equal(64353, ev.Port);
        Assert.Equal("562954248454145", ev.LocationId);
    }

    [Theory]
    [InlineData("<2026-09-01T02:00:00.000Z> [Notice] <SystemQuit> CSystem::Quit invoked with - cause=30016, " +
                "reason=User closed the app, exitCode=0, thread id=10360, main thread id=10360 [Team_Unknown][System]",
        "30016", "User closed the app", false)]
    [InlineData("<2026-09-01T02:00:00.000Z> [Error] <SystemQuit> CSystem::Quit invoked with - cause=30024, " +
                "reason=Back-end services are unresponsive, exitCode=0, thread id=33740, main thread id=33740 [Team_Unknown][System]",
        "30024", "Back-end services are unresponsive", true)]
    public void Reads_why_the_client_quit(string raw, string cause, string reason, bool backend)
    {
        var ev = ParseOne<SystemQuitEvent>(raw);

        Assert.Equal(cause, ev.Cause);
        Assert.Equal(reason, ev.Reason);
        Assert.Equal(backend, ev.IsBackendFailure);
    }

    [Theory]
    [InlineData("pub_use1b_11704877_010", "pub", "use1b", "11704877", "010", "US East", "US East 10")]
    [InlineData("pub_euw1b_12545750_150", "pub", "euw1b", "12545750", "150", "EU West", "EU West 150")]
    [InlineData("pub_ape1a_12326004_004", "pub", "ape1a", "12326004", "004", "Asia-Pacific East", "Asia-Pacific East 4")]
    [InlineData("pub_apse2a_12326004_001", "pub", "apse2a", "12326004", "001", "Asia-Pacific Southeast", "Asia-Pacific Southeast 1")]
    public void Takes_a_shard_name_apart(
        string full, string env, string region, string deployment, string number, string label, string spoken)
    {
        Assert.True(ShardName.TryParse(full, out var name));

        Assert.Equal(env, name.Environment);
        Assert.Equal(region, name.RegionCode);
        Assert.Equal(deployment, name.Deployment);
        Assert.Equal(number, name.Number);
        Assert.Equal(label, name.Region);
        Assert.Equal(spoken, name.Short);
    }

    /// <summary>A region this install has never seen is shown as its code, not guessed at.</summary>
    [Fact]
    public void An_unknown_region_keeps_its_code()
    {
        Assert.True(ShardName.TryParse("pub_sam1a_12545750_003", out var name));
        Assert.Equal("SAM1A", name.Region);
    }

    [Theory]
    [InlineData("local_shard")]
    [InlineData("")]
    [InlineData(null)]
    public void A_name_in_another_shape_is_not_a_shard(string? full)
    {
        Assert.False(ShardName.TryParse(full, out _));
        Assert.Null(ShardName.DeploymentOf(full));
    }

    /// <summary>
    /// The ordinary evening: join, play, ask to leave, quit from the menu. The
    /// quit arrives after the stay has already ended and must not open a second
    /// one or change how the first ended.
    /// </summary>
    [Fact]
    public void A_stay_ends_when_the_pilot_leaves()
    {
        var builder = new SessionBuilder("test.log");

        builder.Add(new ShardJoinEvent(T0, "pub_use1b_11704877_010", "34.86.98.241", 64353, "1"));
        builder.Add(new DisconnectEvent(T0.AddMinutes(40), "30016", "Remote Disconnect - Player requested disconnect", true));
        builder.Add(new SystemQuitEvent(T0.AddMinutes(41), "30016", "Quit via console command"));

        var summary = builder.Build();

        var stay = Assert.Single(summary.Shards);
        Assert.Equal("pub_use1b_11704877_010", stay.Shard);
        Assert.Equal(ShardLeave.Left, stay.Ending);
        Assert.Equal(TimeSpan.FromMinutes(40), stay.Duration);
        Assert.Null(summary.CurrentShard);
        Assert.Contains(summary.Timeline, t => t.Kind == "shard" && t.Text == "Joined pub_use1b_11704877_010" && t.Detail == "US East");
    }

    [Fact]
    public void The_frontend_teardown_does_not_end_a_stay()
    {
        var builder = new SessionBuilder("test.log");

        builder.Add(new ShardJoinEvent(T0, "pub_use1b_11704877_010", "34.86.98.241", 64353, "1"));

        // Written 20 ms after every join, for the menu's channel.
        builder.Add(new DisconnectEvent(T0.AddMilliseconds(20), "30010", "Nub destroyed", false));

        var summary = builder.Build();

        Assert.Equal("pub_use1b_11704877_010", summary.CurrentShard);
        Assert.Equal(ShardLeave.LogEnded, Assert.Single(summary.Shards).Ending);
    }

    [Theory]
    [InlineData("Remote Disconnect - player inactive", ShardLeave.Idle)]
    [InlineData("DisconnectCmd: disconnect light ExitToMenu", ShardLeave.Left)]
    public void Tells_a_kick_from_a_choice(string reason, ShardLeave ending)
    {
        var builder = new SessionBuilder("test.log");

        builder.Add(new ShardJoinEvent(T0, "pub_use1b_11704877_010", "34.86.98.241", 64353, "1"));
        builder.Add(new DisconnectEvent(T0.AddMinutes(5), "30028", reason, true));

        Assert.Equal(ending, Assert.Single(builder.Build().Shards).Ending);
    }

    [Theory]
    [InlineData("User closed the app", ShardLeave.Quit)]
    [InlineData("Back-end services are unresponsive", ShardLeave.Backend)]
    public void Quitting_while_on_the_shard_is_its_own_ending(string reason, ShardLeave ending)
    {
        var builder = new SessionBuilder("test.log");

        builder.Add(new ShardJoinEvent(T0, "pub_use1b_11704877_010", "34.86.98.241", 64353, "1"));
        builder.Add(new SystemQuitEvent(T0.AddMinutes(5), "30016", reason));

        Assert.Equal(ending, Assert.Single(builder.Build().Shards).Ending);
    }

    /// <summary>
    /// A second placement with no leave between. The first stay is kept, ends
    /// at the second join, and says the game moved the client.
    /// </summary>
    [Fact]
    public void A_join_while_still_on_a_shard_closes_the_first_stay()
    {
        var builder = new SessionBuilder("test.log");

        builder.Add(new ShardJoinEvent(T0, "pub_use1b_11704877_010", "34.86.98.241", 64353, "1"));
        builder.Add(new ShardJoinEvent(T0.AddMinutes(10), "pub_use1b_11704877_190", "35.245.203.83", 64293, "1"));

        var summary = builder.Build();

        Assert.Equal(2, summary.Shards.Count);
        Assert.Equal(ShardLeave.Replaced, summary.Shards[0].Ending);
        Assert.Equal(TimeSpan.FromMinutes(10), summary.Shards[0].Duration);
        Assert.Equal("pub_use1b_11704877_190", summary.CurrentShard);
    }

    /// <summary>
    /// Building is what the live feed does on every event, so the stay still
    /// open must be reported without being closed.
    /// </summary>
    [Fact]
    public void Building_does_not_end_the_open_stay()
    {
        var builder = new SessionBuilder("test.log");

        builder.Add(new ShardJoinEvent(T0, "pub_use1b_11704877_010", "34.86.98.241", 64353, "1"));
        builder.Add(new LoginEvent(T0.AddMinutes(3), "nekron"));

        var first = builder.Build();
        Assert.Equal(TimeSpan.FromMinutes(3), Assert.Single(first.Shards).Duration);

        builder.Add(new DisconnectEvent(T0.AddMinutes(30), "30016", "Remote Disconnect - Player requested disconnect", true));

        var second = builder.Build();
        var stay = Assert.Single(second.Shards);
        Assert.Equal(ShardLeave.Left, stay.Ending);
        Assert.Equal(TimeSpan.FromMinutes(30), stay.Duration);
    }

    /// <summary>
    /// The menu's own channel going down is not leaving the shard. No real log
    /// has yet shown a non-routine frontend disconnect during a stay, which is
    /// exactly why the rule is tested rather than trusted.
    /// </summary>
    [Fact]
    public void A_frontend_disconnect_does_not_end_a_stay()
    {
        var builder = new SessionBuilder("test.log");

        builder.Add(new ShardJoinEvent(T0, "pub_use1b_11704877_010", "34.86.98.241", 64353, "1"));
        builder.Add(new DisconnectEvent(T0.AddMinutes(5), "30016", "Remote Disconnect - Player requested disconnect", true, "SC_Frontend"));

        var summary = builder.Build();

        Assert.Equal("pub_use1b_11704877_010", summary.CurrentShard);
        Assert.Equal(1, summary.Disconnects);

        builder.Add(new DisconnectEvent(T0.AddMinutes(9), "30016", "Remote Disconnect - Player requested disconnect", true, "SC_Default"));
        Assert.Equal(ShardLeave.Left, Assert.Single(builder.Build().Shards).Ending);
    }

    [Fact]
    public void The_parser_carries_the_gamerules_on_a_disconnect()
    {
        var ev = ParseOne<DisconnectEvent>(
            "<2026-04-27T01:56:52.917Z> [Notice] <Channel Disconnected> cause=30016 reason=\"Remote Disconnect - Player requested disconnect\" " +
            "frame=19594 isRemote=1 map=\"megamap\" gamerules=\"SC_Default\" hostType=\"Replicant\" remoteAddr=35.245.203.83:64293 localAddr=<local>:16 [Team_Network][Network]");

        Assert.Equal("SC_Default", ev.GameRules);
        Assert.False(ev.IsFrontend);
    }
}
