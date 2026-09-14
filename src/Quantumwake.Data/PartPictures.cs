using Quantumwake.Core;
using Quantumwake.Core.GameData;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace Quantumwake.Data;

/// <summary>A picture as bytes, with what it is.</summary>
public sealed record PartPicture(byte[] Bytes, string ContentType);

/// <summary>
/// Pictures of ship components and their makers' logos, fetched from the
/// Star Citizen Wiki once and kept beside the community digest.
/// </summary>
/// <remarks>
/// <para>
/// The game files carry no picture of a part - every displayIcon in the dump
/// is a ship or an FPS preset - and UEX's item records have an empty
/// screenshot field for all of them, so the wiki's in-game cutouts are the
/// only face a cooler has. The wiki has one for about half the bench parts
/// (316 of 633 names when this was written: most guns and power plants,
/// almost no radars or missile racks); the rest show their maker.
/// </para>
/// <para>
/// The maker's logo has the same problem one level up: the Fankit ships
/// logos for the fifteen hull makers and none of the forty-five component
/// makers - Behring, Juno Starwerk, Klaus &amp; Werner. The wiki's
/// manufacturer pages lead with the logo for 57 of the 59, so those come
/// from the same place, by the maker's name.
/// </para>
/// <para>
/// One page-image lookup per subject, by its display name, then one download
/// of a thumbnail. Both go to the wiki only after the community dataset was
/// switched on, which is the app's consent to talk to the network, and carry
/// the name and nothing else. A miss is remembered for a month so a bench
/// full of radars does not ask the wiki the same question every time it
/// opens.
/// </para>
/// </remarks>
public sealed class PartPictures
{
    public const string WikiApi = "https://starcitizen.tools/api.php";

    /// <summary>The size asked for: the bench shows a picture at 56 px, twice that reads sharp on a scaled display.</summary>
    private const int Width = 240;
    private static readonly TimeSpan MissLifetime = TimeSpan.FromDays(30);

    private readonly string _parts;
    private readonly string _makers;
    private readonly string _missesPath;
    private readonly Lock _gate = new();
    private Dictionary<string, DateTimeOffset> _misses = new(StringComparer.Ordinal);

    /// <summary>Two at a time toward the wiki: a bench panel asks for fifteen at once.</summary>
    private readonly SemaphoreSlim _slots = new(2);

    /// <summary>The same subject asked for twice while its first fetch is in flight shares that fetch.</summary>
    private readonly ConcurrentDictionary<string, Task<PartPicture?>> _inFlight = new(StringComparer.Ordinal);

    public PartPictures(string? directory = null)
    {
        var root = directory ?? AppPaths.In("community");
        _parts = Path.Combine(root, "part-pictures");
        _makers = Path.Combine(root, "maker-marks");
        _missesPath = Path.Combine(_parts, "misses.json");
        LoadMisses();
    }

    /// <summary>
    /// The picture for a part: from the cache when it has one, from the wiki
    /// once when it does not, null when the wiki has none either.
    /// </summary>
    public Task<PartPicture?> GetAsync(HttpClient http, PartStats part, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(part.Uuid) || string.IsNullOrWhiteSpace(part.Name))
            return Task.FromResult<PartPicture?>(null);

        return Get(http, part.Uuid, Titles(part.Name), _parts, logo: false, token);
    }

    /// <summary>
    /// A maker's logo by its code and name, for the makers the Fankit does not
    /// cover. Photographs are refused: a page whose lead image is a JPEG is
    /// showing a fleet or a roadmap card, not a mark.
    /// </summary>
    public Task<PartPicture?> GetMakerAsync(HttpClient http, string code, string name, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return Task.FromResult<PartPicture?>(null);

        return Get(http, code.Trim().ToUpperInvariant(), [name.Trim()], _makers, logo: true, token);
    }

    private Task<PartPicture?> Get(HttpClient http, string key, IReadOnlyList<string> titles, string directory, bool logo, CancellationToken token)
    {
        var cached = FromCache(directory, key);
        if (cached is not null)
            return Task.FromResult<PartPicture?>(cached);

        var missKey = logo ? "maker:" + key : key;
        lock (_gate)
        {
            if (_misses.TryGetValue(missKey, out var when) && DateTimeOffset.UtcNow - when < MissLifetime)
                return Task.FromResult<PartPicture?>(null);
        }

        return _inFlight.GetOrAdd(missKey, k => FetchAsync(http, key, titles, directory, logo, missKey, token)
            .ContinueWith(t =>
            {
                _inFlight.TryRemove(k, out _);
                return t.IsCompletedSuccessfully ? t.Result : null;
            }, TaskScheduler.Default));
    }

    /// <summary>The names the wiki may file a part under, most specific first.</summary>
    /// <remarks>
    /// The dump says <c>5MA 'Chimalli'</c>; the wiki's page is <c>Chimalli</c>.
    /// The quoted model name alone is the second guess, never the first - a
    /// bare word can be a system or a station as easily as a shield.
    /// </remarks>
    public static IReadOnlyList<string> Titles(string name)
    {
        var titles = new List<string> { name.Trim() };
        var quote = name.IndexOf('\'');
        var close = quote >= 0 ? name.IndexOf('\'', quote + 1) : -1;
        if (close > quote + 1)
            titles.Add(name[(quote + 1)..close]);
        return titles;
    }

    private async Task<PartPicture?> FetchAsync(HttpClient http, string key, IReadOnlyList<string> titles, string directory, bool logo, string missKey, CancellationToken token)
    {
        await _slots.WaitAsync(token);
        try
        {
            var source = await LookUpAsync(http, titles, logo, token);
            if (source is null)
            {
                RecordMiss(missKey);
                return null;
            }

            using var response = await http.GetAsync(source, token);
            var type = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            if (!response.IsSuccessStatusCode || !type.StartsWith("image/", StringComparison.Ordinal))
            {
                RecordMiss(missKey);
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(token);
            if (logo && type == "image/svg+xml")
                bytes = System.Text.Encoding.UTF8.GetBytes(LightenSvg(System.Text.Encoding.UTF8.GetString(bytes)));
            Store(directory, key, type, bytes);
            return new PartPicture(bytes, type);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException or IOException)
        {
            // Network trouble is not "the wiki has no picture": leave the
            // subject askable again next time rather than blanking it for a month.
            return null;
        }
        finally
        {
            _slots.Release();
        }
    }

    /// <summary>Asks the wiki which picture leads the page, trying each title it might be under.</summary>
    /// <remarks>
    /// The thumbnail is what is fetched even for a logo. For a mark drawn as
    /// SVG the wiki hands the SVG itself back (JUST, BEHR, ACOM did), which is
    /// fine where it is used: an img element renders SVG with scripts and
    /// external loads disabled, so nothing in the file can run.
    /// </remarks>
    private static async Task<string?> LookUpAsync(HttpClient http, IReadOnlyList<string> titles, bool logo, CancellationToken token)
    {
        var url = $"{WikiApi}?action=query&prop=pageimages&piprop=thumbnail|original&pithumbsize={Width}&redirects=1&format=json&titles={WebUtility.UrlEncode(string.Join('|', titles))}";

        using var doc = JsonDocument.Parse(await http.GetStringAsync(url, token));
        if (!doc.RootElement.TryGetProperty("query", out var query) || !query.TryGetProperty("pages", out var pages))
            return null;

        // The wiki answers in its own order; take the first title that has a
        // picture, in the order the titles were guessed.
        var found = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var asked = Asked(query);
        foreach (var page in pages.EnumerateObject())
        {
            if (!page.Value.TryGetProperty("thumbnail", out var thumb) || !thumb.TryGetProperty("source", out var src))
                continue;

            if (logo && page.Value.TryGetProperty("original", out var original)
                && original.TryGetProperty("source", out var file) && IsPhotograph(file.GetString()))
                continue;

            var title = page.Value.GetProperty("title").GetString() ?? "";
            found[asked.GetValueOrDefault(title, title)] = src.GetString() ?? "";
        }

        foreach (var title in titles)
            if (found.TryGetValue(title, out var source) && source.Length > 0)
                return source;

        return null;
    }

    /// <summary>
    /// A logo drawn for a white page, redrawn for a dark one: paths with no
    /// fill of their own, and paths filled near-black, come out pale.
    /// </summary>
    /// <remarks>
    /// The Jump Point marks the wiki holds - ACOM, Behring, WillsOp, most of
    /// Juno Starwerk - are black on transparent, which on the bench is a
    /// blank square. Coloured fills (Juno's red, Lightning Power's yellow)
    /// are the mark and stay; only the ink changes. The sheet colour is the
    /// app's text colour, so the mark reads as text does.
    /// </remarks>
    public static string LightenSvg(string svg)
    {
        const string ink = "#e6edf3";

        // The root inherits down to every path that never said a colour.
        var root = SvgRoot.Match(svg);
        if (root.Success && !root.Value.Contains(" fill=", StringComparison.OrdinalIgnoreCase))
            svg = svg[..root.Index] + root.Value[..^1] + $" fill=\"{ink}\">" + svg[(root.Index + root.Length)..];

        return DarkFill.Replace(svg, m =>
        {
            var colour = m.Groups["c"].Value;
            return IsDark(colour) ? m.Value.Replace(colour, ink) : m.Value;
        });
    }

    private static readonly System.Text.RegularExpressions.Regex SvgRoot =
        new(@"<svg\b[^>]*>", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>A fill as an attribute or in a style, hex or the word black.</summary>
    private static readonly System.Text.RegularExpressions.Regex DarkFill =
        new(@"fill\s*[:=]\s*""?(?<c>#[0-9a-fA-F]{3}(?:[0-9a-fA-F]{3})?|black)\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);

    private static bool IsDark(string colour)
    {
        if (colour.Equals("black", StringComparison.OrdinalIgnoreCase)) return true;
        var hex = colour[1..];
        if (hex.Length == 3) hex = string.Concat(hex.Select(ch => $"{ch}{ch}"));
        var r = Convert.ToInt32(hex[..2], 16);
        var g = Convert.ToInt32(hex[2..4], 16);
        var b = Convert.ToInt32(hex[4..], 16);
        // Ink is dark and grey. Juno's red (#79242f) is as dim as ink but is a
        // colour, and the colour is the mark; Tyler Design's mid-grey stays too.
        var chroma = Math.Max(r, Math.Max(g, b)) - Math.Min(r, Math.Min(g, b));
        return chroma < 48 && (0.299 * r + 0.587 * g + 0.114 * b) / 255 < 0.3;
    }

    /// <summary>A JPEG has no transparency, so a logo never is one; the ones met were a fleet photo and a roadmap card.</summary>
    private static bool IsPhotograph(string? source)
    {
        if (source is null) return false;
        var path = source.Split('?')[0];
        return path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Final page title back to the title that was asked for, through normalisation and redirects.</summary>
    private static Dictionary<string, string> Asked(JsonElement query)
    {
        var back = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var list in new[] { "normalized", "redirects" })
        {
            if (!query.TryGetProperty(list, out var entries) || entries.ValueKind != JsonValueKind.Array) continue;
            foreach (var entry in entries.EnumerateArray())
            {
                var from = entry.GetProperty("from").GetString() ?? "";
                var to = entry.GetProperty("to").GetString() ?? "";
                back[to] = back.GetValueOrDefault(from, from);
            }
        }
        return back;
    }

    private static PartPicture? FromCache(string directory, string key)
    {
        foreach (var (ext, type) in Extensions)
        {
            var path = Path.Combine(directory, key + ext);
            if (File.Exists(path))
                return new PartPicture(File.ReadAllBytes(path), type);
        }
        return null;
    }

    private static void Store(string directory, string key, string type, byte[] bytes)
    {
        var ext = Extensions.FirstOrDefault(e => e.Type == type).Ext ?? ".jpg";
        Directory.CreateDirectory(directory);
        File.WriteAllBytes(Path.Combine(directory, key + ext), bytes);
    }

    private static readonly (string Ext, string Type)[] Extensions =
    [
        (".webp", "image/webp"), (".png", "image/png"), (".jpg", "image/jpeg"), (".gif", "image/gif"), (".svg", "image/svg+xml"),
    ];

    private void RecordMiss(string key)
    {
        lock (_gate)
        {
            _misses[key] = DateTimeOffset.UtcNow;
            try
            {
                Directory.CreateDirectory(_parts);
                File.WriteAllText(_missesPath, JsonSerializer.Serialize(_misses));
            }
            catch (IOException)
            {
                // A miss that is not remembered is asked again next time; nothing worse.
            }
        }
    }

    private void LoadMisses()
    {
        try
        {
            if (File.Exists(_missesPath))
                _misses = JsonSerializer.Deserialize<Dictionary<string, DateTimeOffset>>(File.ReadAllText(_missesPath)) ?? new();
        }
        catch (Exception e) when (e is IOException or JsonException)
        {
            _misses = new();
        }
    }
}
