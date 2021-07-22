using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Atata.Cli
{
    /// <summary>
    /// Represents the CLI command.
    /// It is one-time executable and not reusable.
    /// Uses <see cref="Process"/> class to execute a command.
    /// </summary>
    public class CliCommand : IDisposable
    {
        private readonly Process _process;

        private readonly StringBuilder _outputStringBuilder = new StringBuilder();

        private readonly StringBuilder _errorStringBuilder = new StringBuilder();

        private readonly ManualResetEventSlim _outputResetEvent = new ManualResetEventSlim();

        private readonly ManualResetEventSlim _errorResetEvent = new ManualResetEventSlim();

        private readonly ManualResetEventSlim _exitResetEvent = new ManualResetEventSlim();

        private CliCommandResult _result;

        private bool _isStarted;

        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CliCommand"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="arguments">The arguments.</param>
        public CliCommand(string fileName, string arguments = null)
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo(fileName, arguments)
                {
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true
            };

            _process.OutputDataReceived += OnProcessOutputDataReceived;
            _process.ErrorDataReceived += OnProcessErrorDataReceived;
            _process.Exited += OnProcessExited;
        }

        /// <summary>
        /// Gets the process <see cref="ProcessStartInfo"/> instance that can be configured.
        /// </summary>
        public ProcessStartInfo StartInfo => _process.StartInfo;

        /// <summary>
        /// Gets the executable command text.
        /// </summary>
        public string CommandText
        {
            get
            {
                StringBuilder builder = new StringBuilder();

                if (!string.IsNullOrEmpty(StartInfo.FileName))
                {
                    builder.Append(StartInfo.FileName);

                    if (!string.IsNullOrEmpty(StartInfo.Arguments))
                        builder.Append(' ').Append(StartInfo.Arguments);
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Gets the output of command.
        /// </summary>
        public string Output => _outputStringBuilder.ToString();

        /// <summary>
        /// Gets the error of command.
        /// </summary>
        public string Error => _errorStringBuilder.ToString();

        /// <summary>
        /// Starts the command by starting the <see cref="Process"/> instance.
        /// </summary>
        /// <returns>The same instance.</returns>
        /// <exception cref="ObjectDisposedException">Cannot access a disposed object.</exception>
        /// <exception cref="CliCommandException">The command has already been started.</exception>
        public CliCommand Start()
        {
            EnsureIsNotDisposed();

            if (_isStarted)
                throw CliCommandException.CreateForAlreadyStartedCommand(CommandText, StartInfo.WorkingDirectory);

            try
            {
                _process.Start();
                _isStarted = true;

                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }
            catch (Exception exception)
            {
                Dispose();
                throw CliCommandException.CreateForProcessStartException(CommandText, StartInfo.WorkingDirectory, exception);
            }

            return this;
        }

        /// <summary>
        /// Waits for the associated process to exit with optional timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The command result.</returns>
        /// <exception cref="ObjectDisposedException">Cannot access a disposed object.</exception>
        /// <exception cref="CliCommandException">The command was not started.</exception>
        public CliCommandResult WaitForExit(TimeSpan? timeout = null)
        {
            EnsureIsNotDisposed();

            if (!_isStarted)
                throw CliCommandException.CreateForNotStartedCommand(CommandText, StartInfo.WorkingDirectory);

            int timeoutMilliseconds = -1;

            if (timeout != null)
                timeoutMilliseconds = (int)timeout.Value.TotalMilliseconds;

            try
            {
                if (_process.WaitForExit(timeoutMilliseconds) && _exitResetEvent.Wait(timeoutMilliseconds))
                    return _result;
                else
                    throw CliCommandException.CreateForTimeout(CommandText, StartInfo.WorkingDirectory);
            }
            finally
            {
                Dispose();
            }
        }

        /// <summary>
        /// Immediately stops the associated process.
        /// </summary>
        /// <returns>The command result.</returns>
        /// <exception cref="ObjectDisposedException">Cannot access a disposed object.</exception>
        /// <exception cref="CliCommandException">The command was not started.</exception>
        public CliCommandResult Kill()
        {
            EnsureIsNotDisposed();

            if (!_isStarted)
                throw CliCommandException.CreateForNotStartedCommand(CommandText, StartInfo.WorkingDirectory);

            try
            {
                _process.Kill();
                _exitResetEvent.Wait();
                return _result;
            }
            finally
            {
                Dispose();
            }
        }

        private void OnProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                _outputResetEvent.Set();
            }
            else
            {
                if (_outputStringBuilder.Length > 0)
                    _outputStringBuilder.AppendLine();

                _outputStringBuilder.Append(e.Data);
            }
        }

        private void OnProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                _errorResetEvent.Set();
            }
            else
            {
                if (_errorStringBuilder.Length > 0)
                    _errorStringBuilder.AppendLine();

                _errorStringBuilder.Append(e.Data);
            }
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            _outputResetEvent.Wait();
            _errorResetEvent.Wait();

            CollectResult();

            _exitResetEvent.Set();
        }

        private void CollectResult() =>
            _result = new CliCommandResult
            {
                CommandText = CommandText,
                WorkingDirectory = StartInfo.WorkingDirectory,
                ExitCode = _process.ExitCode,
                Output = _outputStringBuilder.ToString(),
                Error = _errorStringBuilder.ToString()
            };

        private void EnsureIsNotDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _process.Dispose();
                    _outputResetEvent.Dispose();
                    _errorResetEvent.Dispose();
                    _exitResetEvent.Dispose();

                    _outputStringBuilder.Clear();
                    _errorStringBuilder.Clear();
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
