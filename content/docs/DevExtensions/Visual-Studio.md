---
title: "Visual Studio"
weight: 3
---

The Visual Studio extension works with Visual Studio 2022 and 2026 on Windows. Install it from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=CurtisWensley.EtoDevExtensionVisualStudio2022), or:

1. Open **Extensions > Manage Extensions**.
2. Search for *Eto.Forms*.
3. Select the Eto.Forms extension and click **Install**.
4. Restart Visual Studio to finish installing.

## Creating a new project

Create a new project and pick the **Eto.Forms Application** template, in C#, F# or Visual Basic. After naming it, you choose:

- **Framework**: .NET 6 to .NET 10, or .NET Framework 4.6.2 to 4.8.
- **Launcher**: a single **Combined** project that runs on every platform, or **Separate per platform** projects.
- **Platforms** to include, such as WPF, WinForms, GTK and Mac.
- **Form**: how the main form is written:
  - **Xaml**: layout in a `.xeto` file, with code behind for logic and event handlers.
  - **Json**: layout in a `.jeto` file, with code behind.
  - **Code Preview**: layout in a C# `.eto.cs` file that can be previewed, with logic in a separate file.
  - **Code**: everything in a single source file, with no preview.

## Adding a view

Use **Add > New Item** and pick **Eto.Forms View** to add a `Panel`, `Dialog` or `Form`, written in any of the same formats.

## The preview

Opening a `.xeto`, `.jeto` or `.eto.cs` file shows the [live preview](Live-Preview.md) and the code in the same editor tab. The bar between them lets you:

- Click **Design** or **Code** to show only that pane, and click it again to show both.
- Swap the panes, or switch between side by side and above each other.

Visual Studio remembers the layout you choose for the next file you open.

## Other features

See [IntelliSense and Editing](Editing.md) for completion, event handlers, hover documentation and color swatches.
