using System.Net;

namespace Quantumwake.Tests;

/// <summary>
/// The bare server has no tray app to open MFD setup in, and says so rather
/// than answering as if a window had opened.
/// </summary>
[Collection("server")]
public class MfdSetupEndpointTests : IClassFixture<ServerUnderTest>
{
    private readonly ServerUnderTest _server;

    public MfdSetupEndpointTests(ServerUnderTest server) => _server = server;

    [Fact]
    public async Task Without_the_app_there_is_nothing_to_open()
    {
        var response = await _server.Post("/api/mfd/setup");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("Quantum Wake app", await response.Content.ReadAsStringAsync());
    }
}
