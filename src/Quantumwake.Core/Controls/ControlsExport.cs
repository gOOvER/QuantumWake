using System.Xml.Linq;

namespace Quantumwake.Core.Controls;

/// <summary>
/// Writes a keybinding profile in the form the game imports: the export
/// document the keybinding screen and <c>pp_rebindkeys</c> read from
/// <c>user\client\0\controls\mappings\</c>.
/// </summary>
/// <remarks>
/// <para>
/// The live <c>actionmaps.xml</c> and an export are the same document apart
/// from the frame: the live file wraps its <c>ActionProfiles</c> in an
/// <c>ActionMaps</c> root, an export is one <c>ActionMaps</c> root carrying
/// the profile's attributes and a <c>CustomisationUIHeader</c> naming the
/// devices. So a kept copy of the live file becomes an import by moving the
/// attributes up and adding the header - nothing bound is touched.
/// </para>
/// <para>
/// Retargeting is the swap fix: when the sticks were re-enumerated and the
/// throttle that was <c>js2</c> is now <c>js3</c>, every <c>js2_</c> input and
/// the <c>js2</c> options line move to 3, and whatever was 3 moves to where
/// the pilot says. It is string work on the inputs, done through
/// <see cref="ControlInput"/> so a chord's modifiers move with it.
/// </para>
/// </remarks>
public static class ControlsExport
{
    /// <summary>
    /// The export document for a profile, live or already exported.
    /// </summary>
    /// <param name="source">The profile as the game wrote it.</param>
    /// <param name="name">The profile name the game will list it under; also the header label.</param>
    /// <param name="retarget">Old joystick instance to new, for the sticks that moved; others keep their number.</param>
    public static XDocument Build(XDocument source, string name, IReadOnlyDictionary<int, int>? retarget = null)
    {
        var root = source.Root ?? throw new InvalidDataException("No root element.");
        var profile = root.Name.LocalName == "ActionMaps" && root.Element("ActionProfiles") is { } inner ? inner : root;

        var export = new XElement("ActionMaps",
            new XAttribute("version", (string?)profile.Attribute("version") ?? "1"),
            new XAttribute("optionsVersion", (string?)profile.Attribute("optionsVersion") ?? "2"),
            new XAttribute("rebindVersion", (string?)profile.Attribute("rebindVersion") ?? "2"),
            new XAttribute("profileName", name));

        var moved = retarget ?? new Dictionary<int, int>();
        int Instance(int n) => moved.TryGetValue(n, out var to) ? to : n;

        // The header lists the devices the profile binds, keyboard and mouse
        // included, which the game wants even when it can read them off the
        // options below.
        var devices = new XElement("devices");
        var seen = new HashSet<(string, int)>();
        // Keyboard and mouse first, as the game writes them, whether or not
        // the profile carries an options line for either.
        foreach (var type in new[] { "keyboard", "mouse" })
            if (seen.Add((type, 1))) devices.Add(new XElement(type, new XAttribute("instance", 1)));
        foreach (var options in profile.Elements("options"))
        {
            var type = (string?)options.Attribute("type") ?? "";
            if (!int.TryParse((string?)options.Attribute("instance"), out var instance)) continue;
            if (type == "joystick") instance = Instance(instance);
            if (seen.Add((type, instance))) devices.Add(new XElement(type, new XAttribute("instance", instance)));
        }
        export.Add(new XElement("CustomisationUIHeader",
            new XAttribute("label", name), new XAttribute("description", ""), new XAttribute("image", ""),
            devices, new XElement("categories")));

        foreach (var child in profile.Elements())
        {
            if (child.Name.LocalName == "CustomisationUIHeader") continue;
            var copy = new XElement(child);
            if (copy.Name.LocalName == "options" && (string?)copy.Attribute("type") == "joystick"
                && int.TryParse((string?)copy.Attribute("instance"), out var instance))
                copy.SetAttributeValue("instance", Instance(instance));
            if (copy.Name.LocalName == "actionmap" && moved.Count > 0)
                foreach (var rebind in copy.Descendants("rebind"))
                    if ((string?)rebind.Attribute("input") is { } input)
                        rebind.SetAttributeValue("input", Retarget(input, moved));
            export.Add(copy);
        }

        return new XDocument(new XDeclaration("1.0", "utf-8", null), export);
    }

    /// <summary>
    /// The live frame for a document: what <c>actionmaps.xml</c> has to look
    /// like for the game to read it at start. A kept copy of the live file
    /// is already that; an export brought back from the mappings folder is
    /// the same profile under the other root, and is re-framed - the header
    /// dropped, the attributes moved down, the name set back to "default",
    /// which is the only profile the game reads there.
    /// </summary>
    public static XDocument ToLive(XDocument source)
    {
        var root = source.Root ?? throw new InvalidDataException("No root element.");
        if (root.Name.LocalName == "ActionMaps" && root.Element("ActionProfiles") is not null)
            return new XDocument(source);

        var profile = new XElement("ActionProfiles",
            new XAttribute("version", (string?)root.Attribute("version") ?? "1"),
            new XAttribute("optionsVersion", (string?)root.Attribute("optionsVersion") ?? "2"),
            new XAttribute("rebindVersion", (string?)root.Attribute("rebindVersion") ?? "2"),
            new XAttribute("profileName", "default"));
        foreach (var child in root.Elements())
            if (child.Name.LocalName != "CustomisationUIHeader") profile.Add(new XElement(child));
        return new XDocument(new XElement("ActionMaps", profile));
    }

    /// <summary>One input string with its joystick instances moved; anything not a joystick is left alone.</summary>
    public static string Retarget(string input, IReadOnlyDictionary<int, int> moved)
    {
        // Each part of a chord on its own: a segment has no modifiers of its
        // own, so its raw text is the part and nothing more.
        return string.Join("+", input.Split('+').Select(segment =>
        {
            var part = ControlInput.Parse(segment);
            return part.IsJoystick && moved.TryGetValue(part.Instance, out var to) ? part.OnInstance(to) : segment;
        }));
    }

    /// <summary>
    /// A file name the game accepts: letters, digits, dash and underscore;
    /// the profile is then <c>pp_rebindkeys &lt;name&gt;</c> at the console.
    /// </summary>
    public static string SafeName(string name)
    {
        var chars = name.Trim().Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '_').ToArray();
        var safe = new string(chars).Trim('_');
        return safe.Length > 0 ? safe : "quantumwake";
    }
}
