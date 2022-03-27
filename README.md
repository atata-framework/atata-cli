# Atata.Cli

[![NuGet](http://img.shields.io/nuget/v/Atata.Cli.svg?style=flat)](https://www.nuget.org/packages/Atata.Cli/)
[![GitHub release](https://img.shields.io/github/release/atata-framework/atata-cli.svg)](https://github.com/atata-framework/atata-cli/releases)
[![Build status](https://dev.azure.com/atata-framework/atata-cli/_apis/build/status/atata-cli-ci?branchName=main)](https://dev.azure.com/atata-framework/atata-cli/_build/latest?definitionId=41&branchName=main)
[![Slack](https://img.shields.io/badge/join-Slack-green.svg?colorB=4EB898)](https://join.slack.com/t/atata-framework/shared_invite/zt-5j3lyln7-WD1ZtMDzXBhPm0yXLDBzbA)
[![Atata docs](https://img.shields.io/badge/docs-Atata_Framework-orange.svg)](https://atata.io)
[![Twitter](https://img.shields.io/badge/follow-@AtataFramework-blue.svg)](https://twitter.com/AtataFramework)

**Atata.Cli** is a .NET library that provides an API for CLI.

*The package targets .NET Standard 2.0, which supports .NET 5+, .NET Framework 4.6.1+ and .NET Core/Standard 2.0+.*

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [CLI Command Factories](#cli-command-factories)
- [Feedback](#feedback)
- [SemVer](#semver)
- [License](#license)

## Features

- Provides an abstraction over `System.Diagnostics.Process` with `CliCommand` and `ProgramCli` classes.
- Has ability to execute CLI through command shell: cmd, bash, sudo, etc.
- Provides synchronous and asynchronous API methods.
- Works on Windows, Linux and macOS.

## Installation

Install [`Atata.Cli`](https://www.nuget.org/packages/Atata.Cli/) NuGet package.

- Package Manager:
  ```
  Install-Package Atata.Cli
  ```

- .NET CLI:
  ```
  dotnet add package Atata.Cli
  ```

## Usage

### Execute Command to Get Value

```cs
CliCommandResult result = new ProgramCli("dotnet")
    .Execute("--version");

string version = result.Output;
```

### Execute Command in Directory

```cs
new ProgramCli("dotnet")
    .WithWorkingDirectory("some/path")
    .Execute("build -c Release");
```

### Execute Command Through Command Shell

```cs
new ProgramCli("npm", useCommandShell: true)
    .Execute("install -g html-validate");
```

*The default command shell for Windows is cmd, for other OSs it is bash.*

### Execute Command Through Specific Command Shell

```cs
new ProgramCli("npm", new BashShellCliCommandFactory("-login"))
    .Execute("install -g html-validate");
```

or

```cs
new ProgramCli("npm")
    .WithCliCommandFactory(new BashShellCliCommandFactory("-login"))
    .Execute("install -g html-validate");
```

### Set Default Shell CLI Command Factory

The default shell CLI command factory can be set in a global setup/initialization method.

#### Set for Any Operating System

```cs
ProgramCli.DefaultShellCliCommandFactory = new BashShellCliCommandFactory();
```

#### Set Specific to Operating System

```cs
ProgramCli.DefaultShellCliCommandFactory = OSDependentShellCliCommandFactory
    .UseCmdForWindows()
    .UseForOtherOS(new BashShellCliCommandFactory("-login"));
```

## CLI Command Factories

There are several predefined classes that implement `ICliCommandFactory`:

- `DirectCliCommandFactory` - executes the command directly. The default one.
- `CmdShellCliCommandFactory` - executes the command through the Windows cmd shell program.
- `BashShellCliCommandFactory` - executes the command through the Unix Bash shell program.
- `ShShellCliCommandFactory` - executes the command through the Unix sh shell program.
- `SudoShellCliCommandFactory` - executes the command through the Unix sudo shell program.
- `OSDependentShellCliCommandFactory` - executes the command through one of the registered in it
  shell `ICliCommandFactory` instances depending on the current operating system.
- `UnixShellCliCommandFactory` - executes the command through the specified Unix shell program.
- `ShellCliCommandFactory` - base factory class that executes the command through the specified shell program.

## Feedback

Any feedback, issues and feature requests are welcome.

If you faced an issue please report it to [Atata.Cli Issues](https://github.com/atata-framework/atata-cli/issues),
[ask a question on Stack Overflow](https://stackoverflow.com/questions/ask?tags=atata+csharp) using [atata](https://stackoverflow.com/questions/tagged/atata) tag
or use another [Atata Contact](https://atata.io/contact/) way.

## SemVer

Atata Framework follows [Semantic Versioning 2.0](https://semver.org/).
Thus backward compatibility is followed and updates within the same major version
(e.g. from 1.3 to 1.4) should not require code changes.

## License

Atata is an open source software, licensed under the Apache License 2.0.
See [LICENSE](LICENSE) for details.
