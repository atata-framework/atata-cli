namespace Atata.Cli;

/// <summary>
/// Represents the program CLI.
/// </summary>
public class ProgramCli
{
    private static readonly ICliCommandFactory s_directCliCommandFactory = new DirectCliCommandFactory();

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgramCli"/> class.
    /// </summary>
    /// <param name="fileNameOrCommand">The file name or command.</param>
    /// <param name="useCommandShell">If set to <see langword="true"/> uses command shell (cmd/bash).</param>
    public ProgramCli(string fileNameOrCommand, bool useCommandShell = false)
        : this(
            fileNameOrCommand,
            useCommandShell ? DefaultShellCliCommandFactory : s_directCliCommandFactory)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgramCli"/> class.
    /// </summary>
    /// <param name="fileNameOrCommand">The file name or command.</param>
    /// <param name="commandFactory">The command factory.</param>
    public ProgramCli(string fileNameOrCommand, ICliCommandFactory commandFactory)
    {
        Guard.ThrowIfNullOrWhitespace(fileNameOrCommand);
        Guard.ThrowIfNull(commandFactory);

        FileNameOrCommand = fileNameOrCommand;
        CliCommandFactory = commandFactory;
    }

#if NETFRAMEWORK
    /// <summary>
    /// Gets or sets the default shell <see cref="ICliCommandFactory"/> instance.
    /// The default value is an instance of <see cref="CmdShellCliCommandFactory"/>.
    /// </summary>
    public static ICliCommandFactory DefaultShellCliCommandFactory { get; set; } =
        new CmdShellCliCommandFactory();
#else
    /// <summary>
    /// Gets or sets the default shell <see cref="ICliCommandFactory"/> instance.
    /// The default value is <see cref="OSDependentShellCliCommandFactory.UseCmdForWindowsAndShForOthers"/>.
    /// </summary>
    public static ICliCommandFactory DefaultShellCliCommandFactory { get; set; } =
        OSDependentShellCliCommandFactory.UseCmdForWindowsAndShForOthers();
#endif

    /// <summary>
    /// Gets the program file name or command.
    /// </summary>
    public string FileNameOrCommand { get; }

    /// <summary>
    /// Gets or sets the CLI command factory.
    /// </summary>
    public ICliCommandFactory CliCommandFactory { get; set; }

    /// <summary>
    /// Gets a value indicating whether to use command shell (cmd, bash, etc.).
    /// </summary>
    public bool UseCommandShell =>
        CliCommandFactory is ShellCliCommandFactory;

    /// <summary>
    /// Gets or sets the working directory.
    /// </summary>
    public string WorkingDirectory { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// Gets or sets the encoding to use.
    /// </summary>
    public Encoding? Encoding { get; set; }

    /// <summary>
    /// Gets or sets the wait for exit timeout.
    /// </summary>
    public TimeSpan? WaitForExitTimeout { get; set; }

    /// <summary>
    /// Gets the list of configuration actions of process <see cref="Process.StartInfo"/>.
    /// </summary>
    public List<Action<ProcessStartInfo>> ProcessStartInfoConfigurationActions { get; } = [];

    /// <summary>
    /// Gets or sets the command result validation rules that are performed in
    /// <see cref="Execute(string)"/> and <see cref="ExecuteAsync(string?, CancellationToken)"/>
    /// methods and produce <see cref="CliCommandException"/> throwing.
    /// The default value is <see cref="CliCommandResultValidationRules.ZeroExitCode"/>.
    /// </summary>
    public CliCommandResultValidationRules ResultValidationRules { get; set; } = CliCommandResultValidationRules.ZeroExitCode;

    /// <summary>
    /// Adds the process <see cref="Process.StartInfo"/> configuration.
    /// </summary>
    /// <param name="configurationAction">The configuration action.</param>
    /// <returns>The same instance.</returns>
    public ProgramCli AddProcessStartInfoConfiguration(Action<ProcessStartInfo> configurationAction)
    {
        Guard.ThrowIfNull(configurationAction);

        ProcessStartInfoConfigurationActions.Add(configurationAction);

        return this;
    }

    /// <summary>
    /// Sets the CLI command factory.
    /// </summary>
    /// <param name="cliCommandFactory">The CLI command factory.</param>
    /// <returns>The same instance.</returns>
    public ProgramCli WithCliCommandFactory(ICliCommandFactory cliCommandFactory)
    {
        Guard.ThrowIfNull(cliCommandFactory);

        CliCommandFactory = cliCommandFactory;

        return this;
    }

    /// <summary>
    /// Sets the working directory.
    /// </summary>
    /// <param name="workingDirectory">The working directory.</param>
    /// <returns>The same instance.</returns>
    public ProgramCli WithWorkingDirectory(string workingDirectory)
    {
        Guard.ThrowIfNullOrWhitespace(workingDirectory);

        WorkingDirectory = workingDirectory;

        return this;
    }

    /// <summary>
    /// Starts the program with the specified arguments.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <returns>The started <see cref="CliCommand"/> instance.</returns>
    public CliCommand Start(string? arguments = null)
    {
        CliCommand command = CliCommandFactory.Create(FileNameOrCommand, arguments);
        FillStartInfo(command.StartInfo);

        return command.Start();
    }

    private void FillStartInfo(ProcessStartInfo startInfo)
    {
        startInfo.WorkingDirectory = WorkingDirectory;

        if (Encoding != null)
        {
            startInfo.StandardOutputEncoding = Encoding;
            startInfo.StandardErrorEncoding = Encoding;
        }

        foreach (var configurationAction in ProcessStartInfoConfigurationActions)
            configurationAction.Invoke(startInfo);
    }

    /// <summary>
    /// Starts the program with the specified arguments and waits until it exits.
    /// Throws <see cref="CliCommandException"/> if program result doesn't meet validation rules
    /// of <see cref="ResultValidationRules"/> property.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <returns>The <see cref="CliCommandResult"/> instance.</returns>
    public CliCommandResult Execute(string? arguments = null)
    {
        CliCommandResult result = ExecuteRaw(arguments);
        ValidateResult(result);
        return result;
    }

    private void ValidateResult(CliCommandResult result)
    {
        if ((ResultValidationRules.HasFlag(CliCommandResultValidationRules.ZeroExitCode) && result.ExitCode != 0)
            || (ResultValidationRules.HasFlag(CliCommandResultValidationRules.NoError) && result.HasError))
            throw CliCommandException.CreateForErrorResult(result);
    }

    /// <inheritdoc cref="Execute(string)"/>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<CliCommandResult> ExecuteAsync(CancellationToken cancellationToken) =>
        ExecuteAsync(null, cancellationToken);

    /// <inheritdoc cref="Execute(string)"/>
    /// <param name="arguments">The arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<CliCommandResult> ExecuteAsync(string? arguments = null, CancellationToken cancellationToken = default)
    {
        CliCommandResult result = await ExecuteRawAsync(arguments, cancellationToken).ConfigureAwait(false);
        ValidateResult(result);
        return result;
    }

    /// <summary>
    /// Starts the program with the specified arguments and waits until it exits.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <returns>The <see cref="CliCommandResult"/> instance.</returns>
    public CliCommandResult ExecuteRaw(string? arguments = null)
    {
        using var command = Start(arguments);

        return command.WaitForExit(WaitForExitTimeout);
    }

    /// <inheritdoc cref="ExecuteRaw(string)"/>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<CliCommandResult> ExecuteRawAsync(CancellationToken cancellationToken) =>
        ExecuteRawAsync(null, cancellationToken);

    /// <inheritdoc cref="ExecuteRaw(string)"/>
    /// <param name="arguments">The arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<CliCommandResult> ExecuteRawAsync(string? arguments = null, CancellationToken cancellationToken = default)
    {
        using var command = Start(arguments);

        TimeSpan? timeout = WaitForExitTimeout;

        if (timeout is not null && timeout.Value > TimeSpan.Zero)
        {
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cancellationTokenSource.CancelAfter(timeout.Value);

            return await command.WaitForExitAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        }
        else
        {
            return await command.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
