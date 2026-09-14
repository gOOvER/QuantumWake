using Quantumwake.Core.GameData;
using Quantumwake.Data;
using System.Net;
using System.Net.Http.Headers;

namespace Quantumwake.Tests;

/// <summary>
/// The wiki's picture of a part: asked for once, kept, and a miss remembered.
/// </summary>
public class PartPicturesTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"qw-pictures-{Guid.NewGuid():N}");
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 1, 2, 3];

    /// <summary>A wiki that has a picture for the titles it is given, and counts what it was asked.</summary>
    private sealed class Wiki(IReadOnlyDictionary<string, string> pictures) : HttpMessageHandler
    {
        public readonly List<string> Requests = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            var url = request.RequestUri!.ToString();
            Requests.Add(url);

            if (url.StartsWith(PartPictures.WikiApi, StringComparison.Ordinal))
            {
                var asked = WebUtility.UrlDecode(url[(url.IndexOf("titles=", StringComparison.Ordinal) + 7)..]).Split('|');
                var pages = asked.Select((t, i) => pictures.TryGetValue(t, out var src)
                    ? $"\"{i + 1}\":{{\"pageid\":{i + 1},\"title\":\"{t}\",\"thumbnail\":{{\"source\":\"{src}\",\"width\":240,\"height\":180}}}}"
                    : $"\"-{i + 1}\":{{\"title\":\"{t}\",\"missing\":\"\"}}");
                var body = "{\"batchcomplete\":\"\",\"query\":{\"pages\":{" + string.Join(',', pages) + "}}}";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) });
            }

            var content = new ByteArrayContent(Png);
            content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }
    }

    private static PartStats Part(string name, string uuid) =>
        new("X", "Cooler", 1, 2, name, "Aegis Dynamics", uuid, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, null, null, null, null, null);

    [Fact]
    public async Task A_picture_is_fetched_once_and_read_from_disk_after()
    {
        var wiki = new Wiki(new Dictionary<string, string> { ["Glacier"] = "https://media.starcitizen.tools/thumb/glacier.png" });
        using var http = new HttpClient(wiki);

        var first = await new PartPictures(_root).GetAsync(http, Part("Glacier", "u1"));
        Assert.NotNull(first);
        Assert.Equal("image/png", first.ContentType);
        Assert.Equal(Png, first.Bytes);
        Assert.Equal(2, wiki.Requests.Count);

        // A fresh instance - a restart - finds the file and asks nothing.
        var again = await new PartPictures(_root).GetAsync(http, Part("Glacier", "u1"));
        Assert.NotNull(again);
        Assert.Equal(2, wiki.Requests.Count);
    }

    /// <summary>The dump's 5MA 'Chimalli' is the wiki's Chimalli; the quoted model is the second guess.</summary>
    [Fact]
    public async Task A_quoted_model_name_is_tried_when_the_full_name_is_not_a_page()
    {
        var wiki = new Wiki(new Dictionary<string, string> { ["Chimalli"] = "https://media.starcitizen.tools/thumb/chimalli.jpg" });
        using var http = new HttpClient(wiki);

        var picture = await new PartPictures(_root).GetAsync(http, Part("5MA 'Chimalli'", "u2"));

        Assert.NotNull(picture);
        Assert.EndsWith("titles=5MA 'Chimalli'|Chimalli", WebUtility.UrlDecode(wiki.Requests[0]));
        Assert.Equal(["5MA 'Chimalli'", "Chimalli"], PartPictures.Titles("5MA 'Chimalli'"));
        Assert.Equal(["FR-66"], PartPictures.Titles("FR-66"));
    }

    [Fact]
    public async Task A_part_the_wiki_has_no_picture_of_is_not_asked_about_again()
    {
        var wiki = new Wiki(new Dictionary<string, string>());
        using var http = new HttpClient(wiki);

        Assert.Null(await new PartPictures(_root).GetAsync(http, Part("FR-66", "u3")));
        Assert.Single(wiki.Requests);

        Assert.Null(await new PartPictures(_root).GetAsync(http, Part("FR-66", "u3")));
        Assert.Single(wiki.Requests);
    }

    [Fact]
    public async Task A_part_without_a_uuid_or_a_name_is_never_asked_about()
    {
        var wiki = new Wiki(new Dictionary<string, string>());
        using var http = new HttpClient(wiki);

        Assert.Null(await new PartPictures(_root).GetAsync(http, Part("Glacier", "")));
        Assert.Null(await new PartPictures(_root).GetAsync(http, Part(" ", "u4")));
        Assert.Empty(wiki.Requests);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        try { Directory.Delete(_root, true); } catch (IOException) { }
    }
}
