using Quantumwake.Data;

namespace Quantumwake.Tests;

/// <summary>
/// The mining HUD's scan-results panel, read.
/// </summary>
/// <remarks>
/// The fixture is the engine's own output on the Star Citizen Wiki's
/// <c>Mining-4.7-scan-result.png</c> - a 227 × 310 crop, read 2026-09-15 -
/// kept as returned, misreadings included: the mass label came back as
/// "NASS:" with its figure dropped, 5.96% as "s.gs%", 40.47% as "4147%",
/// "ALUMINUM" once as "ÄLUMNUX" and "INERT" as "WERT". What is defended is
/// that the frame is filed as a scan, that every figure the engine did return
/// lands in its field, and that a figure it did not return is null and not
/// a guess. No frame from this install exists yet; the first will replace this.
/// </remarks>
public class ScreenMiningTests
{
    /// <summary>Mining-4.7-scan-result.png, as the engine read it at 227 × 310.</summary>
    private static readonly ScreenTextLine[] ScanResults =
    [
        new("SCAN RESULTS", 18, 25, 16),
        new("ALUMINUM (ORE)", 13, 60, 11),
        new("NASS:", 14, 84, 14),
        new("RESISTANCE:", 14, 106, 14),
        new("INSTABILITY: 1.75", 14, 127, 13),
        new("COMPOSITION", 13, 189, 14),
        new("21.07 scu", 147, 188, 11),
        new("s.gs%", 13, 232, 10),
        new("3.85%", 13, 249, 11),
        new("49.70%", 13, 267, 10),
        new("4147%", 12, 284, 11),
        new("CORUNDUM (RAW)", 69, 232, 10),
        new("ÄLUMNUX (ORE)", 68, 246, 12),
        new("ALUMINUM (ORE)", 68, 267, 10),
        new("WERT MATERIALS", 68, 284, 9),
        new("4øø", 183, 232, 11),
        new("789", 183, 250, 10),
        new("367", 183, 268, 10),
    ];

    /// <summary>The same panel with the figures the engine dropped at that size, as the game prints them.</summary>
    private static readonly ScreenTextLine[] ScanResultsFull =
    [
        new("SCAN RESULTS", 18, 25, 16),
        new("ALUMINUM (ORE)", 13, 60, 11),
        new("MASS:", 14, 84, 14),
        new("6295", 120, 84, 14),
        new("RESISTANCE:", 14, 106, 14),
        new("0%", 120, 106, 14),
        new("INSTABILITY:", 14, 127, 13),
        new("1.75", 120, 127, 13),
        new("EASY", 90, 155, 12),
        new("COMPOSITION", 13, 189, 14),
        new("21.07 SCU", 147, 188, 11),
        new("5.96%", 13, 232, 10),
        new("CORUNDUM (RAW)", 69, 232, 10),
        new("400", 183, 232, 11),
        new("3.85%", 13, 249, 11),
        new("ALUMINUM (ORE)", 68, 249, 12),
        new("789", 183, 250, 10),
        new("49.70%", 13, 267, 10),
        new("ALUMINUM (ORE)", 68, 267, 10),
        new("367", 183, 268, 10),
        new("40.47%", 12, 284, 11),
        new("INERT MATERIALS", 68, 284, 9),
        new("0", 183, 284, 10),
    ];

    private static readonly string[] Commodities = ["Aluminum (Ore)", "Corundum (Raw)", "Quantainium (Raw)", "Gold (Ore)", "Aluminum"];

    [Fact]
    public void The_panel_is_filed_as_a_scan_and_every_figure_the_engine_returned_lands()
    {
        var frame = ScreenFrames.Read(ScanResults, [], [], Commodities);

        Assert.Equal(ScreenKind.Mining, frame.Kind);
        var scan = frame.Mining!;

        Assert.Equal("ALUMINUM (ORE)", scan.PrimaryRead);
        Assert.Equal("Aluminum (Ore)", scan.Primary);
        Assert.Null(scan.MassKg);                 // the engine dropped the figure; nothing is invented
        Assert.Null(scan.ResistancePercent);      // "RESISTANCE:" came back with no figure
        Assert.Equal(1.75, scan.Instability);
        Assert.Equal(21.07, scan.Scu);
        Assert.Null(scan.Difficulty);

        Assert.Equal(4, scan.Parts.Count);
        var corundum = scan.Parts[0];
        Assert.Equal("Corundum (Raw)", corundum.Mineral);
        Assert.Equal(0, corundum.Percent);          // "s.gs%" is no share
        Assert.Null(corundum.Quality);              // "4øø" is no figure

        var mangled = scan.Parts[1];
        Assert.Equal("ÄLUMNUX (ORE)", mangled.Read);
        Assert.Null(mangled.Mineral);               // three letters off is a different word; kept as read, named by nobody
        Assert.Equal(3.85, mangled.Percent);
        Assert.Equal(789, mangled.Quality);

        Assert.Equal(49.70, scan.Parts[2].Percent);
        Assert.Equal(367, scan.Parts[2].Quality);

        var inert = scan.Parts[3];
        Assert.Equal("Inert materials", inert.Mineral);
        Assert.Equal(0, inert.Percent);             // "4147%" is over a hundred and is no share
    }

    [Fact]
    public void At_full_size_every_field_reads_and_the_summary_says_the_rock()
    {
        var frame = ScreenFrames.Read(ScanResultsFull, [], [], Commodities);

        Assert.Equal(ScreenKind.Mining, frame.Kind);
        var scan = frame.Mining!;
        Assert.Equal(6295, scan.MassKg);
        Assert.Equal(0, scan.ResistancePercent);
        Assert.Equal(1.75, scan.Instability);
        Assert.Equal(21.07, scan.Scu);
        Assert.Equal("EASY", scan.Difficulty);
        Assert.Equal([5.96, 3.85, 49.70, 40.47], scan.Parts.Select(p => p.Percent));
        Assert.Equal([400, 789, 367, 0], scan.Parts.Select(p => p.Quality));
    }

    /// <summary>The fracture HUD prints resistance and instability too; without the title it is not the scan.</summary>
    [Fact]
    public void Resistance_and_instability_alone_are_not_a_scan()
    {
        ScreenTextLine[] fracture =
        [
            new("0.71  0.71  Instability", 90, 406, 18),
            new("0.09  0.09  Resistance", 90, 450, 15),
            new("Laser Throttle", 99, 539, 16),
            new("Fracture Mode", 634, 663, 20),
        ];

        Assert.NotEqual(ScreenKind.Mining, ScreenFrames.Read(fracture, [], [], Commodities).Kind);
    }
}
