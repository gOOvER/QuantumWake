using System.Text.RegularExpressions;

namespace Quantumwake.Core.State;

/// <summary>The two ends of a hauling contract, as far as its title says.</summary>
/// <remarks>
/// Either side is null when the title is silent about it. A title that names
/// only where the cargo goes - <c>Junior | Stellar Small Haul | to Stanton
/// Gateway</c> - is the common case, and the pickups behind it are only ever
/// printed in the mobiGlas.
/// </remarks>
public sealed record HaulRoute(string? Pickup, string? Delivery)
{
    public bool Any => Pickup is not null || Delivery is not null;
}

/// <summary>How many places a hauling contract touches at each end.</summary>
public enum HaulShape
{
    Unknown,

    /// <summary>One pickup, one delivery: <c>AToB</c>.</summary>
    Direct,

    /// <summary>One pickup, several deliveries: <c>SingleToMulti3</c>.</summary>
    SingleToMulti,

    /// <summary>Several pickups, one delivery: <c>Multi2ToSingle</c>.</summary>
    MultiToSingle,
}

/// <summary>What the contract's archetype id says about its legs and cargo.</summary>
/// <param name="Pickups">How many places cargo is collected from, when the id counts them.</param>
/// <param name="Deliveries">How many places it goes to, when the id counts them.</param>
/// <param name="Commodity">The cargo, spelled as the id spells it - <c>Aluminium</c> and <c>Aluminum</c> both occur.</param>
public sealed record HaulArchetype(HaulShape Shape, int? Pickups, int? Deliveries, string? Commodity);

/// <summary>
/// Reads the two things the logs do say about a hauling contract's route.
/// </summary>
/// <remarks>
/// <para>
/// The logs never print where a hauling contract's cargo is picked up or
/// delivered - the objective ids are opaque uuids and the objective text stays
/// on the server. Two fragments survive. The acceptance toast carries the
/// contract's <em>displayed</em> title, and with the StarStrings text mod
/// installed that title names a place: across 1,160 hauling acceptances on
/// this install, 235 said <c>| to X</c>, 273 said <c>| from X</c> and 138 said
/// <c>| A &gt; B</c>; the 285 vanilla titles (<c>Junior Rank - Direct Small
/// Cargo Haul</c>) said nothing. And the objective marker carries an archetype
/// id that spells out the shape and the cargo -
/// <c>RedWind_Pyro_SmallGrade_Solar_CFP_TradepostToStation_Aluminum_CargoHauling_Multi3ToSingle</c>
/// - but never a place.
/// </para>
/// <para>
/// Everything here is read off those two strings and nothing is guessed at:
/// a title that names one end leaves the other null, and an archetype that
/// abbreviates its cargo (<c>ShipAmm_Hydro_Med</c>) gives no commodity at all.
/// </para>
/// </remarks>
public static partial class HaulingContract
{
    /// <summary>
    /// A title the mod left half-filled - 11 acceptances on this install read
    /// literally <c>to Destinationname</c> - is not a place.
    /// </summary>
    private const string Placeholder = "Destinationname";

    /// <summary>The route a title names, or null when it names neither end.</summary>
    public static HaulRoute? RouteFromTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return null;

        // StarStrings wraps its own additions in the game's markup, and the
        // rep bracket sits inside the last segment: "to Ruin Station [50/200
        // Rep]". Both come off before the segments are read.
        var clean = Markup.Replace(Brackets.Replace(title, ""), "").Trim().TrimEnd(':').Trim();

        string? pickup = null;
        string? delivery = null;

        foreach (var raw in clean.Split('|'))
        {
            var segment = raw.Trim();

            if (segment.StartsWith("to ", StringComparison.OrdinalIgnoreCase))
                delivery ??= Place(segment[3..]);
            else if (segment.StartsWith("from ", StringComparison.OrdinalIgnoreCase))
                pickup ??= Place(segment[5..]);
            else if (segment.Contains(" > ", StringComparison.Ordinal))
            {
                var ends = segment.Split(" > ", 2, StringSplitOptions.TrimEntries);
                pickup ??= Place(ends[0]);
                delivery ??= Place(ends[1]);
            }
        }

        var route = new HaulRoute(pickup, delivery);
        return route.Any ? route : null;
    }

    private static string? Place(string value)
    {
        var place = Whitespace.Replace(value, " ").Trim();

        return place.Length == 0 || place.Equals(Placeholder, StringComparison.OrdinalIgnoreCase)
            ? null
            : place;
    }

    /// <summary>Whether an archetype id is a hauling contract at all.</summary>
    /// <remarks>
    /// <c>RecoverCargo</c> is not: those are the Covalex and Ling salvage
    /// bounties, and they carry cargo only in the sense that a fight does.
    /// </remarks>
    public static bool IsHauling(string? archetype) =>
        archetype is not null
        && (archetype.Contains("HaulCargo", StringComparison.OrdinalIgnoreCase)
            || archetype.Contains("CargoHauling", StringComparison.OrdinalIgnoreCase));

    /// <summary>The shape and cargo an archetype id spells out, or null for anything that is not hauling.</summary>
    public static HaulArchetype? FromArchetype(string? archetype)
    {
        if (!IsHauling(archetype))
            return null;

        var tokens = archetype!.Split('_', StringSplitOptions.RemoveEmptyEntries);

        var shape = HaulShape.Unknown;
        int? pickups = null;
        int? deliveries = null;

        foreach (var token in tokens)
        {
            var m = ShapeRegex().Match(token);
            if (!m.Success) continue;

            if (m.Groups["ab"].Success)
                (shape, pickups, deliveries) = (HaulShape.Direct, 1, 1);
            else if (m.Groups["stm"].Success)
                (shape, pickups, deliveries) = (HaulShape.SingleToMulti, 1, Count(m.Groups["n"]));
            else
                (shape, pickups, deliveries) = (HaulShape.MultiToSingle, Count(m.Groups["n"]), 1);

            break;
        }

        return new HaulArchetype(shape, pickups, deliveries, Commodity(tokens));
    }

    private static int? Count(Group digits) =>
        digits.Success && int.TryParse(digits.Value, out var n) ? n : null;

    /// <summary>
    /// The cargo token, in the two places the two issuers put it.
    /// </summary>
    /// <remarks>
    /// Red Wind's ids name it just before <c>CargoHauling</c>; Covalex's
    /// (<c>HaulCargo_…</c>) put a category first - <c>RefinedOre_Aluminium</c>,
    /// <c>Waste_Mixed_ScrapWaste</c> - and the token after the category is the
    /// cargo. The interstellar bulk ids abbreviate everything
    /// (<c>ShipAmm_Hydro_Med</c>) and are left unread rather than expanded.
    /// </remarks>
    private static string? Commodity(string[] tokens)
    {
        for (var i = 1; i < tokens.Length; i++)
        {
            if (tokens[i].Equals("CargoHauling", StringComparison.OrdinalIgnoreCase))
                return Spaced(tokens[i - 1]);
        }

        for (var i = 0; i + 1 < tokens.Length; i++)
        {
            if (!Categories.Contains(tokens[i])) continue;

            var next = tokens[i + 1];

            if (next.Equals("Mixed", StringComparison.OrdinalIgnoreCase))
                return i + 2 < tokens.Length ? $"Mixed: {Spaced(tokens[i + 2])}" : "Mixed";

            return Spaced(next);
        }

        return null;
    }

    private static readonly HashSet<string> Categories = new(StringComparer.OrdinalIgnoreCase)
    {
        "RefinedOre", "RawOre", "NonMetal", "Processed", "Gas", "Waste",
    };

    /// <summary><c>AgriculturalSupplies</c> to <c>Agricultural Supplies</c>; <c>QTFuelHydroFuelShipAmmo</c> stays one word per hump.</summary>
    private static string Spaced(string value) =>
        CamelBoundary().Replace(value, " ").Trim();

    [GeneratedRegex(@"^(?:(?<ab>AToB)|(?<stm>SingleToMulti)(?<n>\d*)|(?<mts>Multi)(?<n>\d*)ToSingle)$", RegexOptions.IgnoreCase)]
    private static partial Regex ShapeRegex();

    [GeneratedRegex(@"(?<=[a-z0-9])(?=[A-Z])")]
    private static partial Regex CamelBoundary();

    private static readonly Regex Markup = new(@"</?EM\d*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Brackets = new(@"\[[^\]]*\]\*?", RegexOptions.Compiled);
    private static readonly Regex Whitespace = new(@"\s{2,}", RegexOptions.Compiled);
}
