---
title: "VS Code"
weight: 4
---

The **Eto.Forms Designer** extension works in VS Code and its forks, such as VSCodium, Cursor and Windsurf, on Windows, macOS and Linux.

Install it from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=CurtisWensley.eto-forms-designer) or by searching for *Eto.Forms Designer* in the Extensions view. For VS Code forks, get it from [Open VSX](https://open-vsx.org/extension/CurtisWensley/eto-forms-designer).

It needs the [.NET 8 runtime](https://dotnet.microsoft.com/download) or newer. To create a new project, use [dotnet new](../QuickStart/_index.md#command-line-with-dotnet-new).

## The preview

With a `.xeto`, `.jeto` or `.eto.cs` file open, click the preview button in the editor's title bar, or run **Eto: Open Preview to the Side** from the Command Palette. The [live preview](Live-Preview.md) opens beside the editor and follows the file you're editing.

## Commands

| Command | What it does |
| --- | --- |
| **Eto: Open Preview to the Side** | Opens the live preview for the current view. |
| **Eto: Switch Between Designer and Code** (**Alt+O**) | Jumps between a view and its code behind. |
| **Eto: Restart Language Server** | Restarts IntelliSense, e.g. to pick up a different project's Eto.Forms version. |

## Settings

| Setting | Description |
| --- | --- |
| `eto.dotnetPath` | Path to the `dotnet` executable. Uses `dotnet` from `PATH` when empty. |
| `eto.etoAssemblyPath` | Folder containing the `Eto.dll` to complete against. Detected from the project when empty. |
| `eto.rerunSourceGeneratorsOnSave` | Reruns the C# source generators when a `.xeto` or `.jeto` file is saved, so the code behind sees the fields generated for named controls. Needs the C# extension. On by default. |
| `eto.languageServer.path` | Path to a custom language server. Uses the bundled copy when empty. |
| `eto.previewHost.path` | Path to a custom preview host. Uses the bundled copy when empty. |
| `eto.trace.server` | Logs the traffic between VS Code and the language server, for troubleshooting. |

After changing these settings, run **Eto: Restart Language Server**.

## One Eto.Forms version at a time

IntelliSense loads one Eto.Forms version at a time. After switching to a project that uses a different version, run **Eto: Restart Language Server**.

## Running Mac apps

When debugging an Eto.Mac app, set `program` in your launch configuration to the executable inside the `.app` bundle, e.g. `${workspaceFolder}/MyApp.Mac/bin/Debug/net8.0/MyApp.Mac.app/Contents/MacOS/MyApp.Mac`.
