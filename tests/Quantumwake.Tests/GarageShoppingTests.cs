using Quantumwake.Core.GameData;

namespace Quantumwake.Tests;

/// <summary>
/// The one stop proposed for a bench's worth of parts.
/// </summary>
public class GarageShoppingTests
{
    private static ShoppingLine Line(string part, int count, params (string Terminal, decimal Price)[] shops) => new(part, count, shops);

    /// <summary>
    /// Three of four parts at one counter beats two of them cheaper at another:
    /// the fourth is a second trip either way, the other two are not.
    /// </summary>
    [Fact]
    public void Coverage_beats_price()
    {
        var proposal = GarageShopping.Propose(
        [
            Line("Glacier", 2, ("Dumper's Depot", 12000m), ("Cousin Crow's", 9000m)),
            Line("NightFall", 1, ("Dumper's Depot", 40000m)),
            Line("FR-66", 1, ("Dumper's Depot", 30000m), ("Cousin Crow's", 20000m)),
            Line("Beacon", 1, ("Centermass", 5000m)),
        ]);

        Assert.Equal("Dumper's Depot", proposal.Terminal);
        Assert.Equal(3, proposal.Covered);
        Assert.Equal(4, proposal.Of);
        Assert.Equal(2 * 12000m + 40000m + 30000m, proposal.Total);
        Assert.Equal(["Beacon"], proposal.Missing);
    }

    [Fact]
    public void Equal_coverage_goes_to_the_cheaper_total_counts_included()
    {
        var proposal = GarageShopping.Propose(
        [
            Line("Glacier", 3, ("Dumper's Depot", 12000m), ("Cousin Crow's", 11000m)),
            Line("FR-66", 1, ("Dumper's Depot", 20000m), ("Cousin Crow's", 22000m)),
        ]);

        // 3 × 11,000 + 22,000 = 55,000 against 3 × 12,000 + 20,000 = 56,000.
        Assert.Equal("Cousin Crow's", proposal.Terminal);
        Assert.Equal(55000m, proposal.Total);
        Assert.Empty(proposal.Missing);
    }

    /// <summary>A part nobody sells is still on the list; the proposal just says so.</summary>
    [Fact]
    public void Nothing_sold_anywhere_is_a_list_without_a_stop()
    {
        var proposal = GarageShopping.Propose([Line("Bracer", 1), Line("Endo", 2)]);

        Assert.Null(proposal.Terminal);
        Assert.Equal(0, proposal.Covered);
        Assert.Equal(2, proposal.Of);
        Assert.Equal(["Bracer", "Endo"], proposal.Missing);
    }

    [Fact]
    public void An_empty_list_proposes_nothing()
    {
        var proposal = GarageShopping.Propose([]);
        Assert.Null(proposal.Terminal);
        Assert.Equal(0, proposal.Of);
    }

    /// <summary>The same terminal quoted twice for one part is one row at its best price, not double coverage.</summary>
    [Fact]
    public void A_terminal_quoted_twice_counts_once_at_its_best_price()
    {
        var proposal = GarageShopping.Propose([Line("Glacier", 1, ("Dumper's Depot", 12400m), ("dumper's depot", 12000m))]);

        Assert.Equal(1, proposal.Covered);
        Assert.Equal(12000m, proposal.Total);
    }
}
