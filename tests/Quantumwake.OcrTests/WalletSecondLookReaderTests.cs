using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Quantumwake.Data;
using Quantumwake.Ocr;
using Xunit.Abstractions;

namespace Quantumwake.OcrTests;

/// <summary>
/// The patch read: that a crop of a file goes through scaled and sheared, and
/// that the boxes come back in the frame's own pixels.
/// </summary>
/// <remarks>
/// As with <see cref="WindowsScreenReaderTests"/>, what is defended is the
/// wrapping and the geometry, not the recognition. The fixture is drawn large
/// and clean so that a marginal engine cannot fail it for its own reasons;
/// whether the treatment rescues the real balance is a measurement, and it is
/// the one below that stands down without frames.
/// </remarks>
public class WalletSecondLookReaderTests : IDisposable
{
    private static readonly WindowsScreenReader Reader = new();

    private readonly List<string> _written = [];

    [Fact]
    public async Task A_machine_with_no_engine_returns_nothing_from_the_second_look_too()
    {
        if (Reader.Available) return;

        Assert.Empty(await Reader.ReadAsync(Png(0), new ScreenPatch(0, 0, 100, 100), new ScreenTreatment(2, 0.2)));
    }

    /// <summary>
    /// The panel is drawn at a known place in a 1200x400 frame, slanted by
    /// the angle the treatment undoes; the patch asked for is the region
    /// around it. A line has to come back with its box near where it was
    /// drawn, in frame pixels - not in the crop's, and not multiplied by the
    /// scale - or the wallet's "left of the bar, near its row" test would be
    /// measuring against the wrong frame.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The box measured is the handle's, not the figure's. Whether this engine
    /// returns a drawn line of digits depends on their height in a way that
    /// has nothing to do with the plumbing - the same fixture read at 36 px
    /// and not at 54, at 50 and not at 44 - while the word beneath came back
    /// on every run at every size, which is also how the real panel behaves.
    /// Whether the treatment recovers the real figure is the measurement
    /// below, on real frames.
    /// </para>
    /// <para>
    /// Skewing the drawing by the shear's own angle also checks the shear
    /// runs the right way: the wrong sign would double the slant and put the
    /// box somewhere else.
    /// </para>
    /// </remarks>
    [Theory]
    [InlineData(1, 0.0)]
    [InlineData(2, 0.2)]
    [InlineData(1.5, 0.25)]
    public async Task The_patch_read_maps_its_boxes_back_into_the_frame(double scale, double shear)
    {
        if (!Reader.Available) return;

        var lines = await Reader.ReadAsync(Png(shear),
            new ScreenPatch(Left: 300, Top: 100, Width: 500, Height: 150),
            new ScreenTreatment(scale, shear));

        var handle = lines.FirstOrDefault(line => line.Text.Contains("NEKRON"));

        Assert.NotNull(handle);

        // Drawn with its top-left at (360, 200), then slid right by the skew
        // about the frame's bottom edge. The engine's box starts at the cap
        // height rather than the em box, hence the slack below the top.
        var drawnLeft = 360 + shear * (400 - 200);
        Assert.InRange(handle.Left, drawnLeft - 15, drawnLeft + 25);
        Assert.InRange(handle.Top, 195, 225);
        Assert.InRange(handle.Height, 12, 36);
    }

    [Fact]
    public async Task A_patch_off_the_edge_is_clipped_rather_than_thrown()
    {
        if (!Reader.Available) return;

        var lines = await Reader.ReadAsync(Png(0.2),
            new ScreenPatch(Left: -50, Top: -50, Width: 900, Height: 300),
            new ScreenTreatment(2, 0.2));

        Assert.Contains(lines, line => line.Text.Contains("NEKRON"));
    }

    [Fact]
    public async Task A_patch_with_nothing_in_it_reads_as_nothing()
    {
        if (!Reader.Available) return;

        var lines = await Reader.ReadAsync(Png(0),
            new ScreenPatch(Left: 1100, Top: 300, Width: 1, Height: 1),
            new ScreenTreatment(2, 0.2));

        Assert.Empty(lines);
    }

    /// <summary>
    /// White bold digits on black with the handle beneath, the shape of the
    /// mobiGlas panel. <paramref name="slant"/> leans the whole drawing to
    /// the right by that much per pixel of height, about the frame's bottom
    /// edge, which is the slant a treatment with the same shear takes back
    /// out.
    /// </summary>
    /// <remarks>
    /// The handle is not decoration. The engine returns nothing at all for a
    /// line of digits standing alone on a frame - measured on crops of the
    /// real panel that held only the figure, at three sizes and with three
    /// treatments - and reads the same digits once there is a word beneath
    /// them. On the real panel there always is one.
    /// </remarks>
    private string Png(double slant)
    {
        var visual = new DrawingVisual();

        using (var draw = visual.RenderOpen())
        {
            draw.DrawRectangle(Brushes.Black, null, new Rect(0, 0, 1200, 400));

            // WPF's skew is x' = x + tan(angle) * (y - centre), and y runs down
            // the frame, so a right lean about the bottom edge is a negative
            // angle centred on y = 400.
            draw.PushTransform(new SkewTransform(-Math.Atan(slant) * 180 / Math.PI, 0, 0, 400));
            draw.DrawText(Text("3,958,160", 36), new Point(360, 140));
            draw.DrawText(Text("NEKRON", 24), new Point(360, 200));
            draw.Pop();
        }

        var target = new RenderTargetBitmap(1200, 400, 96, 96, PixelFormats.Pbgra32);
        target.Render(visual);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(target));

        var path = Path.Combine(Path.GetTempPath(), $"qw-ocr-{Guid.NewGuid():N}.png");
        using (var file = File.Create(path)) encoder.Save(file);

        _written.Add(path);
        return path;
    }

    private static FormattedText Text(string text, double size) =>
        new(text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
            size,
            Brushes.White,
            96);

    public void Dispose()
    {
        foreach (var path in _written)
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException)
            {
                // A temp file left behind is not worth failing a test over.
            }
        }

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Both looks at the wallet, run against real frames.
/// </summary>
/// <remarks>
/// Stands down unless <c>QUANTUMWAKE_WALLET_FRAMES</c> names image files,
/// separated by semicolons. For each it prints what the whole-frame read made
/// of the balance, then what every rung of the second look made of it, then
/// what the app would show. The rungs are all printed rather than stopping at
/// agreement, because what this is for is seeing how close to the edge a
/// frame is - two rungs reading is a pass; two rungs reading and four not is
/// a pass with a note.
/// <para>
/// On the four frames it was built against (2026-09-12 00:47, 22:32:26,
/// 22:32:27 and 2026-09-13 01:43) the whole read returned the figure on two,
/// and the second look settled the other two with no wrong digit.
/// </para>
/// </remarks>
public class WalletFrameMeasurement(ITestOutputHelper output)
{
    private static string[] Frames =>
        (Environment.GetEnvironmentVariable("QUANTUMWAKE_WALLET_FRAMES") ?? "")
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    [Fact]
    public async Task The_balance_on_each_frame_by_both_looks()
    {
        var frames = Frames;

        if (frames.Length == 0)
        {
            output.WriteLine(
                "QUANTUMWAKE_WALLET_FRAMES is not set, so there is nothing to measure. "
                + "Set it to one or more image files separated by semicolons.");
            return;
        }

        var reader = new WindowsScreenReader();

        if (!reader.Available)
        {
            output.WriteLine("no OCR engine on this machine");
            return;
        }

        foreach (var frame in frames)
        {
            var lines = await reader.ReadAsync(frame);
            var first = ScreenFrames.Read(lines, [], []).Wallet;

            output.WriteLine($"{Path.GetFileName(frame)}");
            output.WriteLine($"  first look : {(first is null ? "no bar" : first.Balance?.ToString("N0") ?? "no figure")}");

            if (ScreenFrames.WalletPanel(lines) is not { } panel)
            {
                output.WriteLine("  second look: not taken");
                continue;
            }

            output.WriteLine($"  panel      : {panel.Left:0},{panel.Top:0} {panel.Width:0}x{panel.Height:0}");

            foreach (var treatment in WalletSecondLook.Treatments)
            {
                var again = await reader.ReadAsync(frame, panel, treatment);
                var figures = again
                    .Select(line => (line, value: ScreenFrames.Read([.. lines, line], [], []).Wallet?.Balance))
                    .Where(pair => pair.value is not null)
                    .Select(pair => $"{pair.value:N0} at {pair.line.Left:0},{pair.line.Top:0}")
                    .ToList();

                output.WriteLine(
                    $"  x{treatment.Scale} shear {treatment.Shear:0.00}: "
                    + (figures.Count == 0 ? "-" : string.Join("; ", figures)));
            }

            var settled = await WalletSecondLook.TakeAsync(lines,
                (patch, treatment, token) => reader.ReadAsync(frame, patch, treatment, token));
            var wallet = ScreenFrames.Read(settled, [], []).Wallet!;

            output.WriteLine($"  the app    : {wallet.Balance?.ToString("N0") ?? wallet.Trouble}");
        }
    }
}
