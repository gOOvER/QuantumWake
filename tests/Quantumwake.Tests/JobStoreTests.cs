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
}
