namespace Quantumwake.Tests;

using Quantumwake.Core.Controls;

/// <summary>
/// The name a profile has to carry to exist as far as the game is concerned.
/// </summary>
/// <remarks>
/// The game's own export writes layout_&lt;name&gt;_exported.xml, and the one file
/// in this install's mappings folder - written by the game, not by us - is
/// layout_nick_exported.xml. The app wrote quantumwake-bindings.xml and told the
/// pilot to run "pp_rebindkeys quantumwake-bindings", so every export made while
/// the game was running went somewhere the game does not look. Nothing said so.
/// </remarks>
public class ControlsMappingNameTests
{
    [Fact]
    public void A_name_is_wrapped_in_the_form_the_game_lists()
    {
        Assert.Equal("layout_nick_exported.xml", ControlsExport.MappingFileName("nick"));
        Assert.Equal("layout_quantumwake-bindings_exported.xml", ControlsExport.MappingFileName("quantumwake-bindings"));
    }

    [Theory]
    [InlineData("layout_nick_exported")]
    [InlineData("layout_nick")]
    [InlineData("nick_exported")]
    [InlineData("LAYOUT_nick_EXPORTED")]
    public void A_name_that_already_carries_the_form_does_not_grow_a_second_one(string given)
    {
        // A pilot naming the export after the file they already have, or after a
        // round trip through the app, must not end at layout_layout_nick_exported_exported.
        Assert.Equal("layout_nick_exported.xml", ControlsExport.MappingFileName(given));
    }

    [Fact]
    public void An_unusable_name_still_produces_a_file_the_game_can_load()
    {
        Assert.Equal("layout_quantumwake_exported.xml", ControlsExport.MappingFileName(""));
        Assert.Equal("layout_quantumwake_exported.xml", ControlsExport.MappingFileName("   "));
        Assert.Equal("layout_quantumwake_exported.xml", ControlsExport.MappingFileName("layout__exported"));
    }

    [Fact]
    public void The_pilots_own_punctuation_is_made_safe_but_kept_readable()
    {
        Assert.Equal("layout_my_hotas_setup_exported.xml", ControlsExport.MappingFileName("my hotas setup"));
        Assert.Equal("layout_dual-vkb_exported.xml", ControlsExport.MappingFileName("dual-vkb"));
    }
}
