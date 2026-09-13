using Quantumwake.Server;

namespace Quantumwake.Tests;

/// <summary>
/// What counts as somewhere to land, in the words the game map uses. The
/// cached map on this install names them two ways: stations carry
/// "Landing Pad M" beside "Hangar L", and the four Stanton cities carry only
/// "Hangar XL" - 6 of those, 178 "Hangar L", 110 "Landing Pad M".
/// </summary>
public class RouteEndsTests
{
    [Theory]
    [InlineData("Hangar XL", true)]
    [InlineData("Hangar L", true)]
    [InlineData("Hangar", true)]
    [InlineData("Landing Pad M", true)]
    [InlineData("Refinery", false)]
    [InlineData("Repair", false)]
    public void A_hangar_is_somewhere_to_land(string amenity, bool landing)
    {
        Assert.Equal(landing, RouteEnds.IsLandingSpot(amenity));
    }

    /// <summary>
    /// A city with only "Hangar XL" used to read as having no pad record and
    /// was dropped by both the known-pads and the XL filters - so a Hermes
    /// pilot asking for XL at both ends was told nothing qualified, with
    /// every major city in the list.
    /// </summary>
    [Fact]
    public void A_city_with_an_xl_hangar_passes_the_xl_and_known_pad_filters()
    {
        string[] city = ["Hangar XL"];
        string[] station = ["Landing Pad M", "Hangar L"];

        Assert.True(RouteEnds.MatchesPads(city, city, "xl"));
        Assert.True(RouteEnds.MatchesPads(city, station, "known"));
        Assert.False(RouteEnds.MatchesPads(city, station, "xl"));
        Assert.False(RouteEnds.MatchesPads(city, [], "known"));
        Assert.True(RouteEnds.MatchesPads([], [], "any"));
    }
}
