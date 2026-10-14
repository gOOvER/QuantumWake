namespace Quantumwake.WebTests;

/// <summary>
/// The Now page in French, including the feed - whose sentences the parser
/// wrote in English and the page translates by shape.
/// </summary>
/// <remarks>
/// The feed is the one place where English text arrives finished rather than
/// as a key, so what is proved here is the shape matching: names survive,
/// the longer shapes win over their prefixes ("Bought 12 SCU" is not "Bought
/// an item called 12 SCU"), and a sentence nobody taught the page is shown as
/// the parser wrote it rather than dropped.
/// </remarks>
public class NowFrenchTests
{
    private const string Feed =
        "recentEvents:["
        + "{at:'2026-10-12T20:00:00Z',kind:'quantum',text:'Quantum to Port Tressler',detail:'Hurston'},"
        + "{at:'2026-10-12T20:01:00Z',kind:'bought',text:'Bought 12 SCU',detail:'1,200 aUEC · Admin'},"
        + "{at:'2026-10-12T20:02:00Z',kind:'ship',text:'Boarded Anvil Hornet',detail:null},"
        + "{at:'2026-10-12T20:03:00Z',kind:'party',text:'astro_ice joined the party',detail:'in your party'},"
        + "{at:'2026-10-12T20:04:00Z',kind:'contract',text:'Contract accepted',detail:'Haul | Ruin Station'},"
        + "{at:'2026-10-12T20:05:00Z',kind:'mystery',text:'Something the page was never taught',detail:null}]";

    private static Page Now(string? language, string extra)
    {
        var page = new Page(language);
        page.Serve("/api/briefing", "{}");
        page.Serve("/api/trips", "[]");
        page.Do($"renderNow({{ connected:true, inGame:true, location:'Port Tressler', confidence:'High', {extra} }});");
        return page;
    }

    [Fact]
    public void The_feed_reads_in_french_and_keeps_its_names()
    {
        var feed = Now("fr", Feed).NodeText("#now-feed");

        Assert.Contains("Saut quantique vers Port Tressler", feed);
        Assert.Contains("12 SCU achetés", feed);
        Assert.Contains("Embarqué : Anvil Hornet", feed);
        Assert.Contains("astro_ice a rejoint le groupe", feed);
        Assert.Contains("dans votre groupe", feed);
        Assert.Contains("Contrat accepté", feed);
        Assert.Contains("Haul | Ruin Station", feed);
        Assert.DoesNotContain("Quantum to", feed);
    }

    [Fact]
    public void A_sentence_the_page_was_never_taught_is_shown_as_written()
    {
        Assert.Contains("Something the page was never taught", Now("fr", Feed).NodeText("#now-feed"));
    }

    [Fact]
    public void An_english_page_shows_the_feed_exactly_as_the_parser_wrote_it()
    {
        var feed = Now(null, Feed).NodeText("#now-feed");

        Assert.Contains("Quantum to Port Tressler", feed);
        Assert.Contains("Bought 12 SCU", feed);
        Assert.Contains("astro_ice joined the party", feed);
    }

    [Fact]
    public void The_kind_column_is_french_but_its_class_is_the_parsers()
    {
        var page = Now("fr", Feed);

        Assert.Contains("quantique", page.NodeText("#now-feed"));
        Assert.Equal(1, page.Count("__dom.node('#now-feed').querySelectorAll('.quantum').length"));
    }

    [Fact]
    public void Status_and_party_read_in_french()
    {
        var page = Now("fr", "recentEvents:[], party:[{handle:'B-Kon',moment:'joined',at:'2026-10-12T20:03:59Z'}]");

        Assert.Equal("confiance élevée", page.NodeText("#now-confidence"));
        Assert.Contains("a rejoint", page.NodeText("#now-party-list"));
        Assert.StartsWith("1 personne nommée", page.NodeText("#now-party-note"));
    }

    [Fact]
    public void The_in_menus_line_is_not_taken_for_a_place_by_routes()
    {
        var page = Now("fr", "recentEvents:[], location:null, inGame:false");

        Assert.Equal("Dans les menus", page.NodeText("#now-location"));
        page.Do("__dom.node('#routes-here').checked = true; await loadRoutes();");
        Assert.Contains(page.Fetched(), url => url.Contains("/api/routes?") && url.Contains("from=&"));
    }
}
