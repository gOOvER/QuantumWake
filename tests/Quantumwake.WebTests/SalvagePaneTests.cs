namespace Quantumwake.WebTests;

/// <summary>
/// The Salvage pane: the hulls with what they make and what a full hold of
/// it fetches, the modules and heads priced, and the one thing the pane
/// cannot say - what a wreck is worth - said in words rather than as a
/// column of blanks.
/// </summary>
public class SalvagePaneTests
{
    private const string Model = """
        {"ready":true,"constants":{"hullThicknessMetres":0.009,"ammoToMaterialFactor":100},
         "modules":[{"class":"Salvage_Modifier_Scraper_Medium","name":"Abrade Scraper Module","speed":0.15,"radius":3.5,"efficiency":0.9,"manufacturer":"Greycat Industrial",
                     "market":{"price":20188,"shops":[{"terminal":"Platinum Everus","place":"Everus Harbor","system":"Stanton","price":20188}]}},
                    {"class":"Salvage_Modifier_Scraper_Large","name":"Trawler Scraper Module","speed":0.05,"radius":6,"efficiency":0.6,"manufacturer":"Greycat Industrial","market":{"price":null,"shops":[]}}],
         "heads":[{"class":"Salvage_Head_standard","name":"Baler Salvage Head","slots":2,"manufacturer":"Greycat Industrial","market":{"price":null,"shops":[]}}],
         "ships":[{"ship":"DRAK_Vulture","name":"Drake Vulture","scrapesTo":"Recycled Material Composite","scuPerCubicMetre":0.0027,"disintegratesTo":"Construction Rubble","heads":2,"boxSecondsPerScu":3,
                   "hold":12,"flown":true,
                   "scrape":{"commodity":"Recycled Material Composite","perScu":7700,"at":"Patch City"},
                   "pieces":{"commodity":"Construction Materials","perScu":13000,"at":"ARC-L3"},
                   "fullHoldOfScrape":92400,"fullHoldOfPieces":156000}],
         "itemPricesKnown":true,"holdsKnown":true,
         "perHull":"What a given hull is worth scraped is not in the game files: the rule is there and the hull's area and volume it would apply to are not, nor in UEX, nor in the logs."}
        """;

    [Fact]
    public void The_hulls_say_what_they_make_and_what_a_full_hold_fetches()
    {
        var page = new Page();
        page.Serve("/api/salvage/model", Model);
        page.Do("await loadSalvage();");

        Assert.True(page.Truth("__dom.node('#salvage-unready').hidden"));
        var row = page.Text("__dom.node('#salvage-ships tbody').children[0].textContent");
        Assert.Contains("Drake Vulture", row);
        Assert.Contains("flown", row);
        Assert.Contains("Recycled Material Composite", row);
        Assert.Contains("0.0027 SCU/m³ of Construction Rubble", row);
        Assert.Contains("12 SCU", row);
        Assert.Contains("92,400 aUEC", row);
        Assert.Contains("156,000 aUEC", row);

        var note = page.NodeText("#salvage-ships-note");
        Assert.Contains("RMC at UEX's best sell: 7,700 aUEC a SCU at Patch City", note);
        Assert.Contains("sold as that one commodity", note);
        Assert.Contains("ceiling on a trip", note);

        var modules = "__dom.node('#salvage-modules tbody').children";
        // Widest beam first.
        Assert.Contains("Trawler", page.Text($"{modules}[0].textContent"));
        Assert.Contains("6 m", page.Text($"{modules}[0].textContent"));
        Assert.Contains("60%", page.Text($"{modules}[0].textContent"));
        Assert.Contains("no terminal recorded", page.Text($"{modules}[0].textContent"));
        Assert.Contains("20,188 aUEC", page.Text($"{modules}[1].textContent"));
        Assert.Contains("Platinum Everus, Everus Harbor", page.Text($"{modules}[1].textContent"));

        Assert.Contains("Baler Salvage Head", page.NodeText("#salvage-heads tbody"));
        Assert.Contains("a beam takes 9 mm of hull", page.NodeText("#salvage-note"));
        Assert.Contains("not in the game files", page.NodeText("#salvage-note"));
    }

    [Fact]
    public void Before_the_install_is_read_the_pane_says_so()
    {
        var page = new Page();
        page.Serve("/api/salvage/model", """{"ready":false,"modules":[],"heads":[],"ships":[]}""");
        page.Do("await loadSalvage();");
        Assert.False(page.Truth("__dom.node('#salvage-unready').hidden"));
        Assert.True(page.Truth("__dom.node('#salvage-ships').hidden"));
    }
}
