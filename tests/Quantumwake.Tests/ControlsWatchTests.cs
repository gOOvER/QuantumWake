using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using Quantumwake.Core.Logging;
using Quantumwake.Data;
using Quantumwake.Server;

namespace Quantumwake.Tests;

public class ControlsWatchTests
{
    [Fact]
    public async Task A_locked_profile_is_backed_up_after_unlock_without_another_metadata_change()
    {
        var root = Path.Combine(Path.GetTempPath(), "qw-controls-watch-" + Guid.NewGuid().ToString("N"));
        var install = new GameInstall("test", root);
        var path = ControlsWatchService.ProfilePath(install);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        const string profile = "<ActionMaps><ActionProfiles profileName=\"default\" /></ActionMaps>";
        File.WriteAllText(path, profile);
        using var cancel = new CancellationTokenSource();
        var store = new ControlsStore(Path.Combine(root, "backups"));
        using var watcher = new ControlsWatchService(store, NullLogger<ControlsWatchService>.Instance, install);
        Task? watching = null;
        try
        {
            using (var held = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                // Invoke directly so the first poll runs up to its delay while
                // the lock is held, without a race with BackgroundService startup.
                watching = (Task)typeof(ControlsWatchService).GetMethod("ExecuteAsync", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .Invoke(watcher, [cancel.Token])!;
                Assert.Empty(store.Backups());
            }
            var deadline = DateTime.UtcNow.AddSeconds(15);
            while (store.Backups().Count == 0 && DateTime.UtcNow < deadline)
                await Task.Delay(100);
            var backup = Assert.Single(store.Backups());
            Assert.Equal(profile, System.Text.Encoding.UTF8.GetString(store.Read(backup.Id)!));
        }
        finally
        {
            cancel.Cancel();
            if (watching is not null) await watching;
            Directory.Delete(root, true);
        }
    }
}
