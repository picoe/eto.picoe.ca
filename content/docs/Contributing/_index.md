---
title: "Contributing"
weight: 7
---

## Code conventions

We use [EditorConfig](http://editorconfig.org) to automatically use correct coding conventions.

## System Requirements

These are the system requirements for developing and/or contributing to Eto.Forms. Note that these are not necessarily the same requirements for running Eto.Forms apps, which are outlined in [Supported Platforms](../QuickStart/Supported-Platforms).

### Windows

On Windows, you can target WPF, WinForms and GTK.

* [Visual Studio 2022 or 2026](https://visualstudio.com), [Visual Studio Code](https://code.visualstudio.com) or [Rider](https://www.jetbrains.com/rider/)
* [.NET SDK](https://dotnet.microsoft.com/download)
* (optional) For GTK: [GTK 3](https://www.gtk.org/docs/installations/windows/)

Notes:

* Can build but not run Mac64 applications, and can't build macOS applications

### macOS

On macOS, you can target Mac64, macOS and GTK.

* [Visual Studio Code](https://code.visualstudio.com) or [Rider](https://www.jetbrains.com/rider/)
* [.NET SDK](https://dotnet.microsoft.com/download)
* (optional) For macOS: [.NET macos workload](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-workload-install)
* (optional) For GTK: [GTK 3](https://formulae.brew.sh/formula/gtk+3)

Notes:

* Can build but not run WPF or WinForms applications

### Linux

On Linux, you can target GTK.

* [Visual Studio Code](https://code.visualstudio.com) or [Rider](https://www.jetbrains.com/rider/)
* [.NET SDK](https://dotnet.microsoft.com/download)
* GTK 3.20 or later

Notes:

* Can build but not run WPF, WinForms or Mac64 applications, and can't build macOS applications

## Building

To build, open **src/Eto.slnx** in your IDE, or use the VS Code tasks.

To build Eto.Forms and associated NuGet packages outside of your IDE, you can run **build.sh** (macOS/Linux) or **build.cmd** (Windows) from the root of the repository.
You can also pass a version to these scripts, e.g. `./build.sh 2.14.0` or `build.cmd 2.14.0`. See [Versioning](./Versioning) for how versions and releases work.

## Pull Requests

Ensure your pull request builds and passes all checks. A check or X will show next to your pull request on github showing you the build status.

## Contributing Guidelines

1. **You agree to the licensing terms**

Eto.Forms is [BSD-3 Licensed](http://opensource.org/licenses/BSD-3-Clause). By sending a pull request, you agree to license your contributions by the same license.
