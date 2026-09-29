using Quantumwake.Core.Locations;
using Quantumwake.Core.State;
using Quantumwake.Data;

namespace Quantumwake.Tests;

/// <summary>
/// The atlas is kept until something it is made of changes.
/// </summary>
/// <remarks>
/// Every terminal lookup reads the atlas, and the map's services resolve every
/// known terminal, so an atlas rebuilt on each call made the map wait half a
/// minute. The risk in keeping it is the opposite one - a place visited today,
/// or a wipe line moved, that the map does not show until a restart - so both
/// halves are pinned here.
/// </remarks>
public class AtlasCacheTests : IDisposable
{
    private static readonly DateTimeOffset At =
        new(2026, 9, 20, 18, 0, 0, TimeSpan.Zero);

    private readonly SessionStore _store = new(":memory:");
    private readonly LogLibrary _library;

    public AtlasCacheTests() => _library = new LogLibrary(_store);

    private void Visit(string session, DateTimeOffset at, string rawId, string name) =>
        _store.Save(
            new SessionSummary
            {
                Id = session,
                SourceFile = $"{session}.log",
                StartedAt = at,
                EndedAt = at.AddHours(1),
                Handle = "nekron",
                Locations = [new LocationVisit(at, rawId, name, "Stanton", "Hurston", LocationKind.City)],
            },
            $"fingerprint:{session}");

    [Fact]
    public void An_unchanged_library_answers_from_the_same_atlas()
    {
        Visit("s1", At, "Stanton1_Lorville", "Lorville");

        Assert.Same(_library.Atlas(), _library.Atlas());
        Assert.Same(_library.Terminals, _library.Terminals);
    }

    [Fact]
    public void A_newly_saved_visit_reaches_the_next_atlas()
    {
        Visit("s1", At, "Stanton1_Lorville", "Lorville");
        Assert.Single(_library.Atlas());

        Visit("s2", At.AddDays(1), "Stanton4_Levski", "Levski");

        Assert.Equal(2, _library.Atlas().Count);
    }

    [Fact]
    public void Moving_the_wipe_line_redraws_the_atlas()
    {
        Visit("s1", At, "Stanton1_Lorville", "Lorville");
        Visit("s2", At.AddDays(2), "Stanton4_Levski", "Levski");
        Assert.Equal(2, _library.Atlas().Count);

        _library.Wipe = new Wipe(At.AddDays(1), "Alpha 4.10");

        var place = Assert.Single(_library.Atlas());
        Assert.Equal("Stanton4_Levski", place.RawId);
    }

    [Fact]
    public void Clearing_the_store_empties_the_atlas()
    {
        Visit("s1", At, "Stanton1_Lorville", "Lorville");
        Assert.Single(_library.Atlas());

        _store.Clear();

        Assert.Empty(_library.Atlas());
    }

    public void Dispose() => _store.Dispose();
}
