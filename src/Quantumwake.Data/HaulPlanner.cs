using Quantumwake.Core.State;

namespace Quantumwake.Data;

/// <summary>One accepted hauling contract with everything known about where it goes.</summary>
/// <param name="Source">
/// Where the legs came from: <c>screenshot</c> when a Contracts-app frame
/// of this contract was read, <c>title</c> when only the title named an
/// end, <c>none</c> when nothing did.
/// </param>
/// <param name="Shot">The frame the legs were read from, when they were.</param>
/// <param name="Note">What the plan could not see about this contract, in words.</param>
public sealed record PlannedContract(
    string Title,
    string? MissionId,
    string? Commodity,
    HaulShape Shape,
    IReadOnlyList<HaulLeg> Legs,
    string Source,
    string? Shot,
    DateTimeOffset? ShotAt,
    int Pickups,
    int PickupsDone,
    int Deliveries,
    int DeliveriesDone,
    int? Scu,
    string? Note);

/// <summary>Something to do at a stop: load or unload some cargo for one contract.</summary>
/// <param name="Scu">The count when the screen printed one for this leg; null when only the contract total is known.</param>
/// <param name="Note">Set on an unload whose contract has no pickup on the plan: it is placed last, and says why.</param>
public sealed record HaulAction(string Kind, string? Commodity, int? Scu, int Contract, string ContractTitle, string? Note = null);

/// <summary>One visit on the run.</summary>
/// <param name="PlaceId">The map's id when the atlas knows the place; empty when it does not.</param>
/// <param name="Body">The body the place is on or above, from the atlas or from the contract text.</param>
public sealed record HaulStop(
    string Place,
    string PlaceId,
    string? Body,
    string? System,
    IReadOnlyList<HaulAction> Actions,
    string? Note);

/// <summary>The run: every contract, the stops in an order, and what the plan could not see.</summary>
/// <param name="KnownScu">SCU summed over the legs that printed a count, and delivery totals where they did not - a floor.</param>
/// <param name="Notes">What is missing and what a screenshot would add, for the page to say.</param>
public sealed record HaulPlan(
    IReadOnlyList<PlannedContract> Contracts,
    IReadOnlyList<HaulStop> Stops,
    int KnownScu,
    IReadOnlyList<string> Notes);

/// <summary>What the atlas says about a place name, when it says anything.</summary>
public sealed record ResolvedPlace(string Id, string Name, string? Body, string? System);

/// <summary>
/// Puts every open hauling contract on one route.
/// </summary>
/// <remarks>
/// <para>
/// A hauler with five contracts open sees five cards, each naming its own
/// destination and nothing else; where the cargo is collected is in each
/// card's detail panel, one card at a time. So the plan is assembled from
/// two sources of unequal reach. The logs know every open contract and,
/// with the text mod, one end of most (54 of 139 named a delivery on this
/// install, 41 a pickup, 14 both). A Contracts-app screenshot of a card
/// gives that card's every leg. Each contract says which it got.
/// </para>
/// <para>
/// A frame is matched to a contract by title, which is not unique - three
/// of the five cards on the 8 Sep frame read "to Stanton Gateway" - so the
/// cargo and the number of sources break the tie, and a frame already
/// given to one contract is not given to a second unless nothing else fits.
/// </para>
/// <para>
/// The order is pickups before deliveries, which is the one rule that is
/// always true, with stops on the same body kept together because a
/// quantum hop is the expensive part. It is a starting order, not the
/// answer: the plan is written into a flight plan the pilot can reorder.
/// Nothing here knows a distance - the atlas has no coordinates for a
/// station - so it does not pretend to.
/// </para>
/// </remarks>
public static class HaulPlanner
{
    public static HaulPlan Plan(
        IReadOnlyList<ContractRecord> contracts,
        IReadOnlyList<ScreenSighting> frames,
        Func<string, ResolvedPlace?> resolve)
    {
        var hauling = contracts
            .Where(c => HaulingContract.IsHauling(c.Raw))
            .Where(c => c.CompletedAt is null && c.Outcome is ContractOutcome.Unknown or ContractOutcome.InProgress)
            .OrderBy(c => c.FirstSeen)
            .ToList();

        var planned = new List<PlannedContract>();
        var taken = new HashSet<string>(StringComparer.Ordinal);

        foreach (var contract in hauling)
        {
            var title = ContractTags.Clean(contract.Name);
            var archetype = HaulingContract.FromArchetype(contract.Raw);
            var route = HaulingContract.RouteFromTitle(contract.Title);

            var frame = BestFrame(contract, title, archetype, frames, taken);
            var reused = frame is not null && taken.Contains(frame.Shot);

            if (frame is not null)
            {
                taken.Add(frame.Shot);
                var (legs, deliveries) = HaulLegs.From(frame.Contracts!);

                var scu = legs.Any(l => l.Scu is not null)
                    ? legs.Sum(l => l.Scu ?? 0)
                    : deliveries.Sum(d => d.Scu ?? 0);

                planned.Add(new PlannedContract(
                    title, contract.MissionId, archetype?.Commodity ?? legs.FirstOrDefault(l => l.Commodity is not null)?.Commodity,
                    archetype?.Shape ?? HaulShape.Unknown, legs, "screenshot", frame.Shot, frame.ShotAt,
                    contract.Pickups, contract.PickupsDone, contract.Deliveries, contract.DeliveriesDone,
                    scu > 0 ? scu : null,
                    legs.Count == 0 ? "the frame showed this card selected but no objectives read"
                        : reused ? "read from a frame that also matched another card with this title; photograph this card to be sure"
                        : null));
                continue;
            }

            // Only the title to go on: one leg with whatever end it named, and
            // the archetype's count of the other end as the note.
            var fromTitle = route is null ? [] : new List<HaulLeg>
            {
                new(route.Pickup, null, route.Delivery, null, archetype?.Commodity, null, null),
            };

            planned.Add(new PlannedContract(
                title, contract.MissionId, archetype?.Commodity, archetype?.Shape ?? HaulShape.Unknown,
                fromTitle, route is null ? "none" : "title", null, null,
                contract.Pickups, contract.PickupsDone, contract.Deliveries, contract.DeliveriesDone,
                null, MissingNote(route, archetype)));
        }

        var stops = Route(planned, resolve);

        var known = planned.Sum(c => c.Scu ?? 0);
        var notes = new List<string>();

        var unread = planned.Count(c => c.Source != "screenshot");
        if (unread > 0)
            notes.Add(unread == 1
                ? $"1 of {planned.Count} contract{(planned.Count == 1 ? "" : "s")} has no screenshot of its card: open it on the Contracts app's Accepted tab and take one, and its pickups and SCU will be read."
                : $"{unread} of {planned.Count} contracts have no screenshot of their card: open each on the Contracts app's Accepted tab and take one, and its pickups and SCU will be read.");

        if (planned.Any(c => c.Scu is null))
            notes.Add("The SCU total is a floor: it counts only contracts whose card was read.");

        var unplaced = stops.Where(s => s.PlaceId.Length == 0).Select(s => s.Place).Distinct().ToList();
        if (unplaced.Count > 0)
            notes.Add($"Not on the map yet: {string.Join(", ", unplaced)}. A place is on the map once the logs have seen you there; the stop keeps its name either way.");

        return new HaulPlan(planned, stops, known, notes);
    }

    private static string? MissingNote(HaulRoute? route, HaulArchetype? archetype)
    {
        if (route is null)
            return "the title names no place; only a screenshot of this card can say where it goes";

        if (route.Pickup is null)
            return archetype?.Pickups is { } n and > 1
                ? $"{n} pickups, places unknown until this card is photographed"
                : "pickup unknown until this card is photographed";

        if (route.Delivery is null)
            return archetype?.Deliveries is { } n and > 1
                ? $"{n} drop-offs, places unknown until this card is photographed"
                : "drop-off unknown until this card is photographed";

        return null;
    }

    /// <summary>
    /// The newest frame showing this contract selected, taken after it was
    /// accepted, with the cargo and the source count as tie-breakers.
    /// </summary>
    private static ScreenSighting? BestFrame(
        ContractRecord contract, string title, HaulArchetype? archetype,
        IReadOnlyList<ScreenSighting> frames, HashSet<string> taken)
    {
        ScreenSighting? best = null;
        var bestScore = int.MinValue;

        foreach (var frame in frames
            .Where(f => !f.Dismissed && f.Contracts?.SelectedTitle is { Length: > 0 })
            .Where(f => f.ShotAt >= contract.FirstSeen)
            .OrderByDescending(f => f.ShotAt))
        {
            if (!ScreenFrames.SameContract(frame.Contracts!.SelectedTitle!, title))
                continue;

            var (legs, _) = HaulLegs.From(frame.Contracts);
            var score = 0;

            if (archetype?.Commodity is { } cargo && legs.Any(l => SameCargo(l.Commodity, cargo)))
                score += 2;

            if (archetype?.Pickups is { } sources && legs.Select(l => l.Pickup).Distinct().Count() == sources)
                score += 1;

            // A frame already explaining another contract is a weaker claim
            // for this one, but still better than nothing.
            if (taken.Contains(frame.Shot))
                score -= 10;

            if (score > bestScore)
                (best, bestScore) = (frame, score);
        }

        return best;
    }

    /// <summary>"Aluminum" and "Aluminium" are one cargo; the engine's reading of either is close enough on six letters.</summary>
    internal static bool SameCargo(string? a, string? b)
    {
        if (a is null || b is null) return false;

        var x = ScreenInsight.Fold(a);
        var y = ScreenInsight.Fold(b);
        var n = Math.Min(6, Math.Min(x.Length, y.Length));

        return n > 0 && string.CompareOrdinal(x, 0, y, 0, n) == 0;
    }

    /// <summary>
    /// Stops in an order: every pickup before any delivery, same-body stops
    /// together, and a place that is both a source and a destination visited
    /// twice when its delivery depends on a pickup not yet made.
    /// </summary>
    private static List<HaulStop> Route(IReadOnlyList<PlannedContract> contracts, Func<string, ResolvedPlace?> resolve)
    {
        var places = new Dictionary<string, Place>(StringComparer.Ordinal);

        Place At(string name, string? bodyHint)
        {
            var key = ScreenInsight.Fold(name);

            if (!places.TryGetValue(key, out var place))
            {
                var known = resolve(name);
                places[key] = place = new Place(
                    known?.Name ?? name,
                    known?.Id ?? string.Empty,
                    known?.Body ?? bodyHint,
                    known?.System);
            }

            place.Body ??= bodyHint;
            return place;
        }

        for (var i = 0; i < contracts.Count; i++)
        {
            var contract = contracts[i];
            var blind = !contract.Legs.Any(l => l.Pickup is not null);

            foreach (var leg in contract.Legs.Where(l => l.Pickup is not null))
                At(leg.Pickup!, leg.PickupBody).Loads.Add(new HaulAction("load", leg.Commodity, leg.Scu, i, contract.Title));

            // One unload per destination per contract, not one per leg: three
            // sources feeding Stanton Gateway are one drop of 18 SCU, and the
            // screen printed that total on the parent line rather than a share
            // on each source.
            foreach (var group in contract.Legs.Where(l => l.Delivery is not null).GroupBy(l => ScreenInsight.Fold(l.Delivery!)))
            {
                var legs = group.ToList();
                var scu = legs.All(l => l.Scu is not null) ? legs.Sum(l => l.Scu)
                    : legs.Count == contract.Legs.Count ? contract.Scu
                    : null;

                At(legs[0].Delivery!, legs[0].DeliveryBody).Unloads.Add(new HaulAction(
                    "unload", legs.Select(l => l.Commodity).FirstOrDefault(c => c is not null), scu, i, contract.Title,
                    blind ? "after its pickups, which are not on this plan" : null));
            }
        }

        // A delivery for contract i is ready once every pickup of i that the
        // plan knows about has been visited. A contract whose pickups it
        // cannot see at all waits for every pickup it can: the cargo has to
        // come from somewhere, and last is the only honest place for it.
        var pickupsLeft = contracts.Select((c, i) => (i, n: places.Values.Sum(p => p.Loads.Count(a => a.Contract == i)))).ToDictionary(x => x.i, x => x.n);
        var stops = new List<HaulStop>();
        var pending = places.Values.ToList();
        string? lastBody = null;

        bool Ready(HaulAction unload) =>
            pickupsLeft[unload.Contract] == 0
            && (unload.Note is null || pending.All(p => p.Loads.Count == 0));

        while (pending.Count > 0)
        {
            var ready = pending.Where(p => p.Unloads.All(Ready)).ToList();

            // Sources first, and among them the ones whose deliveries are also
            // ready so the stop is done in one visit; same body as the last
            // stop before anything else.
            var next = Prefer(ready.Where(p => p.Loads.Count > 0), lastBody)
                ?? Prefer(pending.Where(p => p.Loads.Count > 0), lastBody)
                ?? Prefer(ready, lastBody)
                ?? Prefer(pending, lastBody);

            pending.Remove(next);
            foreach (var load in next.Loads) pickupsLeft[load.Contract]--;

            var unloadsNow = next.Unloads.Where(Ready).ToList();
            var later = next.Unloads.Except(unloadsNow).ToList();
            var actions = next.Loads.Concat(unloadsNow).ToList();

            // The same place twice running is one visit: the second only
            // existed because its delivery was waiting on the first's pickup.
            if (stops.Count > 0 && stops[^1].Place == next.Name && stops[^1].PlaceId == next.Id)
            {
                var last = stops[^1];
                stops[^1] = last with { Actions = [.. last.Actions, .. actions], Note = later.Count > 0 ? last.Note : null };
            }
            else
            {
                stops.Add(new HaulStop(next.Name, next.Id, next.Body, next.System, actions,
                    later.Count > 0 ? "back here later to deliver, once its cargo is aboard" : null));
            }

            lastBody = next.Body;

            // The deliveries that could not happen yet get a second visit.
            if (later.Count > 0)
                pending.Add(new Place(next.Name, next.Id, next.Body, next.System) { Unloads = later });
        }

        return stops;
    }

    private static Place? Prefer(IEnumerable<Place> candidates, string? body)
    {
        var list = candidates.ToList();
        if (list.Count == 0) return null;

        return list.FirstOrDefault(p => body is not null && p.Body is not null
                && string.Equals(p.Body, body, StringComparison.OrdinalIgnoreCase))
            ?? list.OrderByDescending(p => p.Loads.Count + p.Unloads.Count).First();
    }

    private sealed class Place(string name, string id, string? body, string? system)
    {
        public string Name => name;
        public string Id => id;
        public string? Body { get; set; } = body;
        public string? System => system;
        public List<HaulAction> Loads { get; init; } = [];
        public List<HaulAction> Unloads { get; init; } = [];
    }
}
