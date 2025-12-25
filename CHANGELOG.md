# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Add `public Task<CliCommandResult> WaitForExitAsync(CancellationToken)` method to `CliCommand`.
- Add `public CliCommandResult? Result { get; }` property to `CliCommand`.
- Add enum `CliCommandKillOnDispose` with values: `None`, `OnlyProcess`, `EntireProcessTree`.
- Add `public CliCommandKillOnDispose KillOnDispose { get; set; } = CliCommandKillOnDispose.EntireProcessTree` property to `CliCommand`.
- Add `public Task<CliCommandResult> ExecuteAsync(CancellationToken)` method to `ProgramCli`.
- Add `public Task<CliCommandResult> ExecuteRawAsync(CancellationToken)` method to `ProgramCli`.
- Add `public string MergedOutput { get; }` property to `CliCommandResult` and `CliCommand`.

### Changed

- Change package target frameworks from .NET Standard 2.0 to .NET 8.0 and .NET Framework 4.6.2.
- Enable nullable reference types.
- Update `WaitForExit` and `Kill` methods of `CliCommand` to check whether a process has exited.
- Remove internal `Dispose` method call in the end of `WaitForExit` and `Kill` methods of `CliCommand`.
- Add `CancellationToken` parameter to `ExecuteAsync` and `ExecuteRawAsync` methods of `ProgramCli`.

### Removed

- Remove obsolete `CliCommandException.Create` static method.

## [2.2.0] - 2022-07-29

### Added

- Add `CliCommand.Kill(bool entireProcessTree)` method.
- Add `CliCommand.Process` property.

## [2.1.0] - 2022-07-21

### Added

- Add `CliCommandResultValidationRules` enumeration with the flag values: `None`, `ZeroExitCode`, `NoError`, `ZeroExitCodeAndNoError`.
- Add `public CliCommandResultValidationRules ResultValidationRules { get; set; } = CliCommandResultValidationRules.ZeroExitCode` property to `ProgramCli`.

### Changed

- Change default `CliCommandResult` validation in `Execute` and `ExecuteAsync` methods to check `ExitCode == 0` instead of `HasError == false`.
- Add exit code to `CliCommandException` message.

## [2.0.0] - 2022-05-10

### Added

- Add `UseCmdForWindowsAndShForOthers()` static method to `OSDependentShellCliCommandFactory`.

### Changed

- Make `ShellCliCommandFactory` abstract and remove its obsolete behavior.
- Set default value of `ProgramCli.DefaultShellCliCommandFactory` to `OSDependentShellCliCommandFactory.UseCmdForWindowsAndShForOthers()`.

## [1.4.0] - 2022-03-25

### Added

- Add `CmdShellCliCommandFactory` class.
- Add `UnixShellCliCommandFactory` class.
- Add `BashShellCliCommandFactory` class.
- Add `ShShellCliCommandFactory` class.
- Add `SudoShellCliCommandFactory` class.
- Add `OSDependentShellCliCommandFactory` class.
- Add `public static ICliCommandFactory DefaultShellCliCommandFactory { get; set; }`
  property to `ProgramCli`.
- Add `public ICliCommandFactory CliCommandFactory { get; set; }`
  property to `ProgramCli`.
- Add `WithCliCommandFactory(ICliCommandFactory cliCommandFactory)`
  method to `ProgramCli` and `ProgramCli<TCli>`.

### Changed

- Improve `ShellCliCommandFactory` to be stick to a specific shell
  and can be used as a base class for a specific shell CLI command factory.

## [1.3.0] - 2021-07-23

### Added

- Add `string WorkingDirectory` property to `CliCommandResult`.

### Changed

- Change the format of `CliCommandException` message.
  Add "Working directory" to message.

## [1.2.0] - 2021-07-21

### Added

- Add `HasError` property to `CliCommandResult`.
- Add `WithWorkingDirectory(string)` method to `ProgramCli` and `ProgramCli<TCli>`.
- Add `ExecuteRawAsync(string)` method to `ProgramCli`.

### Changed

- Change access modifier of `ProgramCli.ExecuteRaw(string)` method from `protected` to `public`.

## [1.1.0] - 2021-06-24

### Fixed

- Fix `bash` commands execution

## [1.0.0] - 2021-06-24

Initial version release.

[Unreleased]: https://github.com/atata-framework/atata-cli/compare/v2.2.0...HEAD
[2.2.0]: https://github.com/atata-framework/atata-cli/compare/v2.1.0...v2.2.0
[2.1.0]: https://github.com/atata-framework/atata-cli/compare/v2.0.0...v2.1.0
