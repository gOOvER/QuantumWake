namespace Quantumwake.WebTests;

/// <summary>
/// The Mining page's three panes: where to go, can it be cracked, your
/// runs. One at a time, remembered, and a rock handed over from the Log
/// opens on the calculator.
/// </summary>
public class MiningPanesTests
{
    private static string Active(Page page, string id) => page.Text($"__dom.node('#{id}').classList.contains('active') ? 'on' : 'off'");

    [Fact]
    public void One_pane_shows_at_a_time_and_the_choice_is_remembered()
    {
        var page = new Page();
        page.Do("showMiningPane('crack');");

        Assert.Equal("on", Active(page, "mining-pane-crack"));
        Assert.Equal("off", Active(page, "mining-pane-go"));
        Assert.Equal("off", Active(page, "mining-pane-runs"));
        Assert.Equal("crack", page.Text("localStorage.getItem('qw-mining-pane')"));
        // The deposit filters belong to the first pane alone.
        Assert.True(page.Truth("__dom.node('#mining-kind').hidden"));

        page.Do("showMiningPane('go');");
        Assert.Equal("on", Active(page, "mining-pane-go"));
        Assert.False(page.Truth("__dom.node('#mining-kind').hidden"));

        page.Do("showMiningPane('nonsense');");
        Assert.Equal("on", Active(page, "mining-pane-go"));
    }

    [Fact]
    public void A_rock_handed_over_from_the_log_opens_the_calculator()
    {
        var page = new Page();
        page.Serve("/api/mining/model", """{"ready":false,"lasers":[],"modules":[],"gadgets":[],"minerals":[],"ships":[]}""");
        page.Do("window.scrollTo = () => {}; localStorage.setItem('qw-mining-pane', 'go'); miningPane = 'go'; crackFromScan = {massKg: 5000}; showView('mining');");

        Assert.Equal("on", Active(page, "mining-pane-crack"));
    }

    [Fact]
    public void Salvage_hides_the_mining_only_prospect_ranking()
    {
        var page = new Page();
        page.Serve("/api/reference/resources", """
            [{"resource":"Wreckage","deposit":null,"minPercent":null,"maxPercent":null,
              "kind":"salvageable","location":"Daymar","system":"Stanton","group":"Salvage",
              "groupChance":0.5,"share":0.4,"bestSell":null,"quality":null,"source":"install"}]
            """);

        page.Do("await loadMiningRef(); __dom.node('#mining-kind').value = 'salvageable'; renderMiningRef();");

        Assert.True(page.Truth("__dom.node('#mining-prospect-board').hidden"));
        Assert.False(page.Truth("__dom.node('#mining-salvage-brief').hidden"));
        Assert.True(page.Truth("__dom.node('#mining-table').classList.contains('salvage-table')"));
    }
}
