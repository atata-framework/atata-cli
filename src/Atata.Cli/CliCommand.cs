namespace Atata.Cli;

/// <summary>
/// Represents the CLI command.
/// It is one-time executable and not reusable.
/// Uses <see cref="Process"/> class to execute a command.
/// </summary>
public class CliCommand : IDisposable
{
    private readonly Process _process;

    private readonly StringBuilder _outputStringBuilder = new();

    private readonly StringBuilder _errorStringBuilder = new();

    private readonly StringBuilder _mergedOutputStringBuilder = new();

    private readonly object _mergedOutputSyncLock = new();

    private readonly Lazy<string> _lazyResultOutput;

    private readonly Lazy<string> _lazyResultError;

    private readonly Lazy<string> _lazyResultMergedOutput;

    private readonly ManualResetEventSlim _outputResetEvent = new();

    private readonly ManualResetEventSlim _errorResetEvent = new();

    private readonly ManualResetEventSlim _exitResetEvent = new();

    private CliCommandResult? _result;

    private bool _isStarted;

    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CliCommand"/> class.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="arguments">The arguments.</param>
    public CliCommand(string fileName, string? arguments = null)
    {
        _process = new Process
        {
            StartInfo = new ProcessStartInfo(fileName, arguments ?? string.Empty)
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

        _lazyResultOutput = new(_outputStringBuilder.ToString);
        _lazyResultError = new(_errorStringBuilder.ToString);
        _lazyResultMergedOutput = new(_mergedOutputStringBuilder.ToString);
    }

    /// <summary>
    /// Gets the process.
    /// </summary>
    public Process Process => _process;

    /// <summary>
    /// Gets the process <see cref="ProcessStartInfo"/> instance that can be configured.
    /// </summary>
    public ProcessStartInfo StartInfo => _process.StartInfo;

#if NET8_0_OR_GREATER
    /// <summary>
    /// Gets or sets the target to kill on dispose.
    /// The default value is <see cref="CliCommandKillOnDispose.EntireProcessTree"/>.
    /// </summary>
    public CliCommandKillOnDispose KillOnDispose { get; set; } =
        CliCommandKillOnDispose.EntireProcessTree;
#else
    /// <summary>
    /// Gets or sets the target to kill on dispose.
    /// The default value is <see cref="CliCommandKillOnDispose.Process"/>.
    /// </summary>
    public CliCommandKillOnDispose KillOnDispose { get; set; } =
        CliCommandKillOnDispose.Process;
#endif

    /// <summary>
    /// Gets the executable command text.
    /// </summary>
    public string CommandText
    {
        get
        {
            StringBuilder builder = new();

            if (StartInfo.FileName?.Length > 0)
            {
                builder.Append(StartInfo.FileName);

                if (StartInfo.Arguments?.Length > 0)
                    builder.Append(' ').Append(StartInfo.Arguments);
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// Gets the command standard output (stdout).
    /// </summary>
    public string Output =>
        _result is null
            ? _outputStringBuilder.ToString()
            : GetResultOutput();

    /// <summary>
    /// Gets the command standard error (stderr).
    /// </summary>
    public string Error =>
        _result is null
            ? _errorStringBuilder.ToString()
            : GetResultError();

    /// <summary>
    /// Gets the command merged output: <see cref="Output"/> + <see cref="Error"/>.
    /// </summary>
    public string MergedOutput =>
        _result is null
            ? _mergedOutputStringBuilder.ToString()
            : GetResultMergedOutput();

    /// <summary>
    /// Gets the result of command.
    /// Until the command is executed, the property returns <see langword="null"/>.
    /// </summary>
    public CliCommandResult? Result =>
        _result;

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

        int timeoutMilliseconds = ConvertTimeoutToMilliseconds(timeout);

        if (!_process.HasExited)
        {
            var stopwatch = Stopwatch.StartNew();
            bool hasExitedAfterWait = _process.WaitForExit(timeoutMilliseconds);

            if (!hasExitedAfterWait)
                throw CliCommandException.CreateForTimeout(CommandText, StartInfo.WorkingDirectory);

            if (timeoutMilliseconds != -1)
            {
                timeoutMilliseconds -= (int)stopwatch.ElapsedMilliseconds;
                timeoutMilliseconds = Math.Max(timeoutMilliseconds, 0);
            }
        }

        if (!_exitResetEvent.Wait(timeoutMilliseconds))
            throw CliCommandException.CreateForTimeout(CommandText, StartInfo.WorkingDirectory);

        return ResolveResult();
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Waits asynchronously for the associated process to exit.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> of <see cref="CliCommandResult"/>.</returns>
    public async Task<CliCommandResult> WaitForExitAsync(CancellationToken cancellationToken = default)
    {
        EnsureIsNotDisposed();

        if (!_isStarted)
            throw CliCommandException.CreateForNotStartedCommand(CommandText, StartInfo.WorkingDirectory);

        if (!_process.HasExited)
            await _process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        _exitResetEvent.Wait(cancellationToken);

        return ResolveResult();
    }
#endif

    private CliCommandResult ResolveResult() =>
        _result ?? throw CliCommandException.CreateForResultResolveFailure(CommandText, StartInfo.WorkingDirectory);

    /// <summary>
    /// Immediately stops the associated process.
    /// </summary>
    /// <returns>The command result.</returns>
    /// <exception cref="ObjectDisposedException">Cannot access a disposed object.</exception>
    /// <exception cref="CliCommandException">The command was not started.</exception>
    public CliCommandResult Kill() =>
        DoKill(false);

#if NET8_0_OR_GREATER
    /// <summary>
    /// Immediately stops the associated process, and optionally its child/descendent processes.
    /// </summary>
    /// <param name="entireProcessTree">
    /// <see langword="true"/> to kill the associated process and its descendants;
    /// <see langword="false"/> to kill only the associated process.
    /// </param>
    /// <returns>The command result.</returns>
    /// <exception cref="ObjectDisposedException">Cannot access a disposed object.</exception>
    /// <exception cref="CliCommandException">The command was not started.</exception>
    public CliCommandResult Kill(bool entireProcessTree) =>
        DoKill(entireProcessTree);
#endif

    private CliCommandResult DoKill(bool entireProcessTree)
    {
        EnsureIsNotDisposed();

        if (!_isStarted)
            throw CliCommandException.CreateForNotStartedCommand(CommandText, StartInfo.WorkingDirectory);

        if (!_process.HasExited)
        {
            if (entireProcessTree)
                KillEntireProcessTree();
            else
                _process.Kill();
        }

        _exitResetEvent.Wait();
        return ResolveResult();
    }

    [SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static")]
    private void KillEntireProcessTree() =>
#if NETFRAMEWORK
        throw new NotSupportedException("Killing entire process tree is not supported in .NET Framework.");
#else
        _process.Kill(true);
#endif

    private void OnProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is null)
        {
            _outputResetEvent.Set();
        }
        else
        {
            lock (_mergedOutputSyncLock)
            {
                AppendLineToStringBuilder(e.Data, _outputStringBuilder);
                AppendLineToStringBuilder(e.Data, _mergedOutputStringBuilder);
            }
        }
    }

    private void OnProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is null)
        {
            _errorResetEvent.Set();
        }
        else
        {
            lock (_mergedOutputSyncLock)
            {
                AppendLineToStringBuilder(e.Data, _errorStringBuilder);
                AppendLineToStringBuilder(e.Data, _mergedOutputStringBuilder);
            }
        }
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        _outputResetEvent.Wait();
        _errorResetEvent.Wait();

        CollectResult();

        _exitResetEvent.Set();
    }

    private void CollectResult() =>
        _result = new CliCommandResult(
            CommandText,
            StartInfo.WorkingDirectory,
            _process.ExitCode,
            GetResultOutput,
            GetResultError,
            GetResultMergedOutput);

    private string GetResultOutput() =>
        _lazyResultOutput.Value;

    private string GetResultError() =>
        _lazyResultError.Value;

    private string GetResultMergedOutput() =>
        _lazyResultMergedOutput.Value;

    private void EnsureIsNotDisposed() =>
        Guard.ThrowIfDisposed(_isDisposed, this);

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                KillOnDisposeIfStartedAndNotExited();

                _process.Dispose();
                _outputResetEvent.Dispose();
                _errorResetEvent.Dispose();
                _exitResetEvent.Dispose();
            }

            _isDisposed = true;
        }
    }

    [SuppressMessage("Major Code Smell", "S1066:Mergeable \"if\" statements should be combined")]
    private void KillOnDisposeIfStartedAndNotExited()
    {
        if (_isStarted)
        {
#if NET8_0_OR_GREATER
            if (KillOnDispose is CliCommandKillOnDispose.EntireProcessTree)
                DoKill(true);
#endif

            if (KillOnDispose is CliCommandKillOnDispose.Process)
                DoKill(false);
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

    private static int ConvertTimeoutToMilliseconds(TimeSpan? timeout)
    {
        if (timeout is null)
            return -1;

        if (timeout.Value < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout cannot be negative.");

        return (int)timeout.Value.TotalMilliseconds;
    }

    private static void AppendLineToStringBuilder(string line, StringBuilder stringBuilder)
    {
        if (stringBuilder.Length > 0)
            stringBuilder.AppendLine();

        stringBuilder.Append(line);
    }
}
