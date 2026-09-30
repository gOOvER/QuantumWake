using Quantumwake.Core.State;

namespace Quantumwake.Tests;

/// <summary>
/// Party toasts are the only lines in a 4.9 log that name another player, and
/// they arrive as free text on a channel that also carries chatter naming
/// nobody. The risk is not failing to read one - it is reading a word out of a
/// sentence and calling it a person.
/// </summary>
public class PartyTests
{
    private static readonly DateTimeOffset At =
        new(2026, 8, 23, 3, 14, 21, TimeSpan.Zero);

    /// <summary>
    /// The text as it reaches the parser: the game prints the title and the body
    /// on separate lines, and LogFileReader joins them with a space, leaving the
    /// title glued to the front and the notification's colon on the end.
    /// </summary>
    [Theory]
    [InlineData("Party B-Kon connected.:", "B-Kon", PartyMoment.Connected)]
    [InlineData("Party B-Kon disconnected.:", "B-Kon", PartyMoment.Disconnected)]
    [InlineData("Party Echoes-of-Cartography connected.:", "Echoes-of-Cartography", PartyMoment.Connected)]
    [InlineData("Party nova_rime connected.:", "nova_rime", PartyMoment.Connected)]
    [InlineData("Party TX207 disconnected.:", "TX207", PartyMoment.Disconnected)]
    [InlineData("New Party Leader Brenner is now party leader.:", "Brenner", PartyMoment.BecameLeader)]
    [InlineData("New Member Joined Quorren-7 has joined the party.:", "Quorren-7", PartyMoment.Joined)]
    [InlineData("New Member Joined MarcusHollowell-TzqfPwbe has joined the party.:",
        "MarcusHollowell-TzqfPwbe", PartyMoment.Joined)]
    [InlineData("Member Left Velaris has left the party.:", "Velaris", PartyMoment.Left)]
    [InlineData("Member Left bkonz has left the party.:", "bkonz", PartyMoment.Left)]
    public void Reads_who_and_what_happened(string text, string handle, PartyMoment moment)
    {
        var note = Party.Read(At, text);

        Assert.NotNull(note);
        Assert.Equal(handle, note.Handle);
        Assert.Equal(moment, note.Moment);
        Assert.Equal(At, note.At);
    }

    /// <summary>
    /// Disbanding names nobody, and must not invent somebody. Note the title:
    /// this one is "Party Disbanded", not "Party", so the body arrives as
    /// "Party Disbanded The party has been disbanded." - which is why the title
    /// is what gets matched.
    /// </summary>
    [Fact]
    public void A_disbanded_party_has_no_handle()
    {
        var note = Party.Read(At, "Party Disbanded The party has been disbanded.:");

        Assert.NotNull(note);
        Assert.Null(note.Handle);
        Assert.Equal(PartyMoment.Disbanded, note.Moment);
    }

    /// <summary>
    /// The rest of the party channel. None of these say who is present, and
    /// each would yield a plausible-looking handle to a looser reader - "The",
    /// "Initiated", "Join". The mojibake is real: one file in this corpus has a
    /// line where the game wrote a half-decoded string.
    /// </summary>
    [Theory]
    [InlineData("Party Launch Initiated by party leader TX207.:")]
    [InlineData("Party Launch Join queue canceled by party leader TX207.:")]
    [InlineData("Party Launch Accepted Initiated by party leader TX207.:")]
    [InlineData("Party Notifications sent to party members.:")]

    [InlineData("New Member Joined")]
    [InlineData("Member Left")]
    [InlineData("Party ��潍楳s���:")]
    [InlineData("Party :")]
    [InlineData("Party")]
    public void Chatter_that_names_nobody_is_left_unread(string text)
    {
        Assert.Null(Party.Read(At, text));
    }

    /// <summary>
    /// A handle is one unbroken word. Without that rule any sentence ending in
    /// the right verb becomes a player - and the party channel is full of
    /// sentences.
    /// </summary>
    [Fact]
    public void A_sentence_ending_the_right_way_is_still_not_a_handle()
    {
        Assert.Null(Party.Read(At, "Party Everyone in the group connected.:"));
        Assert.Null(Party.Read(At, "New Party Leader nobody at all is now party leader.:"));
    }

    /// <summary>Notifications from every other channel are not ours to read.</summary>
    [Theory]
    [InlineData("Contract Accepted: Recover Cargo")]
    [InlineData("Medical Bed: You have been healed.")]
    [InlineData("Incapacitated: While incapacitated, ask others in your party...")]
    public void Only_the_party_channel_counts(string text)
    {
        Assert.False(Party.IsParty(text));
        Assert.Null(Party.Read(At, text));
    }

    /// <summary>
    /// "Incapacitated" mentions the word party in its body, which is exactly the
    /// kind of thing a substring check would swallow.
    /// </summary>
    [Fact]
    public void The_channel_is_matched_at_the_front_not_anywhere()
    {
        Assert.False(Party.IsParty(
            "Incapacitated: While incapacitated, ask others in your party, in chat, "
            + "or through rescue service beacons to revive you."));
    }
    /// <summary>
    /// A ship's comms channel emptying is not a party event, and must not even
    /// be counted as one.
    /// </summary>
    /// <remarks>
    /// The game gives it the same "Member Left" title, and 27 of this install's
    /// lines under that title are channels rather than parties. Read refuses
    /// them on their body either way; IsParty refuses them too, so the count of
    /// party notifications keeps meaning what it says and the gap between it and
    /// the notes read stays a measure of what the reader declined to guess at.
    /// </remarks>
    [Theory]
    [InlineData("Member Left Velaris has left the channel 'RSI Ursa Medivac : NightWardeno1'.:")]
    [InlineData("Member Left B-Kon has left the channel 'MISC Starlancer MAX : nekron'.:")]
    [InlineData("Member Left  has left the channel 'RSI Ursa Medivac : NightWardeno1'.:")]
    public void A_ship_channel_emptying_is_not_a_party_event(string text)
    {
        Assert.False(Party.IsParty(text));
        Assert.Null(Party.Read(At, text));
    }

    /// <summary>
    /// Leaving and dropping are different facts, and the one that matters is
    /// the one that means somebody is not coming back.
    /// </summary>
    [Fact]
    public void Leaving_is_not_the_same_as_dropping()
    {
        var left = Party.Read(At, "Member Left Velaris has left the party.:");
        var dropped = Party.Read(At, "Party Velaris disconnected.:");

        Assert.Equal(PartyMoment.Left, left!.Moment);
        Assert.Equal(PartyMoment.Disconnected, dropped!.Moment);
        Assert.Equal("Velaris", left.Handle);
        Assert.Equal(left.Handle, dropped.Handle);
    }

    /// <summary>
    /// Somebody who dropped and came back is present. A tally of arrivals
    /// against departures would score that as a draw and show them as gone,
    /// which is why the reduction keeps the last word rather than counting.
    /// </summary>
    [Fact]
    public void Latest_keeps_the_last_word_about_each_player()
    {
        var latest = Party.Latest([
            new PartyNote(At, "B-Kon", PartyMoment.Connected),
            new PartyNote(At.AddMinutes(5), "B-Kon", PartyMoment.Disconnected),
            new PartyNote(At.AddMinutes(9), "B-Kon", PartyMoment.Connected),
            new PartyNote(At.AddMinutes(2), "Velaris", PartyMoment.Joined),
        ]);

        Assert.Equal(2, latest.Count);

        // Most recent first, so the card leads with what just happened.
        Assert.Equal("B-Kon", latest[0].Handle);
        Assert.Equal(PartyMoment.Connected, latest[0].Moment);
        Assert.Equal(At.AddMinutes(9), latest[0].At);
        Assert.Equal("Velaris", latest[1].Handle);
    }

    /// <summary>
    /// Disbanding names nobody, so it cannot become a row with an empty handle.
    /// </summary>
    [Fact]
    public void Latest_drops_the_note_that_names_nobody()
    {
        var latest = Party.Latest([
            new PartyNote(At, "B-Kon", PartyMoment.Connected),
            new PartyNote(At.AddMinutes(1), null, PartyMoment.Disbanded),
        ]);

        Assert.Equal("B-Kon", Assert.Single(latest).Handle);
    }

}
