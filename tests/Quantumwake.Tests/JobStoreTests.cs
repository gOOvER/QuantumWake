using Quantumwake.Data;

namespace Quantumwake.Tests;

public sealed class JobStoreTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"qw-jobs-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }

    [Fact]
    public void Replacing_an_open_list_keeps_one_current_fit()
    {
        var store = new JobStore(_root);
        var first = store.ReplaceOpenList("RSI Hermes fit", "garage:RSI_Hermes",
            [new JobItem("SolarFlare", 2)], "Terra", "P1");
        var second = store.ReplaceOpenList("RSI Hermes fit", "garage:RSI_Hermes",
            [new JobItem("SolarFlare", 2), new JobItem("JS-400", 1)], "Area18", "P2");

        var list = Assert.Single(store.All());
        Assert.True(first.Created);
        Assert.False(second.Created);
        Assert.Equal(first.Job.Id, list.Id);
        Assert.Equal(["SolarFlare", "JS-400"], list.Items.Select(item => item.Name));
        Assert.Equal("Area18", list.Destination);
    }

    [Fact]
    public void Replacing_a_fit_consolidates_the_old_automatic_part_list()
    {
        var store = new JobStore(_root);
        store.Add("RSI Hermes · SolarFlare", "list", "garage:RSI_Hermes", [new JobItem("SolarFlare", 2)]);

        var saved = store.ReplaceOpenList("RSI Hermes fit", "garage:RSI_Hermes",
            [new JobItem("SolarFlare", 2)], "Terra", "P1",
            new HashSet<string>(StringComparer.Ordinal)
            {
                "RSI Hermes · SolarFlare",
                "RSI Hermes - SolarFlare",
            });

        var list = Assert.Single(store.All());
        Assert.True(saved.Created);
        Assert.Single(saved.ConsolidatedIds);
        Assert.Equal("RSI Hermes fit", list.Title);
    }
    /// <summary>
    /// A destination the pilot pointed the list at on Shopping is theirs. The
    /// next rewrite of the list keeps it and says so, rather than writing the
    /// new proposal over it; a proposed destination is still replaced.
    /// </summary>
    [Fact]
    public void A_chosen_destination_survives_the_lists_next_rewrite()
    {
        var store = new JobStore(_root);
        var first = store.ReplaceOpenList("RSI Hermes fit", "garage:RSI_Hermes", [new JobItem("SolarFlare", 2)], "Terra", "P1");
        Assert.False(first.DestinationKept);

        // Proposed, not chosen: the next proposal takes its place.
        var proposed = store.ReplaceOpenList("RSI Hermes fit", "garage:RSI_Hermes", [new JobItem("SolarFlare", 2)], "Area18", "P2");
        Assert.Equal("Area18", proposed.Job.Destination);
        Assert.False(proposed.DestinationKept);

        Assert.True(store.SetDestination(first.Job.Id, "Orison", "P3"));

        var rewritten = store.ReplaceOpenList("RSI Hermes fit", "garage:RSI_Hermes", [new JobItem("SolarFlare", 2), new JobItem("JS-400", 1)], "Lorville", "P4");
        Assert.True(rewritten.DestinationKept);
        Assert.Equal("Orison", rewritten.Job.Destination);
        Assert.Equal("P3", rewritten.Job.DestinationId);
        Assert.Equal(2, rewritten.Job.Items.Count);

        // Clearing the destination hands the choice back.
        Assert.True(store.SetDestination(first.Job.Id, null, null));
        var again = store.ReplaceOpenList("RSI Hermes fit", "garage:RSI_Hermes", [new JobItem("SolarFlare", 2)], "Lorville", "P4");
        Assert.False(again.DestinationKept);
        Assert.Equal("Lorville", again.Job.Destination);

        Assert.Equal(first.Job.Id, store.OpenList("garage:RSI_Hermes", "RSI Hermes fit")!.Id);
        Assert.Null(store.OpenList("garage:RSI_Hermes", "Quiet fit"));
    }
}
