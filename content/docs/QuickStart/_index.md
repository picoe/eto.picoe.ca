---
title: "Quick Start"
weight: 2
aliases:
  - /docs/quickstart/creating-a-project-manually/
  - /docs/quickstart/running-your-application/
---

The easiest way to start an Eto.Forms app is from the project templates, either in Visual Studio or with `dotnet new` from the command line.

## Visual Studio

Install the [Eto.Forms extension for Visual Studio](../DevExtensions/Visual-Studio.md), then create a new project using the **Eto.Forms Application** template.

## Command line with dotnet new

With the [.NET SDK](https://dotnet.microsoft.com/download) installed, install the templates and create an app:

```bash
dotnet new install Eto.Forms.Templates
dotnet new etoapp -o MyApp
cd MyApp
dotnet run
```

This works the same in VS Code, Rider or any other editor. Install the [Eto.Forms Designer extension](../DevExtensions/VS-Code.md) for VS Code, or the [plugin for Rider](../DevExtensions/Rider.md), for a live preview and IntelliSense.

Run `dotnet new etoapp --help` to see all options. The most useful ones are:

| Option | What it does |
| --- | --- |
| `-m`, `--mode` | How the UI is written: `code` (default), `preview` (C# with a live preview), `xaml` or `json`. |
| `-f`, `--framework` | Target framework, from `net10.0` (default) back to `net462`. |
| `-sp`, `--separate-projects` | Creates a separate launcher project per platform instead of a single project. |
| `-sln`, `--include-solution` | Adds a solution file. |

To add a new `Panel`, `Dialog` or `Form` to your project, use `dotnet new etofile -n MyDialog -b Dialog`.

## Building for each platform

By default the template creates a single project that builds for the OS you are on: WPF on Windows, Mac64 on macOS and GTK on Linux. Set the `BuildPlatform` property to build for another one:

```bash
dotnet build -p:BuildPlatform=Gtk
```

Valid values are `Wpf`, `WinForms`, `Mac64`, `MacOS` and `Gtk`. See [Supported Platforms](./Supported-Platforms) for what each one needs.

If you prefer one project per platform, create the app with `--separate-projects`. Your UI goes in a shared library, and each platform gets a small launcher project.

## Next steps

- Follow the [Tutorials](../Tutorials) to build out your first app.
- When you're ready to ship, see [Publishing Your Application](../Guides/Publishing-your-app).
