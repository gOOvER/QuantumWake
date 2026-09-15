using Quantumwake.Core.GameData;
using System.Text.RegularExpressions;

namespace Quantumwake.Data;

/// <summary>One port of the photographed fit, and what became of it on the bench.</summary>
/// <param name="Slot">The port as the loadout screen labelled it.</param>
/// <param name="PortId">The dump's port it was matched to, or null when none was.</param>
/// <param name="Class">The class put in the port: a part, or null for a port the screen said was empty.</param>
/// <param name="Changed">Whether that differs from what the ship ships with.</param>
/// <param name="Why">For a port not applied: the reason, in the words the page shows.</param>
public sealed record PhotographedPort(
    string Slot,
    string? PortId,
    string? Name,
    string? Class,
    bool Applied,
    bool Changed,
    string? Why);

/// <summary>A loadout screenshot turned into bench swaps for one ship.</summary>
/// <param name="Swaps">Port id to class, null for emptied - the shape the sheet takes.</param>
public sealed record PhotographedFit(
    string Shot,
    DateTimeOffset ShotAt,
    string Ship,
    string? Scope,
    IReadOnlyDictionary<string, string?> Swaps,
    IReadOnlyList<PhotographedPort> Ports)
{
    public int Applied => Ports.Count(p => p.Applied);
    public int Changed => Ports.Count(p => p.Applied && p.Changed);
}

/// <summary>
/// Turns what a Vehicle Loadout Manager frame read into swaps the bench can
/// start from, port by port.
/// </summary>
/// <remarks>
/// <para>
/// The screen labels ports by kind and number - <c>Cooler 2</c>,
/// <c>Power Plant 1</c>, <c>Shield Generator 4</c> - and the dump names them
/// by hardpoint - <c>hardpoint_cooler_01</c>, <c>hardpoint_cooler_left</c>.
/// Where the hardpoint carries a number, that is the number the screen
/// prints (the Hermes' <c>hardpoint_shield_generator_02_hermes</c> is its
/// Shield Generator 2, and the dump happens to list it before _01). Where a
/// kind's hardpoints carry none - left and right - the dump's order stands in
/// for the screen's, which is an assumption about the game rather than a
/// fact read from it, and it is why the page calls the result a starting
/// point and dates it, not the fit.
/// </para>
/// <para>
/// Only a port read with one class is applied. A reading two parts fit
/// equally (the M6A that read as the M8A) names neither, and the bench must
/// not pick for it; a part the catalogue did not recognise stays as read; a
/// port the screen printed <c>Empty</c> in is emptied, because that is a
/// reading too. Everything not applied is listed with its reason, so the
/// pilot can see what the screenshot did not settle.
/// </para>
/// </remarks>
public static class GaragePhotograph
{
    /// <summary>The screen's word for a kind, to the dump's group.</summary>
    private static readonly Dictionary<string, string> Kinds = new(StringComparer.OrdinalIgnoreCase)
    {
        ["cooler"] = "Cooler",
        ["power plant"] = "PowerPlant",
        ["shield generator"] = "Shield",
        ["shield"] = "Shield",
        ["quantum drive"] = "QuantumDrive",
        ["missile rack"] = "MissileLauncher",
        ["radar"] = "Radar",
        ["emp"] = "EMP",
        ["quantum enforcement device"] = "QuantumInterdictionGenerator",
        ["mining laser"] = "WeaponMining",
        ["weapon"] = "WeaponGun",
    };

    /// <summary>
    /// A port label as the Vehicle Loadout Manager prints it - <c>Cooler 2</c>,
    /// an ordinal - or as the loadout estimate does - <c>Cooler ×2</c>, a
    /// count of ports carrying the same part.
    /// </summary>
    private static readonly Regex Label = new(@"^(?<kind>.+?)\s*(?:×\s*(?<count>\d+)|(?<n>\d+))?$", RegexOptions.Compiled);

    /// <summary>
    /// The dump's group for a screen's word, or null. The estimate's type
    /// column pluralises some kinds (<c>Quantum Drives</c>), and the engine
    /// reads its Q as an O on the Fleet Manager's amber - a confusion of the
    /// two glyphs, not of the frame, so it is undone here rather than taught
    /// to the reader as a name.
    /// </summary>
    private static string? KindOf(string word)
    {
        var text = word.Trim();
        if (text.StartsWith("ouantum", StringComparison.OrdinalIgnoreCase))
            text = "q" + text[1..];

        if (Kinds.TryGetValue(text, out var kind)) return kind;
        if (text.EndsWith('s') && Kinds.TryGetValue(text[..^1], out kind)) return kind;
        return null;
    }

    /// <summary>
    /// Matches a reading to a ship. Null when the reading is not of this ship:
    /// the name has to have read exactly, a resemblance is not enough.
    /// </summary>
    public static PhotographedFit? Match(
        ShipBase ship,
        ScreenSighting sighting,
        IReadOnlyDictionary<string, PartStats> parts,
        IReadOnlyDictionary<string, ShipInfo> ships)
    {
        var reading = sighting.Loadout;
        if (reading?.Ship is null || !IsShip(reading.Ship, ship, ships))
            return null;

        // Editable ports by group, in the dump's order; guns under a turret are
        // left out because the screen numbers them within the turret and the
        // dump within the ship, and the two do not line up.
        var byGroup = new Dictionary<string, List<FitPort>>(StringComparer.Ordinal);
        Walk(ship.Loadout, underTurret: false, byGroup);
        foreach (var group in byGroup.Keys.ToList())
            byGroup[group] = Numbered(byGroup[group]);

        var swaps = new Dictionary<string, string?>(StringComparer.Ordinal);
        var ports = new List<PhotographedPort>();

        foreach (var fitting in reading.Fittings)
        {
            var match = Label.Match(fitting.Slot.Trim());
            var kind = KindOf(match.Groups["kind"].Value);

            if (kind is null)
            {
                ports.Add(Skipped(fitting, "not a kind the bench changes"));
                continue;
            }

            byGroup.TryGetValue(kind, out var list);
            list ??= [];

            // An ordinal names one port; a count from the estimate names the
            // first that many of the kind, which is a claim about the ship
            // (it has that many, all alike) rather than about which is which -
            // and the estimate does not say which, so the dump's order stands
            // in, as it does for left and right.
            var targets = new List<FitPort>();
            if (match.Groups["count"].Success)
            {
                targets.AddRange(list.Take(int.Parse(match.Groups["count"].Value)));
            }
            else
            {
                var ordinal = match.Groups["n"].Success ? int.Parse(match.Groups["n"].Value) : 1;
                if (ordinal >= 1 && ordinal <= list.Count) targets.Add(list[ordinal - 1]);
                if (targets.Count == 0)
                {
                    ports.Add(Skipped(fitting, $"the ship has no {Kinds.First(k => k.Value == kind).Key} {ordinal} the bench can change"));
                    continue;
                }
            }

            if (targets.Count == 0)
            {
                ports.Add(Skipped(fitting, $"the ship has no {Kinds.First(k => k.Value == kind).Key} the bench can change"));
                continue;
            }

            foreach (var port in targets) Apply(fitting, port);
        }

        return new PhotographedFit(sighting.Shot, sighting.ShotAt, reading.Ship, reading.Scope, swaps, ports);

        void Apply(ScreenFitting fitting, FitPort port)
        {
            if (fitting.IsEmpty)
            {
                swaps[port.PortId] = null;
                ports.Add(new PhotographedPort(fitting.Slot, port.PortId, null, null, true, port.Class is not null, null));
                return;
            }

            if (fitting.NothingRead)
            {
                ports.Add(Skipped(fitting, "nothing read under it", port.PortId));
                return;
            }

            if (fitting.ClassName is null)
            {
                ports.Add(Skipped(fitting, fitting.Name is null
                    ? $"read “{fitting.Read}”, which names no one part"
                    : $"read as {fitting.Name}, which more than one class answers to", port.PortId));
                return;
            }

            if (!parts.TryGetValue(fitting.ClassName, out var part))
            {
                ports.Add(Skipped(fitting, $"{fitting.Name ?? fitting.ClassName} is not in the reference", port.PortId));
                return;
            }

            if (!port.Accepts.Contains(part.Type, StringComparer.Ordinal) || part.Size < port.MinSize || part.Size > port.MaxSize)
            {
                ports.Add(Skipped(fitting, $"{part.Name} (S{part.Size} {part.Type}) does not fit that port", port.PortId));
                return;
            }

            swaps[port.PortId] = part.Class;
            ports.Add(new PhotographedPort(
                fitting.Slot, port.PortId, part.Name, part.Class, true,
                !string.Equals(part.Class, port.Class, StringComparison.Ordinal), null));
        }
    }

    /// <summary>
    /// The newest reading of this ship, matched. Null when none of the
    /// readings is of it.
    /// </summary>
    public static PhotographedFit? Latest(
        ShipBase ship,
        IEnumerable<ScreenSighting> latestLoadouts,
        IReadOnlyDictionary<string, PartStats> parts,
        IReadOnlyDictionary<string, ShipInfo> ships) =>
        latestLoadouts
            .OrderByDescending(s => s.ShotAt)
            .Select(s => Match(ship, s, parts, ships))
            .FirstOrDefault(fit => fit is not null);

    /// <summary>
    /// Whether a name the screen read is this ship: the garage's own name for
    /// it, or the fleet's name for the class the garage entry is keyed by.
    /// </summary>
    private static bool IsShip(string read, ShipBase ship, IReadOnlyDictionary<string, ShipInfo> ships)
    {
        if (string.Equals(read, ship.Name, StringComparison.OrdinalIgnoreCase))
            return true;

        return ships.TryGetValue(ship.Class, out var info)
            && string.Equals(read, info.Name, StringComparison.OrdinalIgnoreCase);
    }

    private static void Walk(IReadOnlyList<FitPort> tree, bool underTurret, Dictionary<string, List<FitPort>> byGroup)
    {
        foreach (var port in tree)
        {
            var turret = port.Accepts.Contains("Turret", StringComparer.Ordinal)
                || string.Equals(port.Type, "Turret", StringComparison.Ordinal);

            // A port is filed under the kind of the part it ships with, which is
            // the kind the screen labels it by; only an empty port falls back to
            // everything it accepts.
            if (port.Editable && !underTurret)
            {
                var groups = port.Type is { Length: > 0 } ? [port.Type] : port.Accepts.Distinct(StringComparer.Ordinal);
                foreach (var group in groups)
                {
                    if (!byGroup.TryGetValue(group, out var list))
                        byGroup[group] = list = [];
                    list.Add(port);
                }
            }

            Walk(port.Children, underTurret || turret, byGroup);
        }
    }

    /// <summary>
    /// A kind's ports in the screen's order: by the number in the hardpoint
    /// name when every one of them carries one, else as the dump lists them.
    /// </summary>
    private static List<FitPort> Numbered(List<FitPort> ports)
    {
        var numbers = ports.Select(p => HardpointNumber.Match(p.Hardpoint)).ToList();
        if (numbers.Any(m => !m.Success))
            return ports;

        return [.. ports
            .Select((p, i) => (Port: p, Number: int.Parse(numbers[i].Groups["n"].Value), Index: i))
            .OrderBy(x => x.Number).ThenBy(x => x.Index)
            .Select(x => x.Port)];
    }

    /// <summary>The first number in a hardpoint name: cooler_01, shield_generator_02_hermes, class_2.</summary>
    private static readonly Regex HardpointNumber = new(@"_(?<n>\d{1,2})(?=_|$)", RegexOptions.Compiled);

    private static PhotographedPort Skipped(ScreenFitting fitting, string why, string? portId = null) =>
        new(fitting.Slot, portId, fitting.Name, fitting.ClassName, false, false, why);
}
