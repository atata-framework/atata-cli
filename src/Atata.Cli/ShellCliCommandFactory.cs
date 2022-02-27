using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Atata.Cli
{
    /// <summary>
    /// Represents the <see cref="CliCommand"/> factory that executes through command shell (cmd/bash).
    /// </summary>
    public class ShellCliCommandFactory : ICliCommandFactory
    {
        /// <summary>
        /// Gets a <see cref="ConcurrentDictionary{TKey,TValue}"/> where the shell command can be set for a given OS.
        /// </summary>
        public static IDictionary<OSPlatform, ShellCommand> Shells { get; } =
            new ConcurrentDictionary<OSPlatform, ShellCommand>
            {
                [OSPlatform.Windows] = new ShellCommand("cmd", "/c", escapeArguments: false),
                [OSPlatform.Linux] = new ShellCommand("bash", "-c"),
                [OSPlatform.OSX] = new ShellCommand("bash", "-c"),
            };

        /// <inheritdoc/>
        public CliCommand Create(string fileNameOrCommand, string arguments)
        {
            var platform = Shells.Keys.FirstOrDefault(RuntimeInformation.IsOSPlatform);
            // Fallback on Linux: if it's not a listed OS (e.g. FreeBSD) it's probably still Unix-like.
            if (!Shells.TryGetValue(platform, out var shellCommand)) shellCommand = Shells[OSPlatform.Linux];

            var (actualFileName, actualArguments) = shellCommand.Build(fileNameOrCommand, arguments);

            return new CliCommand(actualFileName, actualArguments);
        }
    }
}
