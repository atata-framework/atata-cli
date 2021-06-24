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
            string argumentsPart = string.IsNullOrEmpty(arguments)
                ? null
                : $" {arguments}";

            (string actualFileName, string actualArguments) = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? ("cmd.exe", $"/c {fileNameOrCommand}{argumentsPart}")
                : ("/bin/bash", $"-c '{fileNameOrCommand}{argumentsPart}'");

            return new CliCommand(actualFileName, actualArguments);
        }
    }
}
