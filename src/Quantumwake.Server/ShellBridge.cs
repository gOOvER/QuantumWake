namespace Quantumwake.Server;

/// <summary>
/// The things the server can ask the window around it to do: restart, and
/// open MFD setup.
/// </summary>
/// <remarks>
/// <para>
/// Separate from <see cref="OverlayBridge"/> on purpose. That one is about a
/// window being shown or hidden and is asked about constantly; this is about the
/// process ending itself, and the two have no business sharing a lock.
/// </para>
/// <para>
/// Nothing is attached when the server runs on its own - from the CLI, or in a
/// test - and then <see cref="TryRestart"/> answers false rather than pretending.
/// The caller says "installed, restart it yourself" instead of promising a
/// restart that was never going to happen.
/// </para>
/// </remarks>
public sealed class ShellBridge
{
    private readonly Lock _lock = new();
    private Action? _restart;

    /// <summary>True when something is listening that can actually restart.</summary>
    public bool Available
    {
        get { lock (_lock) return _restart is not null; }
    }

    /// <summary>Called once by the shell as it starts.</summary>
    public void AttachRestart(Action restart)
    {
        lock (_lock) _restart = restart;
    }

    /// <summary>Asks for a restart. False when nothing can.</summary>
    public bool TryRestart()
    {
        Action? restart;
        lock (_lock) restart = _restart;

        if (restart is null)
            return false;

        restart();
        return true;
    }

    private Action? _mfdSetup;

    /// <summary>True when the window around the server can open MFD setup.</summary>
    public bool CanOpenMfdSetup
    {
        get { lock (_lock) return _mfdSetup is not null; }
    }

    /// <summary>Called once by the shell as it starts.</summary>
    /// <remarks>
    /// The dashboard's way into MFD setup, beside the tray and Ctrl+Alt+M: a
    /// browser on any screen of the PC can ask, when the frames have landed
    /// over the taskbar. Not from another machine - LanGuard refuses every
    /// POST from the network, and this one opens a window.
    /// </remarks>
    public void AttachMfdSetup(Action open)
    {
        lock (_lock) _mfdSetup = open;
    }

    /// <summary>Asks for MFD setup to open. False when nothing can.</summary>
    public bool TryOpenMfdSetup()
    {
        Action? open;
        lock (_lock) open = _mfdSetup;

        if (open is null)
            return false;

        open();
        return true;
    }
}
