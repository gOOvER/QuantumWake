using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Quantumwake.Core;

namespace Quantumwake.Data;

/// <summary>One picture of a stick the page can draw bindings on.</summary>
/// <param name="Key">How it is referred to: <c>repo:Thrustmaster/Thrustmaster Warthog - Joystick.svg</c> or <c>local:my-stick.svg</c>.</param>
/// <param name="Name">The file name without its extension - "Thrustmaster Warthog - Joystick".</param>
/// <param name="Maker">The folder it sits in - "Thrustmaster", "Virpil" - or "yours" for a local file.</param>
public sealed record JoystickTemplate(string Key, string Name, string Maker);

/// <summary>
/// Pictures of sticks from Joystick Diagrams' template library, fetched on
/// request and kept, with the pilot's own on top.
/// </summary>
/// <remarks>
/// <para>
/// The library (github.com/Rexeh/joystick-diagrams, GPL-2.0) holds a
/// draw.io SVG per device - 48 in the repository on 2026-09-17, Warthog to
/// WinWing - each a photograph with <c>Button_N</c>, <c>POV_1_U</c> and
/// <c>AXIS_X</c> text placed over it. The photographs' provenance is not
/// stated, so nothing is bundled: the index and each picture are fetched
/// from the repository only once the pilot turns the feed on, kept under
/// <c>controls\templates\</c>, and the page credits the project. See
/// <c>docs/joystick-mapping.md</c>.
/// </para>
/// <para>
/// A device is matched to a template by product GUID: a small table for the
/// sticks whose ids this install and the game's own layouts name, and the
/// pilot's choice over it, kept in <c>controls\templates.json</c>. A folder
/// of the pilot's own SVGs - Joystick Diagrams' community templates from its
/// Discord, or one drawn over a photo - is listed beside the library's.
/// </para>
/// </remarks>
public sealed class JoystickTemplates
{
    public const string Repository = "Rexeh/joystick-diagrams";
    public const string TreeUrl = "https://api.github.com/repos/Rexeh/joystick-diagrams/git/trees/master?recursive=1";
    public const string RawUrl = "https://raw.githubusercontent.com/Rexeh/joystick-diagrams/master/";
    public const string ProjectUrl = "https://github.com/Rexeh/joystick-diagrams";

    /// <summary>
    /// Product GUID to the library's file, for the devices whose ids are
    /// known here: this install's Thrustmasters and the ones the game's own
    /// layouts name. Anything else is the pilot's to pick.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> Known = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["{0402044F-0000-0000-0000-504944564944}"] = "Thrustmaster/Thrustmaster Warthog - Joystick.svg",
        ["{0404044F-0000-0000-0000-504944564944}"] = "Thrustmaster/Thrustmaster Warthog - Throttle.svg",
        ["{B10A044F-0000-0000-0000-504944564944}"] = "Thrustmaster/T.16000M Joystick Right Handed.svg",
        ["{B687044F-0000-0000-0000-504944564944}"] = "Thrustmaster/T.16000M Throttle.svg",
        ["{0200231D-0000-0000-0000-504944564944}"] = "VKB Sim/VKB-Sim Gladiator NXT R.svg",
        ["{0201231D-0000-0000-0000-504944564944}"] = "VKB Sim/VKB-Sim Gladiator NXT L.svg",
        ["{025506A3-0000-0000-0000-504944564944}"] = "Saitek - Logitech/X52_H.O.T.A.S..svg",
        ["{076206A3-0000-0000-0000-504944564944}"] = "Saitek - Logitech/X52_H.O.T.A.S..svg",
        ["{22210738-0000-0000-0000-504944564944}"] = "Saitek - Logitech/X56 H.O.T.A.S. Stick.svg",
        ["{A2210738-0000-0000-0000-504944564944}"] = "Saitek - Logitech/X56 H.O.T.A.S. Throttle.svg",
    };

    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly string _directory;
    private readonly string _statePath;
    private readonly string _indexPath;
    private readonly Lock _gate = new();
    private State _state;

    public JoystickTemplates(string? directory = null)
    {
        _directory = directory ?? AppPaths.In("controls", "templates");
        _statePath = Path.Combine(_directory, "..", "templates.json");
        _indexPath = Path.Combine(_directory, "index.json");
        _state = Load();
    }

    /// <summary>Whether the library may be fetched; off until the pilot says.</summary>
    public bool Enabled => _state.Enabled;

    /// <summary>When the index was last fetched, or null.</summary>
    public DateTimeOffset? FetchedAt => _state.FetchedAt;

    /// <summary>The pilot's own folder of SVGs, or null.</summary>
    public string? Folder => _state.Folder;

    /// <summary>The pilot's choices, product GUID to template key.</summary>
    public IReadOnlyDictionary<string, string> Assignments => _state.Assignments;

    /// <summary>Turns the feed on and fetches the index: every SVG under <c>templates/</c> in the repository.</summary>
    public async Task<int> EnableAsync(HttpClient http, CancellationToken cancel = default)
    {
        var tree = await http.GetFromJsonAsync<Tree>(TreeUrl, cancel)
            ?? throw new InvalidDataException("The template index came back empty.");
        var files = tree.Entries
            .Where(e => e.Type == "blob" && e.Path.StartsWith("templates/", StringComparison.Ordinal) && e.Path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            .Select(e => e.Path["templates/".Length..])
            .Where(p => !p.StartsWith("Starter", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (files.Count == 0) throw new InvalidDataException("The template index lists no SVG.");

        Directory.CreateDirectory(_directory);
        File.WriteAllText(_indexPath, JsonSerializer.Serialize(files, Json));
        lock (_gate)
        {
            _state = _state with { Enabled = true, FetchedAt = DateTimeOffset.UtcNow };
            Save();
        }
        return files.Count;
    }

    /// <summary>Turns the feed off. What was fetched stays on disk and is shown as long as it is there.</summary>
    public void Disable()
    {
        lock (_gate)
        {
            _state = _state with { Enabled = false };
            Save();
        }
    }

    /// <summary>Points at a folder of the pilot's own SVGs, or clears it.</summary>
    public void SetFolder(string? folder)
    {
        lock (_gate)
        {
            _state = _state with { Folder = string.IsNullOrWhiteSpace(folder) ? null : folder.Trim() };
            Save();
        }
    }

    /// <summary>Records which template a device wears; an empty key clears the choice back to the table's.</summary>
    public void Assign(string guid, string? key)
    {
        lock (_gate)
        {
            var next = new Dictionary<string, string>(_state.Assignments, StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(key)) next.Remove(guid); else next[guid] = key;
            _state = _state with { Assignments = next };
            Save();
        }
    }

    /// <summary>Every template on offer: the library's index if fetched, then the pilot's folder.</summary>
    public IReadOnlyList<JoystickTemplate> Available()
    {
        var list = new List<JoystickTemplate>();
        foreach (var path in Index())
        {
            var slash = path.IndexOf('/');
            var maker = slash > 0 ? path[..slash] : "";
            list.Add(new JoystickTemplate("repo:" + path, Path.GetFileNameWithoutExtension(path), maker));
        }
        if (_state.Folder is { } folder && Directory.Exists(folder))
        {
            foreach (var file in Directory.EnumerateFiles(folder, "*.svg").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                list.Add(new JoystickTemplate("local:" + Path.GetFileName(file), Path.GetFileNameWithoutExtension(file), "yours"));
        }
        return list;
    }

    /// <summary>
    /// The template a device should wear: the pilot's choice, else the
    /// table's if the library lists it, else null. A choice that no longer
    /// exists - a local file removed - falls back the same way.
    /// </summary>
    public JoystickTemplate? For(string? guid)
    {
        if (guid is null) return null;
        var available = Available();
        if (_state.Assignments.TryGetValue(guid, out var chosen) && available.FirstOrDefault(t => t.Key == chosen) is { } picked)
            return picked;
        if (Known.TryGetValue(guid, out var known) && available.FirstOrDefault(t => t.Key == "repo:" + known) is { } table)
            return table;
        return null;
    }

    /// <summary>
    /// The SVG for a key: from the folder for a local one, from the kept copy
    /// or the repository for the library's. Null when it cannot be had - the
    /// feed is off and nothing is kept, or the file is gone.
    /// </summary>
    public async Task<byte[]?> ReadAsync(string key, HttpClient? http, CancellationToken cancel = default)
    {
        if (key.StartsWith("local:", StringComparison.Ordinal))
        {
            var name = key["local:".Length..];
            if (name.Contains('/') || name.Contains('\\') || name.Contains("..") || _state.Folder is not { } folder) return null;
            var path = Path.Combine(folder, name);
            return File.Exists(path) ? await File.ReadAllBytesAsync(path, cancel) : null;
        }
        if (!key.StartsWith("repo:", StringComparison.Ordinal)) return null;

        var relative = key["repo:".Length..];
        if (relative.Contains("..")) return null;
        var kept = Path.Combine(_directory, "svg", relative.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(kept)) return await File.ReadAllBytesAsync(kept, cancel);
        if (!_state.Enabled || http is null) return null;

        var bytes = await http.GetByteArrayAsync(RawUrl + "templates/" + Uri.EscapeDataString(relative).Replace("%2F", "/"), cancel);
        Directory.CreateDirectory(Path.GetDirectoryName(kept)!);
        await File.WriteAllBytesAsync(kept, bytes, cancel);
        return bytes;
    }

    private IReadOnlyList<string> Index()
    {
        try
        {
            return File.Exists(_indexPath)
                ? JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_indexPath)) ?? []
                : [];
        }
        catch (Exception e) when (e is IOException or JsonException)
        {
            return [];
        }
    }

    private State Load()
    {
        try
        {
            if (File.Exists(_statePath))
                return JsonSerializer.Deserialize<State>(File.ReadAllText(_statePath), Json) ?? new State();
        }
        catch (Exception e) when (e is IOException or JsonException) { /* a state that will not read is the default */ }
        return new State();
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_statePath)!);
        File.WriteAllText(_statePath, JsonSerializer.Serialize(_state, Json));
    }

    private sealed record State(
        bool Enabled = false,
        DateTimeOffset? FetchedAt = null,
        string? Folder = null,
        Dictionary<string, string>? Assignments = null)
    {
        public Dictionary<string, string> Assignments { get; init; } = Assignments ?? new(StringComparer.OrdinalIgnoreCase);
    }

    private sealed record Tree([property: JsonPropertyName("tree")] List<TreeEntry> Entries);
    private sealed record TreeEntry([property: JsonPropertyName("path")] string Path, [property: JsonPropertyName("type")] string Type);
}
