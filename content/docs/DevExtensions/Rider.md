---
title: "Rider"
weight: 5
---

The **Eto.Forms Designer** plugin works in JetBrains Rider 2025.1 or newer, with the same features as the VS Code extension.

It isn't on the JetBrains Marketplace yet. To install it:

1. Download the `.zip` from the [latest release](https://github.com/picoe/Eto.DevExtensions/releases/latest).
2. In Rider, open **Settings > Plugins**, click **⚙** and choose **Install Plugin from Disk**.
3. Pick the downloaded `.zip` and restart Rider.

It needs the [.NET 8 runtime](https://dotnet.microsoft.com/download) or newer. To create a new project, use [dotnet new](../QuickStart/_index.md#command-line-with-dotnet-new).

## The preview

The **Eto Preview** tool window appears once you open a `.xeto`, `.jeto` or `.eto.cs` file. Open it from its button on the right-hand side, **View > Tool Windows**, or **Open Eto Preview** in the editor's right-click menu. It follows the file you're editing.

Along with the [live preview](Live-Preview.md), the tool window has a **Log** tab showing the preview's output, which helps when a view doesn't draw as expected.

## Switching between designer and code

**Switch Between Eto Designer and Code** in the editor's right-click menu and the **Navigate** menu jumps between a view and its code behind. Give it a shortcut in **Settings > Keymap**.

## Settings

**Settings > Tools > Eto.Forms Designer** has the same options as the [VS Code settings](VS-Code.md#settings): the `dotnet` executable, the language server and preview host paths, and the folder of the `Eto.dll` to complete against.

## One Eto.Forms version at a time

IntelliSense loads one Eto.Forms version at a time. After switching to a project that uses a different version, run **Tools > Restart Eto Language Server**.

## Running Mac apps

When running an Eto.Mac app, set the startup program to the executable inside the `.app` bundle, e.g. `bin/Debug/net8.0/MyApp.Mac.app/Contents/MacOS/MyApp.Mac`.
