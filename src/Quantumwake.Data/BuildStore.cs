using Quantumwake.Core;
using System.Text.Json;

namespace Quantumwake.Data;

/// <summary>
/// A fit the pilot is planning for one of their ships: which port gets which
/// part, under a name.
/// </summary>
/// <param name="ShipClass">The dump's class - MISC_Starlancer_Max - which is what the sheet is keyed by.</param>
/// <param name="Swaps">Port id to the class fitted there; null empties the port. Ports not named stay stock.</param>
public sealed record ShipBuild(
    string Id,
    string ShipClass,
    string Name,
    IReadOnlyDictionary<string, string?> Swaps,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? Note = null) : IStamped<ShipBuild>
{
    public string StampId => Id;
    public ShipBuild Bare() => this with { UpdatedAt = default };
    public ShipBuild Stamped(DateTimeOffset at) => this with { UpdatedAt = at };
    public DateTimeOffset ChangedAt => UpdatedAt;
}

/// <summary>The pilot's saved builds, kept beside the other authored files.</summary>
/// <remarks>
/// Authored, like jobs and map notes: its own file, never touched by a rescan,
/// carried by a backup. A build holds only the swaps - the sheet is recomputed
/// from the reference every time it is opened, so a dataset refresh that
/// changes a part's figures changes the build's figures with it, which is what
/// a plan against the game's numbers should do.
/// </remarks>
public sealed class BuildStore
{
    private readonly string _path;
    private readonly Lock _gate = new();

    /// <summary>Marks what actually changed, so no mutator has to remember to.</summary>
    private readonly ChangeStamp<ShipBuild> _stamp = new(r => JsonSerializer.Serialize(r));
    private List<ShipBuild> _builds = [];

    public BuildStore(string? directory = null)
    {
        _path = Path.Combine(directory ?? AppPaths.Root, "builds.json");
        Load();
    }

    public IReadOnlyList<ShipBuild> All()
    {
        lock (_gate) return [.. _builds];
    }

    public IReadOnlyList<ShipBuild> For(string shipClass)
    {
        lock (_gate) return [.. _builds.Where(b => b.ShipClass.Equals(shipClass, StringComparison.OrdinalIgnoreCase))];
    }

    public ShipBuild? Get(string id)
    {
        lock (_gate) return _builds.FirstOrDefault(b => b.Id == id);
    }

    public ShipBuild? Add(string? shipClass, string? name, IReadOnlyDictionary<string, string?>? swaps, string? note = null)
    {
        var cls = Sanitise.Clean(shipClass, string.Empty, 80);
        if (cls.Length == 0)
            return null;

        var now = DateTimeOffset.UtcNow;
        var build = new ShipBuild(
            Guid.NewGuid().ToString("N")[..8],
            cls,
            Sanitise.Clean(name, "Untitled build", 80),
            CleanSwaps(swaps),
            now,
            now,
            Sanitise.CleanOptional(note));

        lock (_gate)
        {
            _builds.Insert(0, build);
            Save();
            return build;
        }
    }

    /// <summary>Renames a build, replaces its swaps, or both. Null leaves a field alone.</summary>
    public ShipBuild? Update(string id, string? name, IReadOnlyDictionary<string, string?>? swaps, string? note)
    {
        lock (_gate)
        {
            var index = _builds.FindIndex(b => b.Id == id);
            if (index < 0) return null;

            var build = _builds[index];
            if (name is not null) build = build with { Name = Sanitise.Clean(name, build.Name, 80) };
            if (swaps is not null) build = build with { Swaps = CleanSwaps(swaps) };
            if (note is not null) build = build with { Note = Sanitise.CleanOptional(note) };

            _builds[index] = build;
            Save();
            return build;
        }
    }

    public bool Remove(string id)
    {
        lock (_gate)
        {
            if (_builds.RemoveAll(b => b.Id == id) == 0) return false;
            Save();
            return true;
        }
    }

    /// <summary>
    /// Puts a record back exactly as given, replacing any with the same id.
    /// </summary>
    /// <remarks>
    /// For restoring a backup, and nothing else - see <see cref="MapNoteStore.Put"/>
    /// for why a restore must reproduce rather than author.
    /// </remarks>
    public void Put(ShipBuild build)
    {
        lock (_gate)
        {
            var index = _builds.FindIndex(b => b.Id == build.Id);

            if (index >= 0) _builds[index] = build;
            else _builds.Add(build);

            _stamp.Adopt(build);
            Save();
        }
    }

    /// <summary>
    /// Port ids and classes as the dump spells them, clipped, with blanks read
    /// as "empty this port" rather than as a class called nothing.
    /// </summary>
    private static Dictionary<string, string?> CleanSwaps(IReadOnlyDictionary<string, string?>? swaps)
    {
        var clean = new Dictionary<string, string?>(StringComparer.Ordinal);
        if (swaps is null) return clean;

        foreach (var (port, cls) in swaps.Take(200))
        {
            var key = Sanitise.Clean(port, string.Empty, 80);
            if (key.Length == 0) continue;
            clean[key] = Sanitise.CleanOptional(cls, 120);
        }

        return clean;
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_path))
                _builds = JsonSerializer.Deserialize<List<ShipBuild>>(File.ReadAllText(_path)) ?? [];
        }
        catch (Exception e) when (e is IOException or JsonException)
        {
            // Plans, not the ship: a damaged file must leave the garage able to
            // draw a sheet rather than refusing to load.
            _builds = [];
        }

        _stamp.Loaded(_builds);
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        _stamp.Apply(_builds, DateTimeOffset.UtcNow);
        File.WriteAllText(_path, JsonSerializer.Serialize(_builds));
    }
}
