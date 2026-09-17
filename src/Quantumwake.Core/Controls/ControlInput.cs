using System.Text.Json.Serialization;

namespace Quantumwake.Core.Controls;

/// <summary>What a bound input is: a button, a hat direction, an axis, a key, or nothing.</summary>
public enum InputKind
{
    /// <summary>The device prefix with nothing after it - <c>js1_ </c> - which is how the game records a default binding taken away.</summary>
    None,
    Button,
    Hat,
    Axis,
    Key,
    /// <summary>A mouse button, wheel or axis, or a gamepad control: named, not numbered.</summary>
    Named,
}

/// <summary>
/// One input as the game writes it - <c>js2_button11</c>, <c>js1_hat1_up</c>,
/// <c>js4_x</c>, <c>kb1_lalt+f</c>, <c>mo1_mouse1</c> - taken apart.
/// </summary>
/// <param name="Raw">Exactly what the file said.</param>
/// <param name="Device">js, kb, mo or gp; empty when the string carried no prefix (the catalogue's defaults do not).</param>
/// <param name="Instance">The device's number - the <c>2</c> in <c>js2</c>; 0 without a prefix.</param>
/// <param name="Kind">What the last part names.</param>
/// <param name="Index">The button or hat number; 0 otherwise.</param>
/// <param name="Name">The axis, key or named control - <c>x</c>, <c>rotz</c>, <c>slider1</c>, <c>f</c>, <c>mouse1</c>; the hat direction for a hat.</param>
/// <param name="Modifiers">The parts before the last <c>+</c>, each an input of its own - a held button or key.</param>
public sealed record ControlInput(
    string Raw,
    string Device,
    int Instance,
    InputKind Kind,
    int Index,
    string Name,
    IReadOnlyList<ControlInput> Modifiers)
{
    private static readonly HashSet<string> AxisNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "x", "y", "z", "rotx", "roty", "rotz", "slider1", "slider2",
    };

    /// <summary>True for a joystick input.</summary>
    [JsonIgnore]
    public bool IsJoystick => Device == "js";

    /// <summary>The device the way the profile keys it - <c>js2</c>, <c>kb1</c>.</summary>
    [JsonIgnore]
    public string DeviceKey => Device.Length > 0 ? $"{Device}{Instance}" : "";

    /// <summary>
    /// The control without its device - <c>button11</c>, <c>hat1_up</c>, <c>x</c>
    /// - which is what a template names and what a layout for another instance
    /// of the same device shares.
    /// </summary>
    [JsonIgnore]
    public string Control => Kind switch
    {
        InputKind.Button => $"button{Index}",
        InputKind.Hat => $"hat{Index}_{Name}",
        InputKind.None => "",
        _ => Name,
    };

    /// <summary>
    /// Reads one input string. Never throws: a shape it does not know keeps
    /// its raw text under <see cref="InputKind.Named"/>, so an unfamiliar
    /// control is shown as the game wrote it rather than dropped.
    /// </summary>
    public static ControlInput Parse(string raw)
    {
        raw ??= "";
        var text = raw;
        var parts = text.Split('+');
        var last = parts[^1];
        var modifiers = parts.Length > 1
            ? parts[..^1].Select(Parse).ToList()
            : (IReadOnlyList<ControlInput>)[];

        // The device prefix: two letters and a number, then an underscore.
        var device = "";
        var instance = 0;
        var body = last;
        var underscore = last.IndexOf('_');
        if (underscore >= 3 && char.IsLetter(last[0]) && char.IsLetter(last[1]) && int.TryParse(last.AsSpan(2, underscore - 2), out var n))
        {
            device = last[..2].ToLowerInvariant();
            instance = n;
            body = last[(underscore + 1)..];
        }
        body = body.Trim();

        if (body.Length == 0)
            return new ControlInput(raw, device, instance, InputKind.None, 0, "", modifiers);

        if (body.StartsWith("button", StringComparison.OrdinalIgnoreCase) && int.TryParse(body.AsSpan(6), out var button))
            return new ControlInput(raw, device, instance, InputKind.Button, button, "", modifiers);

        if (body.StartsWith("hat", StringComparison.OrdinalIgnoreCase))
        {
            var sep = body.IndexOf('_');
            if (sep > 3 && int.TryParse(body.AsSpan(3, sep - 3), out var hat))
                return new ControlInput(raw, device, instance, InputKind.Hat, hat, body[(sep + 1)..].ToLowerInvariant(), modifiers);
        }

        if (device is ("js" or "") && AxisNames.Contains(body))
            return new ControlInput(raw, device, instance, InputKind.Axis, 0, body.ToLowerInvariant(), modifiers);

        if (device == "kb")
            return new ControlInput(raw, device, instance, InputKind.Key, 0, body, modifiers);

        return new ControlInput(raw, device, instance, InputKind.Named, 0, body, modifiers);
    }

    /// <summary>
    /// The same control on another instance of the device - what retargeting
    /// a layout after the sticks were re-enumerated writes.
    /// </summary>
    public string OnInstance(int other)
    {
        var prefix = Device.Length > 0 ? $"{Device}{other}_" : "";
        var me = Kind == InputKind.None ? $"{prefix} " : $"{prefix}{Control}";
        return Modifiers.Count == 0 ? me : string.Join("+", Modifiers.Select(m => m.OnInstance(other)).Append(me));
    }

    /// <summary>A short human form: "button 11", "hat 1 up", "x axis", "lalt + f".</summary>
    [JsonIgnore]
    public string Label
    {
        get
        {
            var mine = Kind switch
            {
                InputKind.Button => $"button {Index}",
                InputKind.Hat => $"hat {Index} {Name}",
                InputKind.Axis => $"{Name} axis",
                InputKind.None => "unbound",
                _ => Name,
            };
            return Modifiers.Count == 0 ? mine : string.Join(" + ", Modifiers.Select(m => m.Label).Append(mine));
        }
    }
}
