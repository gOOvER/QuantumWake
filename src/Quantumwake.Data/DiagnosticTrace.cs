using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Quantumwake.Core;

namespace Quantumwake.Data;

/// <summary>Opt-in breadcrumbs for a failure that ends the desktop process.</summary>
/// <remarks>
/// A native crash can leave no managed exception to report. The useful evidence
/// then is the last completed stage, not a promise that the process can log
/// after Windows has already stopped it. This stays local, records no game-log
/// text or file names, and is capped so an overnight run cannot fill a disk.
/// </remarks>
public static class DiagnosticTrace
{
    private const int MaximumBytes = 1_000_000;
    private static readonly Lock Gate = new();
    private static bool? _enabled;

    private static string SettingsPath => AppPaths.In("diagnostic-trace.json");
    private static string LogPath => AppPaths.In("diagnostic-trace.log");

    /// <summary>Whether the pilot has asked for detailed local tracing.</summary>
    public static DiagnosticTraceState Snapshot()
    {
        lock (Gate)
        {
            var bytes = 0L;
            try { if (File.Exists(LogPath)) bytes = new FileInfo(LogPath).Length; }
            catch { /* a trace must not become a second failure */ }
            return new DiagnosticTraceState(Enabled(), bytes);
        }
    }

    /// <summary>Turns tracing on or off without deleting the evidence already collected.</summary>
    public static DiagnosticTraceState Configure(bool enabled)
    {
        lock (Gate)
        {
            _enabled = enabled;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
                File.WriteAllText(SettingsPath, JsonSerializer.Serialize(new TraceSettings(enabled)));
            }
            catch { /* as above */ }
        }

        if (enabled)
            Mark("trace", "Detailed tracing enabled.");

        return Snapshot();
    }

    /// <summary>Records the host and runtime at the earliest point its data folder is known.</summary>
    public static void Start(string host) =>
        Mark("startup", $"{host}; runtime {Environment.Version}; {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}.");

    /// <summary>Records a completed or begun stage, without a path or user data.</summary>
    public static void Mark(string stage, string detail) => Write(stage, detail);

    /// <summary>Records a managed exception's type and scrubbed message.</summary>
    public static void Failed(string stage, Exception error) =>
        Write(stage, $"{error.GetType().Name}: {Redact(error.Message)}");

    /// <summary>Returns the local trace for an explicit local save, or null when none exists.</summary>
    public static string? Read()
    {
        lock (Gate)
        {
            try { return File.Exists(LogPath) ? File.ReadAllText(LogPath) : null; }
            catch { return null; }
        }
    }

    private static bool Enabled()
    {
        if (_enabled is not null) return _enabled.Value;

        try
        {
            _enabled = File.Exists(SettingsPath)
                && JsonSerializer.Deserialize<TraceSettings>(File.ReadAllText(SettingsPath))?.Enabled == true;
        }
        catch { _enabled = false; }

        return _enabled.Value;
    }

    private static void Write(string stage, string detail)
    {
        lock (Gate)
        {
            if (!Enabled()) return;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                if (File.Exists(LogPath) && new FileInfo(LogPath).Length >= MaximumBytes)
                    File.WriteAllText(LogPath, $"{DateTimeOffset.UtcNow:O} trace: rotated after {MaximumBytes:N0} bytes.{Environment.NewLine}", Encoding.UTF8);

                File.AppendAllText(LogPath,
                    $"{DateTimeOffset.UtcNow:O} {stage}: {Redact(detail)}{Environment.NewLine}", Encoding.UTF8);
            }
            catch { /* tracing must never make a healthy app fail */ }
        }
    }

    // A trace names stages, not the pilot's folders. Error messages are the
    // one way a path can slip in, so remove drive and UNC paths before writing.
    private static string Redact(string value) =>
        Regex.Replace(value.Replace('\r', ' ').Replace('\n', ' '), "(?i)(?:[a-z]:\\\\|\\\\\\\\)[^\\s\\\"']+", "<path>");

    private sealed record TraceSettings(bool Enabled);
}

/// <summary>The trace setting and its bounded current size, safe to show in Settings.</summary>
public sealed record DiagnosticTraceState(bool Enabled, long Bytes);
