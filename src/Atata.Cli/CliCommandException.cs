using System;
using System.Runtime.Serialization;
using System.Text;

namespace Atata.Cli
{
    /// <summary>
    /// Represents the error that occurred during CLI command execution.
    /// </summary>
    [Serializable]
    public class CliCommandException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CliCommandException"/> class.
        /// </summary>
        public CliCommandException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CliCommandException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CliCommandException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CliCommandException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public CliCommandException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CliCommandException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected CliCommandException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CliCommandException"/> class.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="error">The error.</param>
        /// <param name="output">The output.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <returns>The <see cref="CliCommandException"/> instance.</returns>
        public static CliCommandException Create(string commandText, string error, string output = null, Exception innerException = null)
        {
            StringBuilder messageBuilder = new StringBuilder("CLI command failure: ")
                .AppendLine(commandText)
                .Append(error);

            if (!string.IsNullOrWhiteSpace(output))
            {
                messageBuilder
                    .AppendLine()
                    .AppendLine()
                    .AppendLine("Output:")
                    .Append(output);
            }

            return new CliCommandException(
                messageBuilder.ToString(),
                innerException);
        }
    }
}
