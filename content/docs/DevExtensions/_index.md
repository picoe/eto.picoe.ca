---
title: "Developer Extensions"
weight: 3
---

The Eto.Forms developer extensions add a live preview, IntelliSense and templates for building Eto.Forms apps in Visual Studio, VS Code (and compatible editors) and JetBrains Rider.

## Features

- **[Live preview](Live-Preview.md)** of `.xeto` (XAML), `.jeto` (JSON) and `.eto.cs` (code) views beside the editor. It redraws as you type and after each build, and you can pick which platform and theme draws it.
- **[IntelliSense](Editing.md)** in `.xeto` and `.jeto` files for controls, properties, events, values and `{Binding}` paths, matched to the Eto.Forms version your project uses.
- **[Event handlers](Editing.md#event-handlers)**: completing an event lists the matching methods in the code behind, or adds a new one.
- **Hover documentation** from the Eto.Forms API docs, and **syntax highlighting** for `.xeto` and `.jeto` files.
- **[Switch between designer and code](Editing.md#switching-between-designer-and-code)** with a single key or click.
- **[Project and item templates](Visual-Studio.md#creating-a-new-project)** for C#, F# and Visual Basic (Visual Studio only; for other editors use [dotnet new](../QuickStart/_index.md#command-line-with-dotnet-new)).

## Installing

| Editor | Where to get it |
| --- | --- |
| Visual Studio 2022 and 2026 (Windows) | [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=CurtisWensley.EtoDevExtensionVisualStudio2022), or **Extensions > Manage Extensions** and search for *Eto.Forms* |
| VS Code | [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=CurtisWensley.eto-forms-designer), or search for *Eto.Forms Designer* in the Extensions view |
| VSCodium, Cursor, Windsurf and other VS Code forks | [Open VSX](https://open-vsx.org/extension/CurtisWensley/eto-forms-designer) |
| Rider 2025.1 or newer | Download the `.zip` from the [latest release](https://github.com/picoe/Eto.DevExtensions/releases/latest), then **Settings > Plugins > ⚙ > Install Plugin from Disk** |

Every [release](https://github.com/picoe/Eto.DevExtensions/releases) also has the `.vsix` files, for installing by hand or trying a pre-release before it reaches the marketplaces. In VS Code use **Extensions: Install from VSIX...**; in Visual Studio, double-click the `.vsix`.

See the page for your editor for how to use it there:

- [Visual Studio](Visual-Studio.md)
- [VS Code](VS-Code.md)
- [Rider](Rider.md)

## Requirements

- The [.NET 8 runtime](https://dotnet.microsoft.com/download) or newer (VS Code and Rider).
- On Windows, the preview also needs the .NET Desktop Runtime 8 or newer.
- On Linux, the preview needs GTK 3.

## Build your project first

The preview and IntelliSense read your project's compiled output. Until the first build only the built-in Eto.Forms controls are known; after you build, your own controls, view models and new properties show up too.

## Getting help

Please report problems or ask for features on the [Eto.DevExtensions issue tracker](https://github.com/picoe/Eto.DevExtensions/issues).
