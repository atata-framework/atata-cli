using System;

namespace Atata.Cli
{
    /// <summary>
    /// The enumeration of possible validation rules of CLI command result.
    /// </summary>
    [Flags]
    public enum CliCommandResultValidationRules
    {
        /// <summary>
        /// No validation.
        /// </summary>
        None = 0,

        /// <summary>
        /// The exit code should equal 0.
        /// </summary>
        ZeroExitCode = 1,

        /// <summary>
        /// There should not be an error text.
        /// </summary>
        NoError = 2,

        /// <summary>
        /// The exit code should equal 0 and there should not be an error text.
        /// </summary>
        ZeroExitCodeAndNoError = ZeroExitCode | NoError
    }
}
