using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Quantumwake.Core.State;

/// <summary>
/// A shard name taken apart: <c>pub_use1b_12545750_150</c> is environment,
/// region, server deployment and shard number.
/// </summary>
/// <remarks>
/// <para>
/// The deployment is the part that matters for anything a pilot writes down. A
/// shard lives only as long as the deployment it was started for: 152 distinct
/// names across 193 backups on this install, spread over twelve deployments, and
/// none of them survives one. A note about one is still worth keeping - 68 of
/// those 152 were joined more than once, up to six times - but a list that
/// showed last month's favourite as somewhere you could go back to would be
/// lying.
/// </para>
/// <para>
/// It is <em>not</em> the client's build. The log's own <c>Build(12572603)</c>
/// header and the shard's <c>12545750</c> disagree in 14 of the 19 pairings on
/// this install - the servers roll separately from the client - so "still
/// current" has to be judged against the newest deployment ever joined, never
/// against the game's version.
/// </para>
/// <para>
/// Region labels are read off the code, because the game never spells them out.
/// The four this install has seen - <c>use1b</c>, <c>euw1b</c>, <c>ape1a</c>,
/// <c>apse2a</c> - follow cloud-zone naming closely enough to be named with
/// confidence; anything else is shown as the code it is.
/// </para>
/// </remarks>
public sealed partial record ShardName(
    string Full,
    string Environment,
    string RegionCode,
    string Deployment,
    string Number)
{
    /// <summary>A readable region, or the raw code when the prefix is new.</summary>
    public string Region => RegionLabel(RegionCode);

    /// <summary>The name as a pilot says it out loud: region and number.</summary>
    public string Short =>
        $"{Region} {(int.TryParse(Number, out var n) ? n.ToString() : Number)}";

    public static bool TryParse(string? full, [NotNullWhen(true)] out ShardName? name)
    {
        name = null;
        if (string.IsNullOrWhiteSpace(full))
            return false;

        var m = NameRegex.Match(full);
        if (!m.Success)
            return false;

        name = new ShardName(
            full,
            m.Groups["env"].Value,
            m.Groups["region"].Value,
            m.Groups["deployment"].Value,
            m.Groups["number"].Value);
        return true;
    }

    /// <summary>The deployment a name belongs to, or null for a name in a shape not seen before.</summary>
    public static string? DeploymentOf(string? full) => TryParse(full, out var name) ? name.Deployment : null;

    public static string RegionLabel(string code)
    {
        // The trailing digit-and-letter is the zone, which says nothing a pilot
        // needs: use1b and use1c would both be "US East".
        return ZoneSuffix.Replace(code, string.Empty) switch
        {
            "use" => "US East",
            "usw" => "US West",
            "usc" => "US Central",
            "euw" => "EU West",
            "euc" => "EU Central",
            "eun" => "EU North",
            "ape" => "Asia-Pacific East",
            "apse" => "Asia-Pacific Southeast",
            "apne" => "Asia-Pacific Northeast",
            "aps" => "Asia-Pacific South",
            "aus" => "Australia",
            _ => code.ToUpperInvariant()
        };
    }

    [GeneratedRegex(@"^(?<env>[a-z]+)_(?<region>[a-z]+\d+[a-z]?)_(?<deployment>\d+)_(?<number>\d+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex NameRegex { get; }

    [GeneratedRegex(@"\d+[a-z]?$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex ZoneSuffix { get; }
}

/// <summary>How a stay on a shard came to an end.</summary>
public enum ShardLeave
{
    /// <summary>The pilot asked to leave - the menu's exit, or a disconnect command.</summary>
    Left,

    /// <summary>The server removed an idle client.</summary>
    Idle,

    /// <summary>The game was closed while still on the shard.</summary>
    Quit,

    /// <summary>The client gave up on the back-end services.</summary>
    Backend,

    /// <summary>
    /// The log ended with the pilot still on the shard. A crash writes its dump
    /// without timestamps and the reader drops those lines, so a crash, a killed
    /// process and a lost server all look like this - which is why it is named
    /// for what the log shows rather than for a cause it cannot see.
    /// </summary>
    LogEnded,

    /// <summary>Another join arrived first - the game moved the client without a disconnect between.</summary>
    Replaced,

    /// <summary>
    /// Still on it. Never written by the builder - a saved session has no
    /// present tense - but put on the live session's open stay when it is
    /// merged into the Servers list, so the row you are sitting on does not
    /// read "log ended".
    /// </summary>
    Open
}

/// <summary>
/// One stretch spent on one shard.
/// </summary>
/// <param name="LeftAt">
/// When the stay ended. For <see cref="ShardLeave.LogEnded"/> this is the last
/// timestamp in the file, which is a floor on the time spent rather than the
/// moment anything happened.
/// </param>
public sealed record ShardStay(
    string Shard,
    DateTimeOffset JoinedAt,
    DateTimeOffset LeftAt,
    ShardLeave Ending)
{
    public TimeSpan Duration => LeftAt - JoinedAt;
}
