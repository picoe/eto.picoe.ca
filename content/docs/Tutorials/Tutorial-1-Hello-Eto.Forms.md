---
title: "Tutorial 1 - Your First App"
---

This tutorial walks through the app the template creates, then adds a text box and a button to it.

## Create the app

Follow the [Quick Start](../QuickStart) to create a new app, choosing the **Code** form type in Visual Studio, or:

```bash
dotnet new etoapp -o MyApp
```

Run it with `dotnet run` (or F5 in your IDE) and you'll see a window with a menu, a toolbar and "Hello World!".

## What the template gives you

**Program.cs** starts the app. `Platform.Detect` picks the platform you built for, so the same code runs everywhere:

```csharp
[STAThread]
static void Main(string[] args)
{
    var app = new Application(Eto.Platform.Detect);
    app.Run(new MainForm());
}
```

**MainForm.cs** is your main window. Its `Content` is the control shown inside the window, here a `StackLayout` that lists controls from top to bottom:

```csharp
Content = new StackLayout
{
    Padding = 10,
    Items =
    {
        "Hello World!",
        // add more controls here
    }
};
```

A string in a layout is shorthand for a `Label`. The rest of the constructor builds the menu and toolbar, which [Tutorial 2](./Tutorial-2-Menus-and-Toolbars) covers.

## Add some controls

Replace the `Content` with a text box and a button that greets whoever's name is typed in:

```csharp
var nameBox = new TextBox { PlaceholderText = "Your name" };

var helloButton = new Button { Text = "Say Hello" };
helloButton.Click += (sender, e) => MessageBox.Show(this, $"Hello, {nameBox.Text}!");

Content = new StackLayout
{
    Padding = 10,
    Spacing = 5,
    Items =
    {
        "What's your name?",
        nameBox,
        helloButton
    }
};
```

Run the app again and try it out. Each control uses the native one for the platform, so it looks at home on Windows, macOS and Linux.

## Other ways to write the UI

The same form can be written in other ways. Pick one when you create the app:

- **Code Preview** (`-m preview`): the layout goes in `MainForm.eto.cs`, which the [live preview](../DevExtensions/Live-Preview.md) can draw as you type.
- **Xaml** (`-m xaml`): the layout goes in `MainForm.xeto`, with event handlers in `MainForm.xeto.cs`.
- **Json** (`-m json`): the layout goes in `MainForm.jeto`, with event handlers in `MainForm.jeto.cs`.

These tutorials use code, but every control and property works the same way in each.

Next: [Tutorial 2 Menus and Toolbars](./Tutorial-2-Menus-and-Toolbars)
