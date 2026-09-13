namespace Quantumwake.Data;

/// <summary>
/// Reads the mobiGlas balance again when the whole frame read and it did not.
/// </summary>
/// <remarks>
/// <para>
/// The balance is bold italic, and the in-box engine's detector is on a knife
/// edge with it: of four frames measured on this install it returned the
/// figure on two and dropped the line on two, and two of those frames were
/// taken a second apart in the same scene, one read and one not. Nothing was
/// misread - the figure either came back right or not at all. The name
/// beneath it, upright, read on all four.
/// </para>
/// <para>
/// What moves it is standing the glyphs up. Cropping the panel and scaling it
/// alone recovered one of the two misses; shearing the crop by 0.15-0.25 -
/// nine to fourteen degrees - at twice the size read all four, and across a
/// hundred and twelve such reads not one digit was wrong. Binarising the crop
/// also recovers the line, and was rejected: on one frame it read 3,958,160
/// as 4,958,160. A cash balance a pilot will plan against cannot come from a
/// treatment that invents digits.
/// </para>
/// <para>
/// Bigger is not better. The engine drops a line of digits once it stands
/// taller than about 55 px - measured on a drawn fixture, which read at 45 px
/// and not at 60 - and the balance is 25 px on a 3440-wide frame, so x2 is
/// the most that is safe and x3 lost one of the four. The rungs stay at x1.5
/// to x2.5.
/// </para>
/// <para>
/// So the rungs below are all shear and scale, no thresholding, and a figure
/// counts only when two of them agree on it. Two because a single read from a
/// treatment that had never misread in the lab is still a single read, and
/// agreement between two different geometries is cheap - the patch is a few
/// hundred pixels across and reads in a few milliseconds. The first two rungs
/// read all four measured frames on their own, so the usual cost is two.
/// </para>
/// </remarks>
public static class WalletSecondLook
{
    /// <summary>
    /// The treatments tried, in order. The first four each read every measured
    /// frame; the last two are the next-best from the same sweep, there for
    /// the frame that is not like the four.
    /// </summary>
    public static readonly IReadOnlyList<ScreenTreatment> Treatments =
    [
        new(Scale: 2, Shear: 0.2),
        new(Scale: 2, Shear: 0.25),
        new(Scale: 1.5, Shear: 0.25),
        new(Scale: 2, Shear: 0.15),
        new(Scale: 2.5, Shear: 0.2),
        new(Scale: 1.5, Shear: 0.2),
    ];

    /// <summary>How many rungs must read the same digits before they are believed.</summary>
    public const int Agreement = 2;

    /// <summary>Reads one patch under one treatment; what the reader does.</summary>
    public delegate Task<IReadOnlyList<ScreenTextLine>> PatchReader(
        ScreenPatch patch, ScreenTreatment treatment, CancellationToken token);

    /// <summary>
    /// The frame's lines with the balance added when a second look found one,
    /// or the lines as they were.
    /// </summary>
    /// <remarks>
    /// The figure goes back in as a line in the frame's own coordinates, so
    /// that <see cref="ScreenFrames.Read"/> finds it exactly as it would have
    /// had the first read returned it - one place decides what the wallet is,
    /// and it is not this one. A reader that throws on the second look loses
    /// only the second look; the frame already read once and that stands.
    /// </remarks>
    public static async Task<IReadOnlyList<ScreenTextLine>> TakeAsync(
        IReadOnlyList<ScreenTextLine> lines, PatchReader read, CancellationToken token = default)
    {
        if (ScreenFrames.WalletPanel(lines) is not { } panel)
            return lines;

        var votes = new Dictionary<long, ScreenTextLine>();
        var counts = new Dictionary<long, int>();

        foreach (var treatment in Treatments)
        {
            token.ThrowIfCancellationRequested();

            IReadOnlyList<ScreenTextLine> again;

            try
            {
                again = await read(panel, treatment, token);
            }
            catch (Exception) when (!token.IsCancellationRequested)
            {
                return lines;
            }

            // One vote per rung per figure: a rung that returned the same
            // digits twice has still only read them once.
            var seen = new HashSet<long>();

            foreach (var line in again)
            {
                if (ScreenFrames.Figure(line.Text.Trim()) is not { } value || !seen.Add(value)) continue;

                counts[value] = counts.GetValueOrDefault(value) + 1;
                votes.TryAdd(value, line);

                if (counts[value] >= Agreement)
                    return [.. lines, votes[value]];
            }
        }

        return lines;
    }
}
