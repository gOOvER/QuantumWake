using Quantumwake.Data;
using System.Net;
using System.Text;

namespace Quantumwake.Tests;

/// <summary>
/// The player marketplace feed: UEX's newest advertisements, each joined to
/// the item it names so the Garage bench can put a seller beside a part.
/// </summary>
/// <remarks>
/// The join is the whole point and the whole difficulty. A listing names its
/// item by UEX's own integer id, and the price feed the app already holds
/// resolves that for 26 of 441 - what players advertise is what no shop
/// stocks. So the item table is read a category at a time, only for the
/// categories advertised, and kept between fetches.
/// </remarks>
public class UexMarketplaceTests : IDisposable
{
    private readonly string _directory =
        Path.Combine(Path.GetTempPath(), $"qw-market-{Guid.NewGuid():N}");

    private UexFeeds NewFeeds() => new(_directory);

    private const string Listings = """
        {"status":"ok","data":[
          {"id":172424,"id_category":19,"id_item":1772,"operation":"sell","type":"item","slug":"quadracell-mt-0Y9jLAxXhB",
           "title":"QuadraCell MT 900Q (3pip)","unit":"unit","price":"8800000","currency":"UEC","location":"Admin - Ruin Station",
           "quality":900,"in_stock":1,"is_sold_out":0,"date_added":1789484274,"date_expiration":1794668274,
           "user_name":"penetrator3000","user_username":"penetrator3000",
           "photos":"https://cdn.uexcorp.space/uid1/marketplace/listings/172424/a-t.jpg,https://cdn.uexcorp.space/uid1/marketplace/listings/172424/b-t.jpg,"},
          {"id":172419,"id_category":19,"id_item":1772,"operation":"sell","type":"item","slug":"quadracell-cheap-f8dfO1KXji",
           "title":"QuadraCell MT","unit":"unit","price":"6000000","currency":"UEC","location":"Levski",
           "quality":0,"in_stock":2,"is_sold_out":0,"date_added":1789481347,"date_expiration":0,
           "user_name":"kaltpfote","user_username":"Kaltpfote","photos":""},
          {"id":172410,"id_category":19,"id_item":1772,"operation":"sell","type":"item","slug":"quadracell-gone-aaaaaaaaaa",
           "title":"QuadraCell MT","unit":"unit","price":"1","currency":"UEC","location":"",
           "in_stock":0,"is_sold_out":1,"date_added":1789481000,"user_username":"gone","photos":""},
          {"id":172400,"id_category":19,"id_item":1772,"operation":"buy","type":"item","slug":"wtb-quadracell-bbbbbbbbbb",
           "title":"WTB QuadraCell","unit":"unit","price":"5000000","currency":"UEC","location":"Orison",
           "in_stock":0,"is_sold_out":0,"date_added":1789480000,"user_username":"buyer","photos":""},
          {"id":172390,"id_category":41,"id_item":0,"operation":"sell","type":"service","slug":"mining-escort-cccccccccc",
           "title":"Mining escort","unit":"hour","price":"250000","currency":"UEC","location":"Stanton",
           "in_stock":0,"is_sold_out":0,"date_added":1789479000,"user_username":"escort","photos":""}
        ]}
        """;

    private const string Coolers = """
        {"status":"ok","data":[
          {"id":1772,"id_category":19,"name":"QuadraCell MT","section":"Systems","category":"Coolers","slug":"quadracell-mt","uuid":"9a0d3b4e-0000-4000-8000-000000001772"},
          {"id":1773,"id_category":19,"name":"Bracer","section":"Systems","category":"Coolers","slug":"bracer","uuid":null}
        ]}
        """;

    private const string Categories = """
        {"status":"ok","data":[
          {"id":19,"type":"item","section":"Systems","name":"Coolers"},
          {"id":41,"type":"service","section":"General","name":"Mining"}
        ]}
        """;

    /// <summary>A UEX that answers by url and counts what it was asked, so the item table reads can be watched.</summary>
    private sealed class Uex(string listings) : HttpMessageHandler
    {
        public readonly List<string> Requests = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            var url = request.RequestUri!.ToString();
            Requests.Add(url);

            var body = url.StartsWith(UexFeeds.ListingsUrl, StringComparison.Ordinal) ? listings
                : url.StartsWith(UexFeeds.CategoriesUrl, StringComparison.Ordinal) ? Categories
                : url == UexFeeds.ItemsUrl + "19" ? Coolers
                : "{\"status\":\"ok\",\"data\":[]}";

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
        }
    }

    [Fact]
    public async Task Listings_are_joined_to_the_item_they_name_through_uex_s_item_table()
    {
        var uex = new Uex(Listings);
        var feeds = NewFeeds();

        var count = await feeds.EnableAsync(UexFeeds.Marketplace, new HttpClient(uex));

        Assert.Equal(5, count);
        // The listings, the category names, and the one category advertised
        // with an item the table did not hold. The service's category is
        // never read: it names no item.
        Assert.Equal([UexFeeds.ListingsUrl, UexFeeds.CategoriesUrl, UexFeeds.ItemsUrl + "19"], uex.Requests);

        var dear = Assert.Single(feeds.Listings, l => l.Id == 172424);
        Assert.Equal("9a0d3b4e-0000-4000-8000-000000001772", dear.ItemUuid);
        Assert.Equal("QuadraCell MT", dear.ItemName);
        Assert.Equal("Coolers", dear.Category);
        Assert.Equal("Systems", dear.Section);
        Assert.Equal(8_800_000m, dear.Price);
        Assert.Equal(900, dear.Quality);
        Assert.Equal("penetrator3000", dear.Seller);
        Assert.Equal("Admin - Ruin Station", dear.Location);
        Assert.Equal("https://cdn.uexcorp.space/uid1/marketplace/listings/172424/a-t.jpg", dear.Photo);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1789484274), dear.Added);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1794668274), dear.Expires);
        Assert.Equal("https://uexcorp.space/marketplace/item/info/quadracell-mt-0Y9jLAxXhB/", dear.Url);

        // Blank quality, no expiry and no photo stay blank rather than zero or "".
        var cheap = Assert.Single(feeds.Listings, l => l.Id == 172419);
        Assert.Null(cheap.Quality);
        Assert.Null(cheap.Expires);
        Assert.Null(cheap.Photo);

        // A service names no item; its category is still named from the list.
        var service = Assert.Single(feeds.Listings, l => l.Type == "service");
        Assert.Null(service.ItemUuid);
        Assert.Equal("Mining", service.Category);
    }

    /// <summary>
    /// What the bench asks: who is selling this part, cheapest first. A buy
    /// request is not an offer and a sold-out listing is not stock, so neither
    /// is in the answer, and the answer is by the game's uuid.
    /// </summary>
    [Fact]
    public async Task Sellers_of_a_part_come_cheapest_first_without_buyers_or_sold_out_stock()
    {
        var feeds = NewFeeds();
        await feeds.EnableAsync(UexFeeds.Marketplace, new HttpClient(new Uex(Listings)));

        var sellers = feeds.ListingsFor("9A0D3B4E-0000-4000-8000-000000001772");

        Assert.Equal([172419, 172424], sellers.Select(l => l.Id));
        Assert.Empty(feeds.ListingsFor(null));
        Assert.Empty(feeds.ListingsFor("nobody"));
    }

    /// <summary>
    /// The item table is kept: a second fetch of the listings reads UEX's
    /// items again only for a category whose advertised item it lacks, and
    /// dropping the feed does not throw the table away.
    /// </summary>
    [Fact]
    public async Task The_item_table_is_kept_between_fetches_and_read_only_for_what_is_missing()
    {
        var feeds = NewFeeds();
        await feeds.EnableAsync(UexFeeds.Marketplace, new HttpClient(new Uex(Listings)));

        var again = new Uex(Listings);
        await feeds.EnableAsync(UexFeeds.Marketplace, new HttpClient(again));
        Assert.Equal([UexFeeds.ListingsUrl], again.Requests);

        feeds.Disable(UexFeeds.Marketplace);
        Assert.Empty(feeds.Listings);
        Assert.False(feeds.IsEnabled(UexFeeds.Marketplace));

        // Off and on again, in a fresh instance: the table was on disk.
        var fresh = new Uex(Listings);
        await NewFeeds().EnableAsync(UexFeeds.Marketplace, new HttpClient(fresh));
        Assert.Equal([UexFeeds.ListingsUrl], fresh.Requests);

        // A listing naming an id the table lacks, in a category read moments
        // ago: not read again - the id is newer than the table and asking
        // again now gets the same answer.
        var newer = new Uex(Listings.Replace("\"id_item\":1772,\"operation\":\"buy\"", "\"id_item\":1799,\"operation\":\"buy\""));
        await NewFeeds().EnableAsync(UexFeeds.Marketplace, new HttpClient(newer));
        Assert.Equal([UexFeeds.ListingsUrl], newer.Requests);
    }

    /// <summary>A listing with no item behind it still stands: it is the seller's advertisement, joined or not.</summary>
    [Fact]
    public async Task A_listing_naming_an_unknown_item_keeps_its_own_title()
    {
        var feeds = NewFeeds();
        await feeds.EnableAsync(UexFeeds.Marketplace, new HttpClient(new Uex(
            Listings.Replace("\"id_item\":1772,\"operation\":\"sell\",\"type\":\"item\",\"slug\":\"quadracell-mt-0Y9jLAxXhB\"",
                "\"id_item\":4242,\"operation\":\"sell\",\"type\":\"item\",\"slug\":\"quadracell-mt-0Y9jLAxXhB\""))));

        var orphan = Assert.Single(feeds.Listings, l => l.Id == 172424);
        Assert.Equal("QuadraCell MT 900Q (3pip)", orphan.Title);
        Assert.Null(orphan.ItemUuid);
        Assert.Null(orphan.ItemName);
        Assert.Equal("Coolers", orphan.Category);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        try { if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true); }
        catch (IOException) { }
    }
}
