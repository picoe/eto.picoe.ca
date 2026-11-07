---
title: "Live Preview"
weight: 1
---

The live preview draws your view beside the editor while you work on it. It redraws as you type and after each build, so you can see changes without running your app.

How to open it depends on your editor: see [Visual Studio](Visual-Studio.md#the-preview), [VS Code](VS-Code.md#the-preview) or [Rider](Rider.md#the-preview).

## What can be previewed

The preview works with three kinds of view, and finds each one's code behind by its file name:

| View | Written in | Code behind |
| --- | --- | --- |
| `MyForm.xeto` | XAML | `MyForm.xeto.cs` |
| `MyForm.jeto` | JSON | `MyForm.jeto.cs` |
| `MyForm.eto.cs` | C# | `MyForm.cs` |

An `.eto.cs` file is compiled on its own for the preview, so it should hold a single class derived from a control (e.g. `Panel`, `Dialog` or `Form`) with a parameterless constructor that builds the UI. The templates create views laid out this way, with the event handlers and other logic in the separate `MyForm.cs`.

## Platform and theme

Drop downs above the preview pick which platform draws it and which theme it uses:

- **Platform**: WPF or WinForms on Windows, macOS or Mac64 on macOS, and GTK anywhere GTK 3 is installed (on macOS, from Homebrew or MacPorts). **Auto** picks the one your solution uses.
- **Theme**: **Default** uses the platform's own look; the others are the themes that platform provides, such as light and dark.

The preview runs your project's code in a separate process, so a crash or hang in your view can't take down your editor.

## Trying other sizes

Drag the handle at the bottom-right corner of the preview to resize the view, e.g. to check how a layout stretches. In Rider, click the size label to go back to the view's own size.

## Your own controls

The preview loads your project's compiled output, so custom controls and any changes to them appear after you build. Errors in your view are shown in the preview rather than as an empty pane.

## Sample data in the preview

Views that use `{Binding}` usually look empty in the preview, because nothing sets their `DataContext`. Give the view a **design data context** to fill it with sample data. It's only used by the preview and IntelliSense, never when your app runs, and it also makes `{Binding ...}` complete the properties of that type.

This needs Eto.Forms 2.13 or newer in your project.

### In a .xeto file

Add the `d` namespace and a `d:DataContext`:

```xml
<Panel xmlns="http://schema.picoe.ca/eto.forms"
       xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
       xmlns:d="http://schema.picoe.ca/eto.forms/design"
       xmlns:local="clr-namespace:MyApp;assembly=MyApp"
       x:Class="MyApp.MyPanel"
       d:DataContext="{d:DesignInstance local:MyViewModel}">
  <TextBox Text="{Binding Name}" />
</Panel>
```

### In a .jeto file

Give either the type name:

```json
{
  "$type": "Panel",
  "d:DataContext": "MyApp.MyViewModel, MyApp",
  "Content": { "$type": "TextBox", "Text": "{Binding Name}" }
}
```

or an object with a `$type` and the sample values to use:

```json
"d:DataContext": { "$type": "MyApp.MyViewModel, MyApp", "Name": "Sample name" }
```

### In code

In `.eto.cs` views, or anywhere else in your code, check `Control.IsDesignMode` to tell when you're running in the preview:

```csharp
if (IsDesignMode)
    DataContext = new MyViewModel { Name = "Sample name" };
```

### Things to know

- The type needs a public parameterless constructor to show sample data. Without one, `{Binding}` still completes its properties but the preview stays empty.
- Only the file being previewed uses its `d:DataContext`. A user control inside it gets its data from the parent, as it would at runtime.

See [Data Binding](../Guides/Data-Binding.md) for more on bindings and view models.
