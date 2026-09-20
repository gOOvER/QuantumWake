using Quantumwake.Core.GameData;

namespace Quantumwake.Tests;

public class ShoppingAvailabilityTests
{
    [Theory]
    [InlineData("garage:ORIG_315p", false)]
    [InlineData("Garage:AEGS_Gladius", false)]
    [InlineData("wikelo:trade", true)]
    [InlineData(null, true)]
    public void Only_a_garage_list_requires_evidence_of_a_loose_part(string? source, bool counts) =>
        Assert.Equal(counts, ShoppingAvailability.CountsInventorySighting(source));
}
