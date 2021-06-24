using System.Runtime.InteropServices;

namespace Atata.Cli
{
    /// <summary>
    /// Represents the <see cref="CliCommand"/> factory that executes through command shell (cmd/bash).
    /// </summary>
    public class ShellCliCommandFactory : ICliCommandFactory
    {
        /// <inheritdoc/>
        public CliCommand Create(string fileNameOrCommand, string arguments)
        {
            (string actualFileName, string actualArguments) = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? ("cmd.exe", $"/c {fileNameOrCommand}")
                : ("/bin/bash", $"-c \"{fileNameOrCommand}\"");

            if (!string.IsNullOrEmpty(arguments))
                actualArguments += $" {arguments}";

            return new CliCommand(actualFileName, actualArguments);
        }
    }
}
