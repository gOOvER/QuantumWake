using Quantumwake.Core;
using System.Text.Json;

namespace Quantumwake.Data;

/// <summary>
/// What the pilot wrote about a shard, and whether they starred it.
/// </summary>
/// <remarks>
/// <para>
/// Keyed by the full shard name rather than by an id of its own, because the
/// name is the identity: two machines that both played on
/// <c>pub_use1b_12545750_150</c> are talking about the same server, and a
/// restore should merge their notes on it rather than keep two.
/// </para>
/// <para>
/// A record with nothing written and no star is removed rather than kept empty,
/// so the file holds opinions and not a row per server ever visited.
/// </para>
/// </remarks>
public sealed record ShardNote(
    string Shard,
    string? Note,
    bool Favorite,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt) : IStamped<ShardNote>
{
    public string StampId => Shard;
    public ShardNote Bare() => this with { UpdatedAt = default };
    public ShardNote Stamped(DateTimeOffset at) => this with { UpdatedAt = at };
    public DateTimeOffset ChangedAt => UpdatedAt;

    /// <summary>True when there is nothing left worth keeping.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsEmpty => !Favorite && string.IsNullOrWhiteSpace(Note);
}

/// <summary>The pilot's own view of the servers they have been placed on.</summary>
/// <remarks>
/// Authored, like jobs and map notes: its own file, never touched by a rescan,
/// carried by a backup. The visits themselves come from the logs and are not
/// here - see <c>LogLibrary.Shards</c>.
/// </remarks>
public sealed class ShardNoteStore
{
    private readonly string _path;
    private readonly Lock _gate = new();

    /// <summary>Marks what actually changed, so no mutator has to remember to.</summary>
    private readonly ChangeStamp<ShardNote> _stamp = new(r => JsonSerializer.Serialize(r));
    private List<ShardNote> _notes = [];

    public ShardNoteStore(string? directory = null)
    {
        _path = Path.Combine(directory ?? AppPaths.Root, "shards.json");
        Load();
    }

    public IReadOnlyList<ShardNote> All()
    {
        lock (_gate) return [.. _notes];
    }

    public ShardNote? Get(string shard)
    {
        lock (_gate) return _notes.FirstOrDefault(n => n.Shard == shard);
    }

    /// <summary>
    /// Writes the note for a shard, leaving its star alone. Null or blank clears it.
    /// </summary>
    public ShardNote? SetNote(string shard, string? note) =>
        Change(shard, existing => existing with { Note = Sanitise.CleanOptional(note) });

    /// <summary>Stars or unstars a shard, leaving its note alone.</summary>
    public ShardNote? SetFavorite(string shard, bool favorite) =>
        Change(shard, existing => existing with { Favorite = favorite });

    /// <summary>
    /// Applies an edit, creating the record if this is the first thing written
    /// about the shard and dropping it if the edit emptied it. Returns what is
    /// now stored, or null when nothing is.
    /// </summary>
    private ShardNote? Change(string shard, Func<ShardNote, ShardNote> edit)
    {
        var key = Sanitise.Clean(shard, string.Empty, 80);
        if (key.Length == 0)
            return null;

        lock (_gate)
        {
            var now = DateTimeOffset.UtcNow;
            var index = _notes.FindIndex(n => n.Shard == key);
            var existing = index >= 0 ? _notes[index] : new ShardNote(key, null, false, now, now);

            var changed = edit(existing);

            if (changed.IsEmpty)
            {
                if (index >= 0)
                {
                    _notes.RemoveAt(index);
                    Save();
                }

                return null;
            }

            if (index >= 0) _notes[index] = changed;
            else _notes.Insert(0, changed);

            Save();
            return changed;
        }
    }

    public bool Remove(string shard)
    {
        lock (_gate)
        {
            if (_notes.RemoveAll(n => n.Shard == shard) == 0) return false;
            Save();
            return true;
        }
    }

    /// <summary>
    /// Puts a record back exactly as given, replacing any for the same shard.
    /// </summary>
    /// <remarks>
    /// For restoring a backup, and nothing else - see <see cref="MapNoteStore.Put"/>
    /// for why a restore must reproduce rather than author.
    /// </remarks>
    public void Put(ShardNote note)
    {
        lock (_gate)
        {
            var index = _notes.FindIndex(n => n.Shard == note.Shard);

            if (index >= 0) _notes[index] = note;
            else _notes.Add(note);

            _stamp.Adopt(note);
            Save();
        }
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_path))
                _notes = JsonSerializer.Deserialize<List<ShardNote>>(File.ReadAllText(_path)) ?? [];
        }
        catch (Exception e) when (e is IOException or JsonException)
        {
            // Opinions about servers, not the servers: a damaged file must
            // leave the page listing the visits rather than refusing to load.
            _notes = [];
        }

        _stamp.Loaded(_notes);
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        _stamp.Apply(_notes, DateTimeOffset.UtcNow);
        File.WriteAllText(_path, JsonSerializer.Serialize(_notes));
    }
}
