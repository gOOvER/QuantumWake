namespace Quantumwake.WebTests;

/// <summary>
/// The shared context is a return path, not another permanent header panel.
/// </summary>
public class ContextRailTests
{
    [Fact]
    public void A_selected_place_shows_its_compact_return_path()
    {
        var page = new Page();

        page.Do("""
            setContextFocus({
              kind: 'place', title: 'GrimHEX', detail: 'Yela · Stanton',
              view: 'map', action: 'Open map'
            });
            """);

        Assert.False(page.Truth("__dom.node('#context-rail').hidden"));
        Assert.Equal("GrimHEX", page.NodeText("#context-title"));
        Assert.Equal("Yela · Stanton", page.NodeText("#context-detail"));
        Assert.Equal("Open map", page.NodeText("#context-action"));
    }

    [Fact]
    public void Clearing_focus_hides_the_rail_entirely()
    {
        var page = new Page();

        page.Do("""
            setContextFocus({ kind: 'shopping', title: 'Refit', view: 'jobs' });
            __dom.node('#context-clear').click();
            """);

        Assert.True(page.Truth("__dom.node('#context-rail').hidden"));
        Assert.Equal("null", page.Text("String(contextFocus)"));
    }

    [Fact]
    public void The_action_returns_to_the_promised_workspace()
    {
        var page = new Page();

        page.Do("""
            window.scrollTo = () => {};
            setContextFocus({
              kind: 'contract', title: 'Courier work', detail: 'Hurston',
              view: 'jobs', anchor: '#jobs-contracts', action: 'Open contracts'
            });
            __dom.node('#context-action').onclick();
            """);

        Assert.True(page.Truth("__dom.node('#view-jobs').classList.contains('active')"));
    }

    [Fact]
    public void Leaving_the_map_closes_its_detail_drawer_while_focus_remains()
    {
        var page = new Page();

        page.Do("""
            window.scrollTo = () => {};
            entityShown = 'place|grimhex';
            __dom.node('#entity-drawer').hidden = false;
            setContextFocus({ kind: 'place', title: 'GrimHEX', view: 'map' });
            showView('fleet');
            """);

        Assert.True(page.Truth("__dom.node('#entity-drawer').hidden"));
        Assert.False(page.Truth("__dom.node('#context-rail').hidden"));
    }
}
