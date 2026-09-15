using Quantumwake.Data;

namespace Quantumwake.Tests;

/// <summary>
/// Saved builds: a fit under a name, kept like the other authored files.
/// </summary>
public class BuildStoreTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"qw-builds-{Guid.NewGuid():N}");
    private static readonly DateTimeOffset Now = new(2026, 9, 14, 20, 0, 0, TimeSpan.Zero);

    public BuildStoreTests() => Directory.CreateDirectory(_root);

    [Fact]
    public void A_build_keeps_its_swaps_across_a_restart()
    {
        var store = new BuildStore(_root);
        var build = store.Add("MISC_Starlancer_Max", "  Quiet fit ", new Dictionary<string, string?>
        {
            ["loadout.15"] = "COOL_TYDT_S02_NightFall_SCItem",
            ["loadout.16"] = "COOL_TYDT_S02_NightFall_SCItem",
            ["loadout.20"] = null,
        });

        Assert.NotNull(build);
        Assert.Equal("Quiet fit", build.Name);

        var back = Assert.Single(new BuildStore(_root).For("MISC_Starlancer_Max"));
        Assert.Equal(3, back.Swaps.Count);
        Assert.Equal("COOL_TYDT_S02_NightFall_SCItem", back.Swaps["loadout.15"]);
        Assert.Null(back.Swaps["loadout.20"]);
    }

    [Fact]
    public void A_build_without_a_ship_is_refused()
    {
        var store = new BuildStore(_root);
        Assert.Null(store.Add("  ", "Nameless", null));
        Assert.Empty(store.All());
    }

    [Fact]
    public void Update_touches_only_what_was_sent()
    {
        var store = new BuildStore(_root);
        var build = store.Add("AEGS_Gladius", "Stealth", new Dictionary<string, string?> { ["p1"] = "A" })!;

        var renamed = store.Update(build.Id, "Stealth II", null, null)!;
        Assert.Equal("Stealth II", renamed.Name);
        Assert.Equal("A", renamed.Swaps["p1"]);

        var refitted = store.Update(build.Id, null, new Dictionary<string, string?> { ["p2"] = "B" }, null)!;
        Assert.Equal("Stealth II", refitted.Name);
        Assert.Single(refitted.Swaps);
        Assert.Equal("B", refitted.Swaps["p2"]);

        Assert.Null(store.Update("nope", "x", null, null));
    }

    [Fact]
    public void Builds_are_in_the_backup_and_come_back_from_one()
    {
        var store = new BuildStore(_root);
        store.Add("AEGS_Gladius", "Stealth", new Dictionary<string, string?> { ["p1"] = "COOL_JUST_S01_Glacier_SCItem" });

        var backup = new BackupBuilder(
            new JobStore(_root), new ChecklistStore(_root), new TripStore(_root),
            new MiningLogStore(_root), new MapNoteStore(_root), new GoalStore(_root),
            new WipeStore(_root), new ItemLabelStore(_root), new TombstoneStore(_root),
            new KitStore(_root), new ScreenReadingStore(_root), new ShardNoteStore(_root), store);

        var file = backup.Build(new ExportProducer("Quantumwake", "0.13.0"), Now).Backup!;
        Assert.Single(file.Builds!);
        Assert.Equal(1, backup.Preview().Builds);

        var other = Path.Combine(_root, "other");
        Directory.CreateDirectory(other);
        using var sessions = new SessionStore(":memory:");
        var target = new BuildStore(other);

        var restore = new RestoreService(
            new JobStore(other), new ChecklistStore(other), new TripStore(other),
            new MiningLogStore(other), new MapNoteStore(other), new GoalStore(other),
            new WipeStore(other), new ItemLabelStore(other), new TombstoneStore(other), new LogLibrary(sessions),
            new KitStore(other), new ScreenReadingStore(other), new ShardNoteStore(other), target);

        restore.Apply(file, restore.Plan(file, "h"), "h", new RestoreChoices());

        var landed = Assert.Single(target.All());
        Assert.Equal("Stealth", landed.Name);
        Assert.Equal("COOL_JUST_S01_Glacier_SCItem", landed.Swaps["p1"]);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        try { Directory.Delete(_root, true); } catch (IOException) { }
    }
}
