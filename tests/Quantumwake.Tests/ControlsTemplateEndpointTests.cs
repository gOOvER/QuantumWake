using System.Net;
using System.Text.Json;

namespace Quantumwake.Tests;

[Collection("server")]
public class ControlsTemplateEndpointTests(ServerUnderTest server) : IClassFixture<ServerUnderTest>
{
    [Fact]
    public async Task Rooted_template_keys_cannot_return_a_local_file()
    {
        var outside = Path.Combine(server.DataDirectory, "outside.svg");
        File.WriteAllText(outside, "private test marker");
        using var response = await server.Client.GetAsync("/api/controls/template?key=" + Uri.EscapeDataString("repo:" + outside));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain("private test marker", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Cached_templates_are_cleaned_and_direct_navigation_is_sandboxed()
    {
        var templates = Path.Combine(server.DataDirectory, "controls", "templates");
        Directory.CreateDirectory(Path.Combine(templates, "svg"));
        File.WriteAllText(Path.Combine(templates, "index.json"), JsonSerializer.Serialize(new[] { "stick.svg" }));
        File.WriteAllText(Path.Combine(templates, "svg", "stick.svg"), """
            <svg xmlns="http://www.w3.org/2000/svg" onload="evil()"><text>Button_1</text><script>evil()</script></svg>
            """);
        using var response = await server.Client.GetAsync("/api/controls/template?key=repo:stick.svg");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/svg+xml", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("sandbox", response.Headers.GetValues("Content-Security-Policy").Single());
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.True(response.Headers.CacheControl?.NoStore);
        var svg = await response.Content.ReadAsStringAsync();
        Assert.Contains("Button_1", svg);
        Assert.DoesNotContain("evil", svg);
    }
}
