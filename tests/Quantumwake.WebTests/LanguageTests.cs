namespace Quantumwake.WebTests;

/// <summary>
/// The page in another language: the lookup, the plurals, and the picker.
/// </summary>
/// <remarks>
/// <para>
/// English is the key, so every other test in this project is also a test that
/// an English page is unchanged. What is left to prove is that a French page
/// actually comes out French, that a string nobody has translated yet still
/// reads, and that the counts agree with their nouns - the one place French and
/// English rules differ in a way a reader notices.
/// </para>
/// <para>
/// The markup half (<c>txDocument</c>) needs a parsed document, which the stub
/// is not; the catalogue tests guard its keys instead, and a screenshot is
/// what shows it working.
/// </para>
/// </remarks>
public class LanguageTests
{
    private const string Languages = """
        {"current":"en","available":[
          {"code":"en","name":"English","covers":null},
          {"code":"fr","name":"Français","covers":"traduit pour l’instant : la navigation"}]}
        """;

    [Fact]
    public void An_english_page_is_its_own_text()
    {
        var page = new Page();

        Assert.Equal("Open map", page.Text("tx('Open map')"));
        Assert.Equal("2 of 5 items in hand", page.Text("tx('{have} of {total} items in hand', { have: 2, total: 5 })"));
    }

    [Fact]
    public void A_french_page_reads_the_catalogue_and_fills_it()
    {
        var page = new Page("fr");

        Assert.Equal("Ouvrir la carte", page.Text("tx('Open map')"));
        Assert.Equal("2 objets sur 5 en main", page.Text("tx('{have} of {total} items in hand', { have: 2, total: 5 })"));
    }

    /// <summary>A partial language is a readable page, not a page of gaps.</summary>
    [Fact]
    public void A_string_nobody_has_translated_stays_english()
    {
        var page = new Page("fr");

        Assert.Equal("Nothing like this is in the catalogue", page.Text("tx('Nothing like this is in the catalogue')"));
    }

    /// <summary>
    /// The difference that matters: French puts nought with one, English with
    /// the many. A rule written as <c>count === 1</c> prints "0 journaux".
    /// </summary>
    [Theory]
    [InlineData(0, "0 journal")]
    [InlineData(1, "1 journal")]
    [InlineData(2, "2 journaux")]
    public void French_counts_take_french_plurals(int count, string expected)
    {
        var page = new Page("fr");

        Assert.Equal(expected, page.Text($"txn({count}, '{{count}} log', '{{count}} logs')"));
    }

    [Theory]
    [InlineData(0, "0 logs")]
    [InlineData(1, "1 log")]
    [InlineData(2, "2 logs")]
    public void English_counts_take_english_plurals(int count, string expected)
    {
        var page = new Page();

        Assert.Equal(expected, page.Text($"txn({count}, '{{count}} log', '{{count}} logs')"));
    }

    /// <summary>
    /// An untranslated plural on a French page is English words, so it takes
    /// English's rule - French's would print "0 widget".
    /// </summary>
    [Fact]
    public void An_untranslated_plural_keeps_the_english_rule()
    {
        var page = new Page("fr");

        Assert.Equal("0 widgets", page.Text("txn(0, '{count} widget', '{count} widgets')"));
    }

    [Fact]
    public void The_command_palette_speaks_the_page_language()
    {
        var page = new Page("fr");

        var labels = page.Text("commandEntries().map(entry => entry.label).join('|')");

        Assert.Contains("Afficher l’overlay", labels);
        Assert.Contains("Disposition d’overlay : minage", labels);
        Assert.DoesNotContain("overlay layout", labels);
    }

    /// <summary>
    /// Each language named in itself, and a partial one saying how far it has
    /// got - shown on an English page too, because that is the page a French
    /// reader is looking at when deciding.
    /// </summary>
    [Fact]
    public void The_picker_names_each_language_and_says_what_is_partial()
    {
        var page = new Page();
        page.Serve("/api/language", Languages);

        page.Do("await renderLanguageSettings();");

        Assert.Equal("English|Français",
            page.Text("__dom.node('#language-pick').children.map(o => o.textContent).join('|')"));
        Assert.Equal("en", page.Text("__dom.node('#language-pick').value"));
        Assert.Equal("Français — traduit pour l’instant : la navigation.", page.NodeText("#language-covers"));
        Assert.False(page.Truth("__dom.node('#language-covers').hidden"));
    }

    /// <summary>
    /// The choice is the server's, so the overlay follows it, and the page
    /// reloads to take it rather than re-rendering in place.
    /// </summary>
    [Fact]
    public void Choosing_a_language_saves_it_and_reloads()
    {
        var page = new Page();
        page.Serve("/api/language", Languages);
        page.Serve("/api/language?code=fr", """{"current":"fr"}""");

        page.Do("await renderLanguageSettings();");
        page.Do("__dom.node('#language-pick').value = 'fr'; await __dom.node('#language-pick').onchange();");

        Assert.Contains("POST /api/language?code=fr", page.Fetched());
        Assert.Equal(1, page.Count("__reloads"));
    }
}
