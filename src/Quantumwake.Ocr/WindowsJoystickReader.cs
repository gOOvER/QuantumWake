using Quantumwake.Data;
using Windows.Gaming.Input;

namespace Quantumwake.Ocr;

/// <summary>
/// The controllers as Windows.Gaming.Input sees them - the live half of the
/// Controls page.
/// </summary>
/// <remarks>
/// <para>
/// <c>RawGameController</c> gives every HID game controller with its
/// vendor and product ids and its button, axis and switch counts, and a
/// reading on demand; no DirectInput, no driver. The game's product GUID
/// composes from the ids - checked against this install's six devices on
/// 2026-09-17, every one matched. Buttons are numbered from 1 here because
/// that is how the game numbers them in <c>js2_button11</c>; the API
/// counts from 0.
/// </para>
/// <para>
/// The same seam as the screen reader: lives here because the project
/// already targets Windows, and is handed to the server by the tray app.
/// The list is taken fresh on each call rather than kept, so a stick
/// plugged in after start appears; the API keeps its own list and the
/// call is cheap.
/// </para>
/// </remarks>
public sealed class WindowsJoystickReader : IJoystickReader
{
    private readonly bool _available;

    public WindowsJoystickReader()
    {
        try
        {
            // Touching the collection once is what makes the API start
            // enumerating; a first call that finds nothing is not "no API".
            _ = RawGameController.RawGameControllers.Count;
            _available = true;
        }
        catch (Exception e) when (e is TypeLoadException or PlatformNotSupportedException or System.Runtime.InteropServices.COMException or System.IO.FileNotFoundException)
        {
            _available = false;
        }
    }

    public bool Available => _available;

    public IReadOnlyList<JoystickDevice> Devices()
    {
        if (!_available) return [];
        var list = new List<JoystickDevice>();
        foreach (var c in RawGameController.RawGameControllers)
        {
            list.Add(new JoystickDevice(
                c.NonRoamableId.TrimEnd((char)0), c.HardwareVendorId, c.HardwareProductId,
                Quantumwake.Core.Controls.ControlDevice.GuidFor(c.HardwareVendorId, c.HardwareProductId),
                c.DisplayName ?? "", c.ButtonCount, c.AxisCount, c.SwitchCount));
        }
        return list;
    }

    public IReadOnlyList<JoystickReading> Read()
    {
        if (!_available) return [];
        var list = new List<JoystickReading>();
        foreach (var c in RawGameController.RawGameControllers)
        {
            var buttons = new bool[c.ButtonCount];
            var switches = new GameControllerSwitchPosition[c.SwitchCount];
            var axes = new double[c.AxisCount];
            try { c.GetCurrentReading(buttons, switches, axes); }
            catch (Exception e) when (e is System.Runtime.InteropServices.COMException or ArgumentException) { continue; }

            var pressed = new List<int>();
            for (var i = 0; i < buttons.Length; i++) if (buttons[i]) pressed.Add(i + 1);
            var hats = switches.Select(Hat).ToList();
            // The API reports an axis 0..1 with 0.5 at rest; the game's
            // world is -1..1 with 0 at rest, which is what a curve preview draws.
            var centred = axes.Select(a => Math.Round(a * 2 - 1, 3)).ToList();
            list.Add(new JoystickReading(c.NonRoamableId.TrimEnd((char)0), pressed, hats, centred));
        }
        return list;
    }

    private static string? Hat(GameControllerSwitchPosition position) => position switch
    {
        GameControllerSwitchPosition.Up => "up",
        GameControllerSwitchPosition.UpRight => "up-right",
        GameControllerSwitchPosition.Right => "right",
        GameControllerSwitchPosition.DownRight => "down-right",
        GameControllerSwitchPosition.Down => "down",
        GameControllerSwitchPosition.DownLeft => "down-left",
        GameControllerSwitchPosition.Left => "left",
        GameControllerSwitchPosition.UpLeft => "up-left",
        _ => null,
    };
}
