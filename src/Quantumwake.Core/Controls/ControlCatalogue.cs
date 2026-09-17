using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Quantumwake.Core.Controls;

/// <summary>An action map from the catalogue: a group of actions with a label and a category.</summary>
/// <param name="Name">The game's id - <c>spaceship_targeting</c>.</param>
/// <param name="Label">In the game's words - "Vehicles - Targeting"; the id when the strings have none.</param>
/// <param name="Category">The keybinding screen's tab - "FLIGHT"; empty for a map with none.</param>
public sealed record ActionMapInfo(string Name, string Label, string Category);

/// <summary>
/// One action the game can bind, with its defaults per device kind and how
/// it fires.
/// </summary>
/// <param name="Label">In the game's words - "Eject"; the id when the strings have none.</param>
/// <param name="Description">The keybinding screen's help line, or empty.</param>
/// <param name="Keyboard">The default keyboard input, or empty; a single space in the file means "none, but bindable".</param>
/// <param name="ActivationMode">tap, hold, double_tap … or empty for press-and-release.</param>
/// <param name="OptionGroup">The curve group an axis belongs to - <c>flight_move_pitch</c> - or empty.</param>
public sealed record ActionInfo(
    string ActionMap,
    string Name,
    string Label,
    string Description,
    string Keyboard,
    string Mouse,
    string Gamepad,
    string Joystick,
    string ActivationMode,
    string OptionGroup);

/// <summary>
/// The keybinding catalogue: every action the game can bind, from
/// <c>Data\Libs\Config\defaultProfile.xml</c>, labelled from the game's own
/// strings.
/// </summary>
/// <remarks>
/// 50 action maps and 1,106 actions on this install (Alpha 4.10); 770 of the
/// 837 distinct labels resolve through <c>global.ini</c>, and the rest read
/// as their id, which is still what the profile calls them.
/// </remarks>
public sealed record ControlCatalogue(
    IReadOnlyList<ActionMapInfo> ActionMaps,
    IReadOnlyList<ActionInfo> Actions)
{
    public static readonly ControlCatalogue Empty = new([], []);

    [JsonIgnore]
    public bool IsEmpty => Actions.Count == 0;

    /// <summary>The action, or null when the profile binds something the catalogue does not list.</summary>
    public ActionInfo? Find(string actionMap, string action) =>
        Actions.FirstOrDefault(a => a.ActionMap == actionMap && a.Name == action)
        ?? Actions.FirstOrDefault(a => a.Name == action);

    public static ControlCatalogue Parse(byte[] data, IReadOnlyDictionary<string, string> text) =>
        Parse(Quantumwake.Core.GameData.CryXml.Parse(data), text);

    public static ControlCatalogue Parse(XDocument document, IReadOnlyDictionary<string, string> text)
    {
        var root = document.Root ?? throw new InvalidDataException("No root element.");
        string Word(string? key)
        {
            if (string.IsNullOrWhiteSpace(key)) return "";
            var bare = key.StartsWith('@') ? key[1..] : key;
            // The strings file keeps a platform variant under "key,P" - the
            // PC wording - and for a third of the actions only that one:
            // ui_CIMissileMode has no plain line, ui_CIMissileMode,P does.
            if (text.TryGetValue(bare, out var value) && value.Length > 0) return value;
            if (text.TryGetValue(bare + ",P", out var pc) && pc.Length > 0) return pc;
            return bare;
        }

        var maps = new List<ActionMapInfo>();
        var actions = new List<ActionInfo>();
        foreach (var map in root.Elements("actionmap"))
        {
            var mapName = (string?)map.Attribute("name") ?? "";
            if (mapName.Length == 0) continue;
            // A category with no string - ui_CCSeatGeneral has none on this
            // install - is no category, not a raw key on a tab.
            var categoryKey = (string?)map.Attribute("UICategory");
            var category = Word(categoryKey);
            if (category == (categoryKey ?? "").TrimStart('@')) category = "";
            maps.Add(new ActionMapInfo(mapName, Word((string?)map.Attribute("UILabel")) is { Length: > 0 } l ? l : mapName, category));

            foreach (var action in map.Elements("action"))
            {
                var name = (string?)action.Attribute("name") ?? "";
                if (name.Length == 0) continue;
                actions.Add(new ActionInfo(
                    mapName, name,
                    Word((string?)action.Attribute("UILabel")) is { Length: > 0 } label ? label : name,
                    Word((string?)action.Attribute("UIDescription")),
                    ((string?)action.Attribute("keyboard") ?? "").Trim(),
                    ((string?)action.Attribute("mouse") ?? "").Trim(),
                    ((string?)action.Attribute("gamepad") ?? "").Trim(),
                    ((string?)action.Attribute("joystick") ?? "").Trim(),
                    (string?)action.Attribute("activationMode") ?? "",
                    (string?)action.Attribute("optionGroup") ?? ""));
            }
        }

        return new ControlCatalogue(maps, actions);
    }
}
