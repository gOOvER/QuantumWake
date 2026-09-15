namespace Quantumwake.Core.GameData;

/// <summary>One part the pilot means to buy, and where it is sold.</summary>
/// <param name="Count">How many of it: two coolers is one line with a two on it.</param>
/// <param name="Shops">Terminal and the price there, one row per terminal.</param>
public sealed record ShoppingLine(string Part, int Count, IReadOnlyList<(string Terminal, decimal Price)> Shops);

/// <summary>Where to go for the list, and how much of it that place has.</summary>
/// <param name="Terminal">Null when nothing on the list is sold anywhere known.</param>
/// <param name="Covered">Lines the terminal sells.</param>
/// <param name="Total">What the covered lines cost there, counts included.</param>
/// <param name="Missing">Lines the terminal does not sell, by part name.</param>
public sealed record ShoppingProposal(string? Terminal, int Covered, int Of, decimal Total, IReadOnlyList<string> Missing);

/// <summary>
/// Picks the one stop for a list of parts.
/// </summary>
/// <remarks>
/// The most of the list first, the cheapest total second. A plan is usually
/// written with one trip in mind, and a counter that has four of five parts
/// beats one that has two of them cheaper - the fifth is a second stop either
/// way. Price breaks the tie because two counters with the same coverage are
/// the same trip.
/// </remarks>
public static class GarageShopping
{
    public static ShoppingProposal Propose(IReadOnlyList<ShoppingLine> lines)
    {
        if (lines.Count == 0)
            return new ShoppingProposal(null, 0, 0, 0, []);

        var byTerminal = new Dictionary<string, (int Covered, decimal Total)>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            // One row per terminal per line, at the best price it quotes.
            foreach (var shop in line.Shops.GroupBy(s => s.Terminal, StringComparer.OrdinalIgnoreCase))
            {
                var price = shop.Min(s => s.Price);
                var so = byTerminal.GetValueOrDefault(shop.Key);
                byTerminal[shop.Key] = (so.Covered + 1, so.Total + price * line.Count);
            }
        }

        if (byTerminal.Count == 0)
            return new ShoppingProposal(null, 0, lines.Count, 0, [.. lines.Select(l => l.Part)]);

        var best = byTerminal
            .OrderByDescending(t => t.Value.Covered)
            .ThenBy(t => t.Value.Total)
            .ThenBy(t => t.Key, StringComparer.OrdinalIgnoreCase)
            .First();

        var missing = lines
            .Where(l => !l.Shops.Any(s => s.Terminal.Equals(best.Key, StringComparison.OrdinalIgnoreCase)))
            .Select(l => l.Part)
            .ToList();

        return new ShoppingProposal(best.Key, best.Value.Covered, lines.Count, best.Value.Total, missing);
    }
}
