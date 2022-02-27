namespace Atata.Cli
{
    public class ShellCommand
    {
        public ShellCommand(string command, string requiredArguments, bool escapeArguments = true)
        {
            Command = command;
            RequiredArguments = requiredArguments;
            EscapeArguments = escapeArguments;
        }

        /// <summary>
        /// Gets the shell executable's name.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Gets the required initial arguments that come before the <c>arguments</c> parameter in <see
        /// cref="ICliCommandFactory.Create"/>. Typically it's <c>"/c"</c>, <c>"-c"</c> or similar.
        /// </summary>
        public string RequiredArguments { get; }

        /// <summary>
        /// Gets a value indicating whether the additional arguments (not <see cref="RequiredArguments"/>) must be
        /// wrapped in quote characters.
        /// </summary>
        public bool EscapeArguments { get; }

        public (string FileName, string Arguments) Build(string fileNameOrCommand, string additionalArguments)
        {
            var arguments = string.IsNullOrEmpty(additionalArguments) ? string.Empty : $" {additionalArguments}";
            if (EscapeArguments)
            {
                arguments = $"\"{fileNameOrCommand}{EscapeDoubleQuotes(arguments)}\"";
            }

            return (Command, $"{RequiredArguments} {arguments}");
        }

        private static string EscapeDoubleQuotes(string value) =>
            value.Replace("\"", "\\\"");
    }
}
