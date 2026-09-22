namespace Quantumwake.Core.Controls;

/// <summary>One thing a reference binds on this stick that the profile does not.</summary>
/// <param name="Suggested">The input as this pilot's profile would write it - the reference's control moved onto their instance number.</param>
/// <param name="Weight">How much of the pilot's own flying lives in this action map; the list is read top down.</param>
public sealed record ControlsGap(
    string DeviceKey,
    string? Product,
    string ActionMap,
    string Action,
    string Label,
    string Map,
    string Suggested,
    string Source,
    int Weight);

/// <summary>
/// What the check looked at and what it put aside, so a short list can be
/// read as a short list rather than as a check that failed to run.
/// </summary>
public sealed record ControlsCheckCounts(
    int Reference,
    int Bound,
    int Cleared,
    int Undefined,
    int Dismissed,
    int Gaps);

public sealed record ControlsCheckResult(
    IReadOnlyList<ControlsGap> Gaps,
    ControlsCheckCounts Counts,
    IReadOnlyList<string> Sources);

/// <summary>
/// What a stick is missing, measured against the layouts the game ships for
/// it.
/// </summary>
/// <remarks>
/// <para>
/// The naive answer is a diff, and the naive answer is almost entirely wrong.
/// Against this install on 2026-09-18: the Warthog layout binds 73 actions,
/// 29 of which are not on the pilot's sticks, and exactly <b>one</b> of those
/// 29 is a gap. Three filters stand between the two numbers, and without all
/// three the feature is noise that teaches people to ignore it.
/// </para>
/// <para>
/// <b>Cleared, not missing.</b> The profile records a default taken away as
/// <c>jsN_</c> with no control - 16 of the 29 here. The pilot flies roll on
/// the rudder axis and took the button version off on purpose; telling them
/// to put it back is telling them they were wrong.
/// </para>
/// <para>
/// <b>The layouts ship stale.</b> 12 of the 29 name actions this patch no
/// longer defines: the game renamed
/// <c>v_ifcs_toggle_vector_decoupling</c> to
/// <c>v_ifcs_vector_decoupling_toggle</c> and never updated
/// <c>layout_hotas_warthog.xml</c>. The pilot has the new one bound. Anything
/// the catalogue does not define is dropped, because a suggestion to bind a
/// name the game has forgotten cannot be acted on and is not true.
/// </para>
/// <para>
/// <b>Dismissed stays dismissed.</b> A check that asks twice is a check people
/// turn off.
/// </para>
/// </remarks>
public static class ControlsCheck
{
    /// <summary>
    /// The gaps for one pilot, against every reference that names one of
    /// their sticks.
    /// </summary>
    /// <param name="mine">The live profile.</param>
    /// <param name="references">The reference profiles, each with the name it is known by.</param>
    /// <param name="catalogue">This patch's actions; an empty one disables the staleness filter rather than dropping everything.</param>
    /// <param name="dismissed">Keys - <c>deviceKey/actionMap/action</c> - the pilot has said no to.</param>
    public static ControlsCheckResult Against(
        ControlProfile mine,
        IReadOnlyList<(string Source, ControlProfile Profile)> references,
        ControlCatalogue catalogue,
        IReadOnlySet<string>? dismissed = null)
    {
        dismissed ??= new HashSet<string>();
        var gaps = new List<ControlsGap>();
        int reference = 0, bound = 0, cleared = 0, undefined = 0, skipped = 0;
        var sources = new List<string>();

        // An action bound anywhere counts as bound: the pilot may have put it
        // on the throttle where the layout puts it on the stick, and that is
        // not a gap.
        var mineLive = new HashSet<string>(
            mine.Bindings.Where(b => b.Input.Kind != InputKind.None).Select(Key), StringComparer.OrdinalIgnoreCase);
        var mineCleared = new HashSet<string>(
            mine.Bindings.Where(b => b.Input.Kind == InputKind.None).Select(Key), StringComparer.OrdinalIgnoreCase);

        // How much of this pilot's flying is in each map, from their own
        // profile. A gap in a map they have thirty bindings in matters more
        // than one in a map they have never touched. It is a proxy - the logs
        // would say it better - so it only ever orders the list.
        var weights = mine.Bindings
            .Where(b => b.Input.Kind != InputKind.None)
            .GroupBy(b => b.ActionMap, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        foreach (var (source, theirs) in references)
        {
            var used = false;
            foreach (var stick in mine.Joysticks)
            {
                if (stick.Guid is null) continue;
                // The reference names the same product under its own instance
                // number; match on the GUID and move the inputs across.
                var match = theirs.Joysticks.FirstOrDefault(
                    d => d.Guid is not null && string.Equals(d.Guid, stick.Guid, StringComparison.OrdinalIgnoreCase));
                if (match is null) continue;
                used = true;

                foreach (var binding in theirs.Bindings)
                {
                    if (binding.Input.DeviceKey != match.Key || binding.Input.Kind == InputKind.None) continue;
                    reference++;
                    var key = Key(binding);

                    if (mineLive.Contains(key)) { bound++; continue; }
                    if (mineCleared.Contains(key)) { cleared++; continue; }

                    var info = catalogue.IsEmpty ? null : catalogue.Find(binding.ActionMap, binding.Action);
                    if (!catalogue.IsEmpty && info is null) { undefined++; continue; }

                    if (dismissed.Contains($"{stick.Key}/{key}")) { skipped++; continue; }

                    gaps.Add(new ControlsGap(
                        stick.Key,
                        stick.Product,
                        binding.ActionMap,
                        binding.Action,
                        info?.Label is { Length: > 0 } label ? label : binding.Action,
                        MapLabel(catalogue, binding.ActionMap),
                        binding.Input.OnInstance(stick.Instance),
                        source,
                        weights.GetValueOrDefault(binding.ActionMap)));
                }
            }
            if (used) sources.Add(source);
        }

        // The same action can sit in two references for one stick; the first
        // one that named it is the one reported.
        var once = gaps
            .GroupBy(g => $"{g.DeviceKey}/{g.ActionMap}/{g.Action}", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderByDescending(g => g.Weight)
            .ThenBy(g => g.Map, StringComparer.OrdinalIgnoreCase)
            .ThenBy(g => g.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new ControlsCheckResult(
            once,
            new ControlsCheckCounts(reference, bound, cleared, undefined, skipped, once.Count),
            sources);
    }

    private static string Key(ControlBinding b) => $"{b.ActionMap}/{b.Action}";

    private static string MapLabel(ControlCatalogue catalogue, string actionMap) =>
        catalogue.ActionMaps.FirstOrDefault(m => string.Equals(m.Name, actionMap, StringComparison.OrdinalIgnoreCase))
            is { } map && map.Label.Length > 0
            ? map.Label
            : actionMap;
}
