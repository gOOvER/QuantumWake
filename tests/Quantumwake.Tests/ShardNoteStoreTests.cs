using Quantumwake.Core.State;
using Quantumwake.Data;

namespace Quantumwake.Tests;

/// <summary>
/// The pilot's notes on servers, and the list they are pinned to.
/// </summary>
public class ShardNoteStoreTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"qw-shards-{Guid.NewGuid():N}");
    private static readonly DateTimeOffset Now = new(2026, 9, 13, 20, 0, 0, TimeSpan.Zero);

    public ShardNoteStoreTests() => Directory.CreateDirectory(_root);

    [Fact]
    public void A_note_and_a_star_survive_a_restart()
    {
        var store = new ShardNoteStore(_root);
        store.SetNote("pub_use1b_12545750_150", "  laggy elevators ");
        store.SetFavorite("pub_use1b_12545750_150", true);

        var back = Assert.Single(new ShardNoteStore(_root).All());
        Assert.Equal("pub_use1b_12545750_150", back.Shard);
        Assert.Equal("laggy elevators", back.Note);
        Assert.True(back.Favorite);
    }

    /// <summary>Clearing the last thing written about a shard removes the record rather than leaving an empty row.</summary>
    [Fact]
    public void An_emptied_record_is_dropped()
    {
        var store = new ShardNoteStore(_root);
        store.SetNote("pub_use1b_12545750_150", "laggy elevators");

        Assert.Null(store.SetNote("pub_use1b_12545750_150", "   "));
        Assert.Empty(store.All());

        // A star alone is enough to keep it.
        store.SetFavorite("pub_use1b_12545750_150", true);
        Assert.Single(store.All());
        Assert.Null(store.SetFavorite("pub_use1b_12545750_150", false));
        Assert.Empty(store.All());
    }

    [Fact]
    public void Editing_the_note_leaves_the_star_alone_and_back()
    {
        var store = new ShardNoteStore(_root);
        store.SetFavorite("pub_use1b_12545750_150", true);
        var edited = store.SetNote("pub_use1b_12545750_150", "good for bunkers");

        Assert.NotNull(edited);
        Assert.True(edited.Favorite);
        Assert.Equal("good for bunkers", edited.Note);
    }

    [Fact]
    public void Notes_are_in_the_backup_and_come_back_from_one()
    {
        var store = new ShardNoteStore(_root);
        store.SetNote("pub_use1b_12545750_150", "laggy elevators");

        var backup = new BackupBuilder(
            new JobStore(_root), new ChecklistStore(_root), new TripStore(_root),
            new MiningLogStore(_root), new MapNoteStore(_root), new GoalStore(_root),
            new WipeStore(_root), new ItemLabelStore(_root), new TombstoneStore(_root),
            new KitStore(_root), new ScreenReadingStore(_root), store);

        var file = backup.Build(new ExportProducer("Quantumwake", "0.12.0"), Now).Backup!;
        Assert.Single(file.Shards!);
        Assert.Equal(1, backup.Preview().Shards);

        // Restored onto a machine that has nothing.
        var other = Path.Combine(_root, "other");
        Directory.CreateDirectory(other);
        using var sessions = new SessionStore(":memory:");
        var target = new ShardNoteStore(other);

        var restore = new RestoreService(
            new JobStore(other), new ChecklistStore(other), new TripStore(other),
            new MiningLogStore(other), new MapNoteStore(other), new GoalStore(other),
            new WipeStore(other), new ItemLabelStore(other), new TombstoneStore(other), new LogLibrary(sessions),
            new KitStore(other), new ScreenReadingStore(other), target);

        restore.Apply(file, restore.Plan(file, "h"), "h", new RestoreChoices());

        var landed = Assert.Single(target.All());
        Assert.Equal("laggy elevators", landed.Note);
    }

    /// <summary>
    /// The list is built from stays, and "current" is judged against the
    /// newest deployment joined - a shard from the one before is history the
    /// moment a join on the new one lands.
    /// </summary>
    [Fact]
    public void The_library_lists_each_shard_once_and_knows_which_are_gone()
    {
        using var sessions = new SessionStore(":memory:");
        var library = new LogLibrary(sessions);

        var a = new SessionBuilder("a.log");
        a.Add(new Core.Events.ShardJoinEvent(Now.AddDays(-30), "pub_use1b_12326004_010", "1.2.3.4", 1, "x"));
        a.Add(new Core.Events.DisconnectEvent(Now.AddDays(-30).AddHours(2), "30016", "Remote Disconnect - Player requested disconnect", true));

        var b = new SessionBuilder("b.log");
        // Arrivals in order: New Babbage before the first join is menu time and
        // must not count; Port Tressler twice inside the stays, New Babbage
        // between, since the builder folds consecutive arrivals at one place.
        b.Add(new Core.Events.LocationInventoryEvent(Now.AddDays(-1).AddMinutes(-5), "nekron", "Stanton4_NewBabbage"));
        b.Add(new Core.Events.ShardJoinEvent(Now.AddDays(-1), "pub_use1b_12545750_150", "1.2.3.4", 1, "x"));
        b.Add(new Core.Events.LocationInventoryEvent(Now.AddDays(-1).AddMinutes(10), "nekron", "RR_MIC_LEO"));
        b.Add(new Core.Events.LocationInventoryEvent(Now.AddDays(-1).AddMinutes(40), "nekron", "Stanton4_NewBabbage"));
        b.Add(new Core.Events.DisconnectEvent(Now.AddDays(-1).AddHours(1), "30028", "Remote Disconnect - player inactive", true));
        b.Add(new Core.Events.ShardJoinEvent(Now.AddDays(-1).AddHours(1).AddMinutes(5), "pub_use1b_12545750_150", "1.2.3.4", 1, "x"));
        b.Add(new Core.Events.LocationInventoryEvent(Now.AddDays(-1).AddHours(2), "nekron", "RR_MIC_LEO"));
        b.Add(new Core.Events.LoginEvent(Now.AddDays(-1).AddHours(3), "nekron"));

        sessions.Save(a.Build(), "fa");
        sessions.Save(b.Build(), "fb");

        var rows = library.Shards();

        Assert.Equal(2, rows.Count);

        var newest = rows[0];
        Assert.Equal("pub_use1b_12545750_150", newest.Shard);
        Assert.True(newest.Current);
        Assert.Equal(2, newest.Visits);
        Assert.Equal(1, newest.Sessions);
        Assert.Equal(TimeSpan.FromHours(3) - TimeSpan.FromMinutes(5), newest.Time);
        Assert.Equal(ShardLeave.LogEnded, newest.LastEnding);
        Assert.Equal(1, newest.Endings["Idle"]);
        Assert.Equal("US East", newest.Region);

        Assert.Equal(2, newest.Places.Count);
        var place = newest.Places[0];
        Assert.Equal("Port Tressler", place.Name);
        Assert.Equal(2, place.Visits);
        Assert.Empty(rows[1].Places);

        var older = rows[1];
        Assert.False(older.Current);
        Assert.Equal("12326004", older.Deployment);

        // Visits before a session exclude that session's own stays.
        Assert.Equal(0, library.ShardVisitsBefore("pub_use1b_12545750_150", "b"));
        Assert.Equal(2, library.ShardVisitsBefore("pub_use1b_12545750_150", "a"));
    }


    /// <summary>
    /// A shard joined since the last scan is in no stored session. Given the
    /// live summary, the list carries it - with the stay marked as still open
    /// rather than as a log that ended - and the live copy replaces the stale
    /// stored copy of the same file rather than counting beside it.
    /// </summary>
    [Fact]
    public void The_live_session_puts_a_brand_new_shard_in_the_list()
    {
        using var sessions = new SessionStore(":memory:");
        var library = new LogLibrary(sessions);

        var stale = new SessionBuilder("Game.log");
        stale.Add(new Core.Events.ShardJoinEvent(Now.AddHours(-2), "pub_use1b_12545750_080", "1.2.3.4", 1, "x"));
        stale.Add(new Core.Events.DisconnectEvent(Now.AddHours(-1), "30016", "Remote Disconnect - Player requested disconnect", true, "SC_Default"));
        sessions.Save(stale.Build(), "f1");

        var live = new SessionBuilder("Game.log");
        live.Add(new Core.Events.ShardJoinEvent(Now.AddHours(-2), "pub_use1b_12545750_080", "1.2.3.4", 1, "x"));
        live.Add(new Core.Events.DisconnectEvent(Now.AddHours(-1), "30016", "Remote Disconnect - Player requested disconnect", true, "SC_Default"));
        live.Add(new Core.Events.ShardJoinEvent(Now.AddMinutes(-30), "pub_use1b_12545750_199", "1.2.3.4", 1, "x"));
        live.Add(new Core.Events.LoginEvent(Now, "nekron"));

        Assert.DoesNotContain(library.Shards(), r => r.Shard == "pub_use1b_12545750_199");

        var rows = library.Shards(live.Build());

        var fresh = Assert.Single(rows, r => r.Shard == "pub_use1b_12545750_199");
        Assert.Equal(ShardLeave.Open, fresh.LastEnding);
        Assert.Equal(TimeSpan.FromMinutes(30), fresh.Time);

        var older = Assert.Single(rows, r => r.Shard == "pub_use1b_12545750_080");
        Assert.Equal(1, older.Visits);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        try { Directory.Delete(_root, true); } catch (IOException) { }
    }
}
