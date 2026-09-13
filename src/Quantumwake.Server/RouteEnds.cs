using Quantumwake.Data;

namespace Quantumwake.Server;

/// <summary>
/// What the map knows about each end of a trade route, and the filters the
/// route page applies to it.
/// </summary>
/// <remarks>
/// <para>
/// Price reports say nothing about hostility or what ship can land. The narrow
/// answers here come from two independent local sources: system security from
/// the resolved map place, and pad labels from the game map's own amenities.
/// An unmatched counter stays unknown; it is never promoted to safe or assumed
/// to fit a hull.
/// </para>
/// <para>
/// Shared by the route table and the circuits under it, which is the point:
/// the circuits once asked for outbound legs with no filter at all, so a table
/// set to Monitored only could have a Pyro loop proposed directly beneath it,
/// and nothing on the panel said so.
/// </para>
/// </remarks>
internal static class RouteEnds
{
    internal sealed record End(string PlaceId, string Security, IReadOnlyList<string> Amenities, List<string> Pads);

    /// <summary>
    /// Whether an amenity is somewhere a ship can be put down. The game map
    /// names them two ways: stations list <c>Landing Pad M</c> beside
    /// <c>Hangar L</c>, and the four Stanton cities list only <c>Hangar XL</c>.
    /// Reading the pads alone left New Babbage, Lorville, Area18 and Orison
    /// with "no pad record", and told a Hermes pilot asking for XL at both
    /// ends that nothing qualified.
    /// </summary>
    internal static bool IsLandingSpot(string amenity) =>
        amenity.Contains("Landing Pad", StringComparison.OrdinalIgnoreCase)
        || amenity.Contains("Hangar", StringComparison.OrdinalIgnoreCase);

    internal static End Of(LogLibrary lib, string terminal)
    {
        var place = lib.Terminals.Resolve(terminal);
        var amenities = place is null
            ? Array.Empty<string>()
            : lib.GameCommodities.Place(place.Name)?.Amenities ?? Array.Empty<string>();
        var pads = amenities.Where(IsLandingSpot).ToList();

        return new End(
            place?.RawId ?? string.Empty,
            TerminalPlaces.SecurityOfSystem(place?.System),
            amenities,
            pads);
    }

    internal static bool MatchesSafety(string buy, string sell, string filter) => filter switch
    {
        "monitored" => buy == "monitored" && sell == "monitored",
        "avoid-lawless" => buy != "lawless" && sell != "lawless",
        "lawless" => buy == "lawless" || sell == "lawless",
        _ => true,
    };

    private static bool HasXlPad(IReadOnlyList<string> pads) => pads.Any(p =>
        p.EndsWith(" XL", StringComparison.OrdinalIgnoreCase));

    internal static bool MatchesPads(IReadOnlyList<string> buy, IReadOnlyList<string> sell, string filter) => filter switch
    {
        "known" => buy.Count > 0 && sell.Count > 0,
        "xl" => HasXlPad(buy) && HasXlPad(sell),
        _ => true,
    };

    internal static int SafetyRank(string buy, string sell) => buy == "monitored" && sell == "monitored"
        ? 2 : buy == "lawless" || sell == "lawless" ? 0 : 1;

    /// <summary>Whether a route's two ends pass the page's safety and pad choices.</summary>
    internal static bool Admits(LogLibrary lib, UexRoute route, string safety, string pad)
    {
        var buy = Of(lib, route.BuyAt);
        var sell = Of(lib, route.SellAt);
        return MatchesSafety(buy.Security, sell.Security, safety)
            && MatchesPads(buy.Pads, sell.Pads, pad);
    }
}
