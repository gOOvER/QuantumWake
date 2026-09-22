using System.Net;
using System.Text;
using System.Text.Json;
using Quantumwake.Data;

namespace Quantumwake.Tests;

public sealed class JoystickTemplatesTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "qw-templates-" + Guid.NewGuid().ToString("N"));
    private const string Picture = "<svg xmlns=\"http://www.w3.org/2000/svg\"><text>Button_1</text></svg>";
    private string Cache => Path.Combine(root, "templates");

    public JoystickTemplatesTests() => Directory.CreateDirectory(Cache);
    public void Dispose() => Directory.Delete(root, true);

    [Theory]
    [InlineData("../outside.svg")]
    [InlineData("/outside.svg")]
    [InlineData("\\outside.svg")]
    [InlineData("C:\\outside.svg")]
    [InlineData("C:outside.svg")]
    [InlineData("\\\\server\\share\\outside.svg")]
    [InlineData("maker/../outside.svg")]
    [InlineData("maker\\outside.svg")]
    [InlineData("maker//outside.svg")]
    [InlineData("maker./outside.svg")]
    [InlineData("maker /outside.svg")]
    [InlineData("stick.svg:other.svg")]
    [InlineData("outside.txt")]
    public async Task Unsafe_keys_are_rejected_even_if_the_index_lists_them(string relative)
    {
        File.WriteAllText(Path.Combine(Cache, "index.json"), JsonSerializer.Serialize(new[] { relative }));
        var templates = new JoystickTemplates(Cache);
        Assert.Null(await templates.ReadAsync("repo:" + relative, null));
        Assert.Empty(templates.Available());
        templates.SetFolder(root);
        Assert.Null(await templates.ReadAsync("local:" + relative, null));
    }

    [Fact]
    public async Task An_absolute_path_cannot_read_an_existing_file_outside_the_cache()
    {
        var outside = Path.Combine(root, "outside.svg");
        File.WriteAllText(outside, Picture);
        File.WriteAllText(Path.Combine(Cache, "index.json"), JsonSerializer.Serialize(new[] { outside }));
        Assert.Null(await new JoystickTemplates(Cache).ReadAsync("repo:" + outside, null));
    }

    [Fact]
    public async Task Only_indexed_cached_SVGs_are_read_when_the_feed_is_off()
    {
        Directory.CreateDirectory(Path.Combine(Cache, "svg", "Maker"));
        const string relative = "Maker/X52_H.O.T.A.S..svg";
        File.WriteAllText(Path.Combine(Cache, "svg", relative), Picture);
        var templates = new JoystickTemplates(Cache);
        Assert.Null(await templates.ReadAsync("repo:" + relative, null));
        File.WriteAllText(Path.Combine(Cache, "index.json"), JsonSerializer.Serialize(new[] { relative }));
        Assert.False(templates.Enabled);
        Assert.Contains("Button_1", Encoding.UTF8.GetString((await templates.ReadAsync("repo:" + relative, null))!));
    }

    [Fact]
    public async Task Local_templates_must_be_SVG_and_are_cleaned_on_every_read()
    {
        var templates = new JoystickTemplates(Cache);
        templates.SetFolder(root);
        File.WriteAllText(Path.Combine(root, "private.txt"), "private");
        Assert.Null(await templates.ReadAsync("local:private.txt", null));
        File.WriteAllText(Path.Combine(root, "stick.svg"), Picture.Replace("<text>", "<text onclick=\"evil()\">"));
        var result = Encoding.UTF8.GetString((await templates.ReadAsync("local:stick.svg", null))!);
        Assert.Contains("Button_1", result);
        Assert.DoesNotContain("onclick", result);
    }

    [Fact]
    public async Task Downloaded_and_previously_cached_templates_are_both_cleaned()
    {
        using var feed = new Feed();
        using var http = new HttpClient(feed);
        var templates = new JoystickTemplates(Cache);
        Assert.Equal(1, await templates.EnableAsync(http));
        var first = await templates.ReadAsync("repo:Maker/stick.svg", http);
        Assert.DoesNotContain("onload", Encoding.UTF8.GetString(first!));
        templates.Disable();
        File.WriteAllText(Path.Combine(Cache, "svg", "Maker", "stick.svg"), Picture.Replace("<text>", "<text onload=\"evil()\">"));
        var cached = await templates.ReadAsync("repo:Maker/stick.svg", null);
        Assert.DoesNotContain("onload", Encoding.UTF8.GetString(cached!));
        Assert.Contains("Button_1", Encoding.UTF8.GetString(cached!));
        Assert.Equal(2, feed.Requests);
    }

    private sealed class Feed : HttpMessageHandler
    {
        public int Requests { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancel)
        {
            Requests++;
            var content = request.RequestUri!.AbsoluteUri == JoystickTemplates.TreeUrl
                ? """{"tree":[{"type":"blob","path":"templates/Maker/stick.svg"}]}"""
                : Picture.Replace("<text>", "<text onload=\"evil()\">");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) });
        }
    }
}
