using System.Globalization;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Quantumwake.Core.Controls;

/// <summary>
/// A device the profile knows: which instance number it holds and, where the
/// game recorded it, which product that was.
/// </summary>
/// <param name="Type">joystick, keyboard, mouse or gamepad.</param>
/// <param name="Instance">The number in <c>js2</c>.</param>
/// <param name="Product">The game's name for it - "Throttle - HOTAS Warthog" - or null for an instance nothing was plugged into.</param>
/// <param name="Guid">The product GUID the game wrote - <c>{0404044F-0000-0000-0000-504944564944}</c> - or null.</param>
/// <param name="Deadzones">Per axis, from the device's <c>deviceoptions</c>.</param>
/// <param name="Curves">Per action option group, the exponent and invert the profile sets - <c>flight_move_pitch</c> → exponent 1.5.</param>
public sealed record ControlDevice(
    string Type,
    int Instance,
    string? Product,
    string? Guid,
    IReadOnlyDictionary<string, double> Deadzones,
    IReadOnlyList<ControlCurve> Curves)
{
    /// <summary>The way a binding names this device - <c>js2</c>.</summary>
    [JsonIgnore]
    public string Key => $"{Prefix(Type)}{Instance}";

    /// <summary>The two-letter prefix a binding uses for a device type.</summary>
    public static string Prefix(string type) => type.ToLowerInvariant() switch
    {
        "joystick" => "js",
        "keyboard" => "kb",
        "mouse" => "mo",
        "gamepad" => "gp",
        var other => other,
    };

    /// <summary>
    /// The USB vendor and product ids inside the game's GUID: the first eight
    /// hex digits are the product id then the vendor id, and the tail spells
    /// "PIDVID". Null for a keyboard or an unrecognised shape.
    /// </summary>
    [JsonIgnore]
    public (int Vendor, int Product)? Usb => UsbOf(Guid);

    public static (int Vendor, int Product)? UsbOf(string? guid)
    {
        if (guid is null) return null;
        var g = guid.Trim('{', '}');
        if (g.Length < 8 || !g.EndsWith("504944564944", StringComparison.OrdinalIgnoreCase)) return null;
        return int.TryParse(g.AsSpan(0, 4), NumberStyles.HexNumber, null, out var pid)
            && int.TryParse(g.AsSpan(4, 4), NumberStyles.HexNumber, null, out var vid)
            ? (vid, pid) : null;
    }

    /// <summary>The GUID the game would write for a device with these USB ids.</summary>
    public static string GuidFor(int vendor, int product) =>
        $"{{{product:X4}{vendor:X4}-0000-0000-0000-504944564944}}";
}

/// <param name="Option">The option group - <c>flight_move_pitch</c>.</param>
/// <param name="Exponent">Null when the profile leaves the curve alone.</param>
public sealed record ControlCurve(string Option, double? Exponent, bool Inverted);

/// <summary>One binding the profile makes: this action, on this input.</summary>
/// <param name="ActivationMode">The profile's override - <c>hold</c>, <c>double_tap</c> - or null for the catalogue's.</param>
public sealed record ControlBinding(
    string ActionMap,
    string Action,
    ControlInput Input,
    string? ActivationMode,
    int? MultiTap);

/// <summary>
/// A keybinding profile as the game writes it - the live
/// <c>actionmaps.xml</c>, an export under <c>controls\mappings\</c>, or one
/// of the reference layouts in the archive; all three are the same document.
/// </summary>
/// <param name="Name">The <c>profileName</c>; "default" for the live file.</param>
/// <param name="Label">The export header's label, or null for the live file.</param>
public sealed record ControlProfile(
    string Name,
    string? Label,
    IReadOnlyList<ControlDevice> Devices,
    IReadOnlyList<ControlBinding> Bindings)
{
    /// <summary>The joystick devices, by instance.</summary>
    [JsonIgnore]
    public IEnumerable<ControlDevice> Joysticks => Devices.Where(d => d.Type == "joystick").OrderBy(d => d.Instance);

    /// <summary>The device a binding's input names, or null for one the profile has no line for.</summary>
    public ControlDevice? DeviceOf(ControlInput input) =>
        Devices.FirstOrDefault(d => d.Key == input.DeviceKey);

    public static ControlProfile Parse(byte[] data) => Parse(Quantumwake.Core.GameData.CryXml.Parse(data));

    public static ControlProfile Parse(XDocument document)
    {
        var root = document.Root ?? throw new InvalidDataException("No root element.");
        // The live file wraps everything in ActionProfiles; an export does not.
        var profile = root.Name.LocalName == "ActionMaps" && root.Element("ActionProfiles") is { } inner ? inner : root;

        var name = (string?)profile.Attribute("profileName") ?? "default";
        var label = (string?)root.Element("CustomisationUIHeader")?.Attribute("label");

        // Dead zones are keyed by product name, not instance, and sit apart
        // from the options; joined here so a device carries its own.
        var deadzones = new Dictionary<string, Dictionary<string, double>>(StringComparer.OrdinalIgnoreCase);
        foreach (var options in profile.Elements("deviceoptions"))
        {
            var product = (string?)options.Attribute("name") ?? "";
            var axes = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var option in options.Elements("option"))
            {
                var input = (string?)option.Attribute("input");
                if (input is null) continue;
                if (double.TryParse((string?)option.Attribute("deadzone"), NumberStyles.Float, CultureInfo.InvariantCulture, out var dz))
                    axes[input] = dz;
            }
            deadzones[product] = axes;
        }

        var devices = new List<ControlDevice>();
        foreach (var options in profile.Elements("options"))
        {
            var type = (string?)options.Attribute("type") ?? "";
            if (!int.TryParse((string?)options.Attribute("instance"), out var instance)) continue;
            var (product, guid) = SplitProduct((string?)options.Attribute("Product"));
            var curves = options.Elements()
                .Select(e => new ControlCurve(
                    e.Name.LocalName,
                    double.TryParse((string?)e.Attribute("exponent"), NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ? x : null,
                    (string?)e.Attribute("invert") == "1"))
                .ToList();
            devices.Add(new ControlDevice(type, instance, product, guid,
                product is not null && deadzones.TryGetValue($"{product}  {guid}", out var dz) ? dz
                    : product is not null && deadzones.TryGetValue(product, out var dz2) ? dz2
                    : new Dictionary<string, double>(),
                curves));
        }

        var bindings = new List<ControlBinding>();
        foreach (var map in profile.Elements("actionmap"))
        {
            var mapName = (string?)map.Attribute("name") ?? "";
            foreach (var action in map.Elements("action"))
            {
                var actionName = (string?)action.Attribute("name") ?? "";
                foreach (var rebind in action.Elements("rebind"))
                {
                    var input = (string?)rebind.Attribute("input");
                    if (input is null) continue;
                    bindings.Add(new ControlBinding(
                        mapName, actionName, ControlInput.Parse(input),
                        (string?)rebind.Attribute("activationMode"),
                        int.TryParse((string?)rebind.Attribute("multiTap"), out var taps) ? taps : null));
                }
            }
        }

        return new ControlProfile(name, label, devices, bindings);
    }

    /// <summary>
    /// "Throttle - HOTAS Warthog  {0404044F-0000-0000-0000-504944564944}" into
    /// its name and its GUID; the live file writes two spaces between them,
    /// the shipped layouts one.
    /// </summary>
    public static (string? Product, string? Guid) SplitProduct(string? product)
    {
        if (string.IsNullOrWhiteSpace(product)) return (null, null);
        var brace = product.LastIndexOf('{');
        if (brace < 0) return (product.Trim(), null);
        var name = product[..brace].Trim();
        var guid = product[brace..].Trim();
        return (name.Length > 0 ? name : null, guid.EndsWith('}') ? guid : null);
    }
}
