# Atata.Cli

[![NuGet](http://img.shields.io/nuget/v/Atata.Cli.svg?style=flat)](https://www.nuget.org/packages/Atata.Cli/)
[![GitHub release](https://img.shields.io/github/release/atata-framework/atata-cli.svg)](https://github.com/atata-framework/atata-cli/releases)
[![Build status](https://dev.azure.com/atata-framework/atata-cli/_apis/build/status/atata-cli-ci?branchName=main)](https://dev.azure.com/atata-framework/atata-cli/_build/latest?definitionId=41&branchName=main)
[![Slack](https://img.shields.io/badge/join-Slack-green.svg?colorB=4EB898)](https://join.slack.com/t/atata-framework/shared_invite/zt-5j3lyln7-WD1ZtMDzXBhPm0yXLDBzbA)
[![Atata docs](https://img.shields.io/badge/docs-Atata_Framework-orange.svg)](https://atata.io)
[![Twitter](https://img.shields.io/badge/follow-@AtataFramework-blue.svg)](https://twitter.com/AtataFramework)

**Atata.Cli** is a .NET library that provides an API for CLI.

*Targets .NET Standard 2.0*

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Feedback](#feedback)
- [SemVer](#semver)
- [License](#license)

## Features

- Provides an abstraction over `System.Diagnostics.Process` with `CliCommand` and `ProgramCli` classes.
- Has ability to execute CLI through command shell (cmd/bash).
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
