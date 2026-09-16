using System.Globalization;
using System.Text.RegularExpressions;

namespace Quantumwake.Data;

/// <summary>One line of a scanned rock's composition.</summary>
/// <param name="Read">The mineral as the engine returned it.</param>
/// <param name="Mineral">The commodity table's name for it, when exactly one fits.</param>
/// <param name="Percent">Its share of the rock.</param>
/// <param name="Quality">The figure the panel prints beside it - the game's quality, 0 for inert material.</param>
public sealed record MiningScanPart(string Read, string? Mineral, double Percent, int? Quality);

/// <summary>What the scan-results panel said about a rock.</summary>
/// <param name="PrimaryRead">The mineral named under the title, as read.</param>
/// <param name="Primary">The commodity table's name for it, when exactly one fits.</param>
/// <param name="MassKg">The rock's mass. Kilograms, as the panel's figure is understood.</param>
/// <param name="ResistancePercent">The panel's resistance, in percent.</param>
/// <param name="Instability">The panel's instability, as printed - 1.75 on the frame this was written from, not a percentage.</param>
/// <param name="Scu">The SCU the panel prints beside COMPOSITION: what the rock would fill.</param>
/// <param name="Difficulty">The word on the bar under the figures - EASY on the frame this was written from.</param>
public sealed record MiningScanReading(
    string? PrimaryRead,
    string? Primary,
    double? MassKg,
    double? ResistancePercent,
    double? Instability,
    double? Scu,
    string? Difficulty,
    IReadOnlyList<MiningScanPart> Parts);

/// <summary>
/// The ship mining HUD's scan-results panel.
/// </summary>
/// <remarks>
/// <para>
/// Written from one frame: the Star Citizen Wiki's <c>Mining-4.7-scan-result.png</c>,
/// a 227 × 310 crop of the panel in Alpha 4.7, read on 2026-09-15 with the
/// same engine the app uses. The panel prints, top to bottom: SCAN RESULTS;
/// the primary mineral (ALUMINUM (ORE)); MASS: 6295; RESISTANCE: 0%;
/// INSTABILITY: 1.75; a difficulty bar (EASY); COMPOSITION with 21.07 SCU on
/// the same row; then one row a mineral - share, name, quality - with INERT
/// MATERIALS last at quality 0. At that size the engine read every label and
/// dropped the mass figure and misread two shares (5.96% as "s.gs%", 40.47%
/// as "4147%"); a screenshot at the game's own resolution is four times the
/// height and is what this will meet. No frame from this install yet - the
/// logs record no mining and the folder holds no scan - so the first one a
/// pilot takes is what the next version of this gets written from.
/// </para>
/// <para>
/// The SCU figure is the one the calculator could not otherwise have: the
/// game's own conversion of this rock's mass to cargo, printed on the panel.
/// </para>
/// </remarks>
public static partial class ScreenFrames
{
    private static readonly Regex PercentFigure = new(@"^(\d{1,3}(?:[.,]\d{1,2})?)\s*%$", RegexOptions.Compiled);
    private static readonly Regex ScuFigure = new(@"(\d+(?:[.,]\d+)?)\s*SCU", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Decimal = new(@"\d+(?:[.,]\d+)?", RegexOptions.Compiled);

    private static MiningScanReading? ReadMiningScan(IReadOnlyList<ScreenTextLine> lines, IReadOnlyList<string> commodityNames)
    {
        var title = lines.FirstOrDefault(line => Is(line.Text, "SCAN RESULTS"));
        var resistance = lines.FirstOrDefault(line => Opens(line.Text, "RESISTANCE"));
        var instability = lines.FirstOrDefault(line => Opens(line.Text, "INSTABILITY"));
        // The title alone is not enough - other panels say "results" - and the
        // two figures alone are the fracture HUD's too. Together they are the scan.
        if (title is null || resistance is null || instability is null) return null;

        // The mass label read as "NASS:" on the frame this was written from;
        // M and N are one stroke apart in this face. The figure is on the row.
        var mass = lines.FirstOrDefault(line => Opens(line.Text, "MASS") || Opens(line.Text, "NASS"));
        var massKg = mass is null ? null : FigureOn(lines, mass, allowPercent: false);
        var resistancePercent = FigureOn(lines, resistance, allowPercent: true);
        var instabilityFigure = FigureOn(lines, instability, allowPercent: false);

        // The primary mineral sits between the title and the mass line.
        var primaryLine = lines
            .Where(line => line.Top > title.Top && (mass is null || line.Top < mass.Top) && line != resistance && line != instability)
            .Where(line => line.Text.Any(char.IsLetter) && !Decimal.IsMatch(line.Text))
            .OrderBy(line => line.Top)
            .FirstOrDefault();
        var primaryRead = primaryLine?.Text;
        var primary = primaryRead is null ? null : NameMineral(primaryRead, commodityNames);

        var composition = lines.FirstOrDefault(line => Opens(line.Text, "COMPOSITION"));
        double? scu = null;
        var scuLine = lines.FirstOrDefault(line => ScuFigure.IsMatch(line.Text));
        if (scuLine is not null && ScuFigure.Match(scuLine.Text) is { Success: true } m)
            scu = ParseFigure(m.Groups[1].Value);

        // The difficulty bar: a single word between the figures and COMPOSITION.
        var difficulty = composition is null ? null : lines
            .Where(line => line.Top > instability.Top && line.Top < composition.Top)
            .Select(line => line.Text.Trim())
            .FirstOrDefault(text => text.Length is >= 3 and <= 12 && text.All(c => char.IsLetter(c) || c == ' '));

        // The rows: a share at the left, the name to its right, the quality at
        // the far right, each on its own row below COMPOSITION. A share the
        // engine mangled ("s.gs%") is a row without a share, kept by name.
        var parts = new List<MiningScanPart>();
        if (composition is not null)
        {
            var below = lines.Where(line => line.Top > composition.Top + composition.Height * 0.5).ToList();
            var names = below
                .Where(line => line.Text.Count(char.IsLetter) >= 4 && !ScuFigure.IsMatch(line.Text))
                .OrderBy(line => line.Top)
                .ToList();

            foreach (var name in names)
            {
                var row = below.Where(line => line != name && Math.Abs(line.Top - name.Top) <= name.Height * 1.4).ToList();
                var share = row.Where(line => line.Left < name.Left).Select(line => PercentFigure.Match(line.Text.Replace(" ", "")))
                    .Where(x => x.Success).Select(x => ParseFigure(x.Groups[1].Value)).FirstOrDefault(v => v is not null);
                var quality = row.Where(line => line.Left > name.Left).Select(line => Plain(line.Text))
                    .Where(text => text.Length > 0 && text.All(char.IsDigit))
                    .Select(text => int.TryParse(text, out var q) ? q : (int?)null)
                    .FirstOrDefault(q => q is not null);

                var inert = ScreenInsight.Fold(name.Text).Contains("INERT", StringComparison.Ordinal)
                    || ScreenInsight.Fold(name.Text).Contains("MATERIAIS", StringComparison.Ordinal);
                parts.Add(new MiningScanPart(name.Text, inert ? "Inert materials" : NameMineral(name.Text, commodityNames), share ?? 0, quality));
            }
        }

        return new MiningScanReading(primaryRead, primary, massKg, resistancePercent, instabilityFigure, scu, difficulty, parts);
    }

    /// <summary>The figure on a label's row: after the colon in the label itself, or in the line beside it.</summary>
    private static double? FigureOn(IReadOnlyList<ScreenTextLine> lines, ScreenTextLine label, bool allowPercent)
    {
        var colon = label.Text.IndexOf(':');
        var tail = colon >= 0 ? label.Text[(colon + 1)..].Trim() : "";
        if (tail.Length > 0 && Decimal.Match(tail) is { Success: true } inline)
            return ParseFigure(inline.Value);

        var beside = lines
            .Where(line => line != label && line.Left > label.Left)
            .Where(line => Math.Abs(line.Top - label.Top) <= label.Height * 1.2)
            .OrderBy(line => line.Left)
            .Select(line => line.Text.Trim())
            .FirstOrDefault(text => Decimal.IsMatch(text) && (allowPercent || !text.Contains('%')));
        return beside is null ? null : ParseFigure(Decimal.Match(beside).Value);
    }

    private static double? ParseFigure(string text) =>
        double.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : null;

    /// <summary>The commodity table's name for a mineral as read, when exactly one fits.</summary>
    private static string? NameMineral(string read, IReadOnlyList<string> commodityNames)
    {
        var folded = ScreenInsight.Fold(read);
        var exact = commodityNames.Where(name => ScreenInsight.Fold(name) == folded).Distinct().ToList();
        if (exact.Count == 1) return exact[0];

        // "ALUMINUM (ORE)" against "Aluminum (Ore)" is exact. Two letters of
        // slack take a dropped or doubled stroke when only one name is that
        // close; "ÄLUMNUX (ORE)" is three off and stays as read, named by nobody.
        var close = commodityNames
            .Where(name => Math.Abs(ScreenInsight.Fold(name).Length - folded.Length) <= 1 && Within(ScreenInsight.Fold(name), folded, 2))
            .Distinct()
            .ToList();
        return close.Count == 1 ? close[0] : null;
    }

    /// <summary>Whether two strings are within a given edit distance - the cheap kind, substitutions and one length step.</summary>
    private static bool Within(string a, string b, int slack)
    {
        if (Math.Abs(a.Length - b.Length) > 1) return false;
        var mismatches = 0;
        for (int i = 0, j = 0; i < a.Length && j < b.Length; i++, j++)
        {
            if (a[i] == b[j]) continue;
            mismatches++;
            if (mismatches > slack) return false;
            if (a.Length > b.Length) j--;
            else if (b.Length > a.Length) i--;
        }
        return mismatches + Math.Abs(a.Length - b.Length) <= slack;
    }

    private static string Plain(string text) => ScreenInsight.Plain(text);
}
