namespace Quantumwake.WebTests;

/// <summary>
/// The dashboard's way into MFD setup: a request to the app, and an honest
/// answer about whether anything heard it.
/// </summary>
public class MfdSetupButtonTests
{
    [Fact]
    public void The_button_asks_the_app_to_open_setup()
    {
        var page = new Page();
        page.Serve("/api/mfd/setup", """{"opened":true}""");

        page.Do("__dom.node('#mfd-setup-open').fire('click');");

        Assert.Contains("POST /api/mfd/setup", page.Fetched());
        Assert.Contains("MFD setup is open", page.NodeText("#mfd-setup-status"));
    }

    /// <summary>Under the bare server nothing can open it, and the page repeats why.</summary>
    [Fact]
    public void Without_the_app_the_page_says_so()
    {
        var page = new Page();
        page.Fail("/api/mfd/setup", 409, """{"message":"MFD setup opens from the Quantum Wake app."}""");

        page.Do("await openMfdSetup();");

        Assert.Equal("MFD setup opens from the Quantum Wake app.", page.NodeText("#mfd-setup-status"));
    }

    [Fact]
    public void The_command_palette_offers_it()
    {
        var page = new Page();

        Assert.Contains("Open MFD setup", page.Text("commandEntries().map(e => e.label).join('|')"));
    }
}
