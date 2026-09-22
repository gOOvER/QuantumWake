namespace Quantumwake.Data;

/// <summary>A game controller as the operating system sees it right now.</summary>
/// <param name="Id">Stable for as long as the device is plugged in; the reader's own.</param>
/// <param name="Guid">The product GUID the game would write for it - <c>{0404044F-…}</c> - from its USB ids.</param>
/// <param name="Name">What the OS calls it; often only "HID-compliant game controller".</param>
public sealed record JoystickDevice(string Id, int Vendor, int Product, string Guid, string Name, int Buttons, int Axes, int Hats);

/// <summary>What a controller is doing this instant.</summary>
/// <param name="Buttons">The pressed buttons, numbered from 1 as the game numbers them.</param>
/// <param name="Hats">Per hat, its direction as the game names it - up, down, left, right, or a diagonal joined with a dash - or null centred.</param>
/// <param name="Axes">Per axis, -1 to 1, in the reader's order.</param>
public sealed record JoystickReading(string Id, IReadOnlyList<int> Buttons, IReadOnlyList<string?> Hats, IReadOnlyList<double> Axes);

/// <summary>
/// The controllers plugged in and what they are doing - the live half of the
/// Controls page. Windows.Gaming.Input under the tray app; nothing under the
/// bare server, which says so rather than pretending.
/// </summary>
public interface IJoystickReader
{
    /// <summary>False when the API is not there; the page then shows the profile alone.</summary>
    bool Available { get; }

    IReadOnlyList<JoystickDevice> Devices();

    /// <summary>The state of every device, one reading each; a device that has gone is left out.</summary>
    IReadOnlyList<JoystickReading> Read();
}
