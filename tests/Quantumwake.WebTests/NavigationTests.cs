using System.Text.RegularExpressions;

namespace Quantumwake.WebTests;

public class NavigationTests
{
    [Fact]
    public void Dashboard_navigation_groups_each_view_by_its_purpose()
    {
        var markup = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "web", "index.html"));
        var navigation = markup[markup.IndexOf("<nav class=\"tabs\"", StringComparison.Ordinal)..];
        navigation = navigation[..navigation.IndexOf("</nav>", StringComparison.Ordinal)];

        Assert.DoesNotContain(">Jobs<span", navigation);
        Assert.DoesNotContain(">Gear<span", navigation);

        Assert.Contains("data-view=\"now\"", navigation);
        Assert.Contains("data-view=\"map\"", navigation);
        Assert.Contains("data-view=\"log\"", navigation);

        AssertViews(navigation, "Operations", "jobs", "routes", "contracts", "checklists", "blueprints", "wikelo");
        AssertViews(navigation, "Flight", "logbook", "sessions", "servers", "places", "points", "crew", "casualties");
        AssertViews(navigation, "Hangar", "fleet", "hangar", "loadout", "stash", "loot", "garage", "armoury");
        AssertViews(navigation, "Economy", "spending", "ledger", "commodities", "market");
        AssertViews(navigation, "Reference", "ships", "parts", "mining", "crafting");
        AssertViews(navigation, "Settings", "settings", "overlay", "controls", "labels", "imports", "about");
    }

    private static void AssertViews(string navigation, string group, params string[] views)
    {
        var match = Regex.Match(
            navigation,
            $"<div class=\"tab-group\">\\s*<button type=\"button\" class=\"group-btn\">{group}<span.*?</div>\\s*</div>",
            RegexOptions.Singleline);

        Assert.True(match.Success, $"{group} navigation group is missing.");
        foreach (var view in views)
            Assert.Contains($"data-view=\"{view}\"", match.Value);
    }
}
