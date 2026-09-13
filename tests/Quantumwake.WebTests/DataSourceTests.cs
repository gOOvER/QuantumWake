namespace Quantumwake.WebTests;

/// <summary>
/// Evidence labels use one vocabulary everywhere: logs, the install, local
/// screenshot reading, and an optional community download are not equivalent.
/// </summary>
public class DataSourceTests
{
    [Fact]
    public void The_dashboard_marks_live_and_screen_answers_with_their_evidence()
    {
        var page = new Page();

        Assert.Contains("GAME LOG", page.NodeText("#now-location-source"));
        Assert.Contains("SCREENSHOT / OCR", page.NodeText("#now-screen-source"));
        Assert.Contains("INFERRED", page.NodeText("#now-stats-source"));
    }

    [Fact]
    public void The_hangar_names_both_the_log_and_installed_game_data()
    {
        var page = new Page();
        var sources = page.NodeText("#hangar-data-sources");

        Assert.Contains("GAME LOG", sources);
        Assert.Contains("GAME INSTALL", sources);
    }
}
