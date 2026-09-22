namespace Quantumwake.Core.GameData;

/// <summary>Whether an inventory sighting is enough to settle a shopping line.</summary>
public static class ShoppingAvailability
{
    /// <summary>
    /// A Garage list asks for a loose component. The game records seeing an
    /// item in inventory but not installing it on another ship, so that signal
    /// cannot safely settle the list; other lists retain their usual
    /// presence-only interpretation.
    /// </summary>
    public static bool CountsInventorySighting(string? source) =>
        !string.IsNullOrWhiteSpace(source)
            ? !source.StartsWith("garage:", StringComparison.OrdinalIgnoreCase)
            : true;
}
