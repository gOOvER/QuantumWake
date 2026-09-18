using System.Text.Json;
using Quantumwake.Core;

namespace Quantumwake.Data;

/// <summary>
/// The suggestions the pilot has said no to, kept so the check converges on
/// nothing rather than asking again every time the page opens.
/// </summary>
/// <remarks>
/// A key is <c>deviceKey/actionMap/action</c> - <c>js4/seat_general/v_eject</c>
/// - which is deliberately not the input: saying no to "put Eject on this
/// stick" should survive the pilot moving their other bindings around, and
/// should come back if they unplug the stick and a different one becomes js4,
/// because then it is a different question.
///
/// Kept as a flat list in <c>controls\dismissed.json</c>. It is preference,
/// not evidence: losing the file costs a few clicks and nothing else, so
/// there is no schema and no migration.
/// </remarks>
public sealed class ControlsDismissals
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    private readonly string _path;
    private readonly Lock _gate = new();
    private HashSet<string> _keys;

    public ControlsDismissals(string? directory = null)
    {
        var folder = directory ?? AppPaths.In("controls");
        _path = Path.Combine(folder, "dismissed.json");
        _keys = Read(_path);
    }

    /// <summary>Every key the pilot has dismissed.</summary>
    public IReadOnlySet<string> Keys
    {
        get { lock (_gate) return new HashSet<string>(_keys, StringComparer.OrdinalIgnoreCase); }
    }

    /// <summary>Say no to a suggestion, or take the no back.</summary>
    public void Set(string key, bool dismissed)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        lock (_gate)
        {
            var next = new HashSet<string>(_keys, StringComparer.OrdinalIgnoreCase);
            if (dismissed) next.Add(key.Trim()); else next.Remove(key.Trim());
            _keys = next;
            Save();
        }
    }

    /// <summary>Ask everything again.</summary>
    public void Clear()
    {
        lock (_gate)
        {
            _keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Save();
        }
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(_keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase), Json));
    }

    private static HashSet<string> Read(string path)
    {
        try
        {
            return File.Exists(path)
                ? new HashSet<string>(JsonSerializer.Deserialize<List<string>>(File.ReadAllText(path)) ?? [], StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception e) when (e is JsonException or IOException or UnauthorizedAccessException)
        {
            // A preference file nobody can read is not worth failing the page for.
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
