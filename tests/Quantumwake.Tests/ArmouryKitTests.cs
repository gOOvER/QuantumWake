using Quantumwake.Core.GameData;

namespace Quantumwake.Tests;

/// <summary>
/// The Armoury's third tab: what the reader decides about an attachment
/// from its class and name, and when an effect is nothing at all.
/// </summary>
/// <remarks>
/// The readers themselves walk <c>Game2.dcb</c> and are checked against
/// the install with <c>--armoury</c> (docs/armoury.md keeps the figures);
/// what can be pinned without the archive is the naming and the "changes
/// nothing" rule a flashlight is shown under.
/// </remarks>
public class ArmouryKitTests
{
    [Theory]
    [InlineData("arma_barrel_supp_s1", "Tacit Suppressor1", "Barrel", "Suppressor")]
    [InlineData("arma_barrel_comp_s2_04", "Stark Compensator2", "Barrel", "Compensator")]
    [InlineData("arma_barrel_stab_s3_02", "Escalate Stabilizer3", "Barrel", "Stabilizer")]
    [InlineData("arma_barrel_flhd_s1", "Veil Flash Hider1", "Barrel", "Flash hider")]
    [InlineData("behr_optics_tsco_x4_s2", "EE04 (4x Telescopic)", "Sight", "Telescopic")]
    [InlineData("klwe_optics_holo_x2_s1", "HG-2 Jaeger (2x Holographic)", "Sight", "Holographic")]
    [InlineData("nvtc_optics_rdot_x1_s1", "Delta (1x Reflex)", "Sight", "Reflex")]
    [InlineData("klwe_optics_disp_x8_s3", "Touchstone (8x Monitor)", "Sight", "Monitor")]
    [InlineData("nvtc_ubarrel_lasr_s1", "250-E Laser Pointer", "Underbarrel", "Laser pointer")]
    [InlineData("nvtc_ubarrel_flsh_s1", "FieldLite Flashlight", "Underbarrel", "Flashlight")]
    public void The_family_comes_from_the_class_token(string cls, string name, string kind, string family) =>
        Assert.Equal(family, GameArmoury.AttachmentFamily(cls, name, kind));

    /// <summary>A class without a known token falls back to the bracketed word, then the slot.</summary>
    [Fact]
    public void An_unknown_token_falls_back_to_the_name_then_the_slot()
    {
        Assert.Equal("Thermal", GameArmoury.AttachmentFamily("xxx_optics_new_x2_s1", "Seer (2x Thermal)", "Sight"));
        Assert.Equal("Barrel", GameArmoury.AttachmentFamily("xxx_barrel_new_s1", "Mystery Piece", "Barrel"));
    }

    [Fact]
    public void An_effect_of_all_ones_is_none_and_any_change_is_not()
    {
        Assert.True(new AttachmentEffect().IsNone);
        Assert.True(AttachmentEffect.None.IsNone);
        Assert.False(new AttachmentEffect(Sound: 0.66).IsNone);
        Assert.False(new AttachmentEffect(Zoom: 4).IsNone);
        Assert.False(new AttachmentEffect(Pellets: -3).IsNone);
    }

    /// <summary>
    /// The archive names an attachment's icon by class; a finish wears its
    /// plain item's, and a variant with no file of its own gets none rather
    /// than a neighbour's.
    /// </summary>
    [Fact]
    public void An_attachment_wears_the_archives_icon_for_its_class_or_its_plain_items()
    {
        WeaponAttachment Piece(string cls, string? baseClass = null) =>
            new(cls, cls, "Barrel", "Suppressor", 1, "ArmaMod", 0.1, AttachmentEffect.None, baseClass);
        var data = new GameArmouryData([], [], [], [], [
            Piece("arma_barrel_supp_s1"),
            Piece("arma_barrel_supp_s1_firerats01", "arma_barrel_supp_s1"),
            Piece("arma_barrel_supp_s1_02"),
        ]);

        GameArmoury.StampIcons(data, [
            GameArmoury.AttachmentIconFolder + "arma_barrel_supp_s1.dds",
            GameArmoury.AttachmentIconFolder + "nvtc_optics_rdot_x1_s1.dds",
            GameArmoury.AttachmentIconFolder[..^6] + "weapon_attachment_bckg.dds",
        ]);

        Assert.Equal(GameArmoury.AttachmentIconFolder + "arma_barrel_supp_s1.dds", data.Attachments[0].Icon);
        Assert.Equal(GameArmoury.AttachmentIconFolder + "arma_barrel_supp_s1.dds", data.Attachments[1].Icon);
        Assert.Null(data.Attachments[2].Icon);
    }

    /// <summary>
    /// A cache written by a build before the third tab deserialises with the
    /// three lists missing; they must come back empty, not null, or every
    /// caller has to know the history.
    /// </summary>
    [Fact]
    public void An_older_armoury_payload_has_empty_lists_rather_than_nulls()
    {
        var data = System.Text.Json.JsonSerializer.Deserialize<GameArmouryData>("""{"Weapons":[],"Armour":[]}""")!;

        Assert.Empty(data.Melee);
        Assert.Empty(data.Throwables);
        Assert.Empty(data.Attachments);
    }
}
