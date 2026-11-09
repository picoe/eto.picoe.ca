---
title: "Tutorial 2 - Menus and Toolbars"
---

This tutorial builds on [Tutorial 1](./Tutorial-1-Hello-Eto.Forms). It moves the menu and toolbar actions into a view model, then adds to the menu and toolbar.

Menus and toolbars work much the same on every desktop platform, with a few macOS differences that Eto.Forms handles for you:

- macOS has an application menu (named after your app) that holds About, Preferences and Quit.
- macOS needs certain menu items, like Cut, Copy, Paste and Undo, for those standard actions to work. Eto.Forms adds them unless you turn this off.

## Commands

A `Command` holds what a menu or toolbar item does, along with its text and shortcut. The same command can appear in both the menu and the toolbar, and enabling or disabling it updates both.

There are three kinds:

- `Command`: runs an action when clicked.
- `CheckCommand`: toggles on or off.
- `RadioCommand`: picks one of a group of options.

You can also add `ButtonMenuItem`, `CheckMenuItem`, `RadioMenuItem` and `SeparatorMenuItem` directly to menus, and `ButtonToolItem`, `CheckToolItem`, `RadioToolItem` and `SeparatorToolItem` to toolbars.

## A view model for the commands

Rather than creating commands in the form, put them in a view model. This keeps what your app does separate from how it looks, and lets XAML and JSON views use the same commands. Add this class to your project:

```csharp {title="MainViewModel.cs"}
using Eto.Forms;

public class MainViewModel
{
    public Command ClickMeCommand { get; } = new Command { MenuText = "Click Me!", ToolBarText = "Click Me!" };

    public Command QuitCommand { get; } = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };

    public Command AboutCommand { get; } = new Command { MenuText = "About..." };

    public MainViewModel()
    {
        ClickMeCommand.Executed += (sender, e) => MessageBox.Show("I was clicked!");
        QuitCommand.Executed += (sender, e) => Application.Instance.Quit();
        AboutCommand.Executed += (sender, e) => new AboutDialog().ShowDialog(Application.Instance.MainForm);
    }
}
```

## The menu bar

Create the view model in your form, set it as the form's `DataContext`, and use its commands in the menu:

{{< code-tabs >}}

```csharp {title="MainForm.cs"}
var model = new MainViewModel();
DataContext = model;

Menu = new MenuBar
{
    Items =
    {
        new SubMenuItem { Text = "&File", Items = { model.ClickMeCommand } },
    },
    ApplicationItems =
    {
        new ButtonMenuItem { Text = "&Preferences..." },
    },
    QuitItem = model.QuitCommand,
    AboutItem = model.AboutCommand
};
```

```xaml {title="MainForm.xeto"}
<Form.Menu>
  <MenuBar>
    <SubMenuItem Text="&amp;File">
      <ButtonMenuItem Text="Click Me!" Command="{Binding ClickMeCommand}" />
    </SubMenuItem>
    <MenuBar.ApplicationItems>
      <ButtonMenuItem Text="Preferences..." />
    </MenuBar.ApplicationItems>
    <MenuBar.QuitItem>
      <ButtonMenuItem Text="Quit" Shortcut="CommonModifier+Q" Command="{Binding QuitCommand}" />
    </MenuBar.QuitItem>
    <MenuBar.AboutItem>
      <ButtonMenuItem Text="About..." Command="{Binding AboutCommand}" />
    </MenuBar.AboutItem>
  </MenuBar>
</Form.Menu>
```

```csharp {tab="xaml" title="MainForm.xeto.cs"}
public MainForm()
{
    XamlReader.Load(this);
    DataContext = new MainViewModel();
}
```

```json {title="MainForm.jeto"}
"Menu": {
  "Items": [
    {
      "$type": "SubMenuItem",
      "Text": "&File",
      "Items": [
        { "$type": "ButtonMenuItem", "Text": "Click Me!", "Command": "{Binding ClickMeCommand}" }
      ]
    }
  ],
  "ApplicationItems": [
    { "$type": "ButtonMenuItem", "Text": "Preferences..." }
  ],
  "QuitItem": { "$type": "ButtonMenuItem", "Text": "Quit", "Shortcut": "CommonModifier+Q", "Command": "{Binding QuitCommand}" },
  "AboutItem": { "$type": "ButtonMenuItem", "Text": "About...", "Command": "{Binding AboutCommand}" }
}
```

```csharp {tab="json" title="MainForm.jeto.cs"}
public MainForm()
{
    JsonReader.Load(this);
    DataContext = new MainViewModel();
}
```

{{< /code-tabs >}}

- `Items` are the top-level menus. `&` marks the letter used for keyboard access on Windows and Linux.
- `ApplicationItems` go in the application menu on macOS, and in the File menu on other platforms.
- `QuitItem` and `AboutItem` are put in the right place for each platform. If you don't set `QuitItem`, macOS still gets one so users can quit your app.

In XAML and JSON, `{Binding ClickMeCommand}` finds the command in the form's `DataContext`. A bound command supplies what the item does and whether it's checked or enabled, so set the item's `Text` and `Shortcut` in the markup.

## Add a View menu

Add a check command to the view model that keeps the window on top:

```csharp {title="MainViewModel.cs"}
public CheckCommand AlwaysOnTopCommand { get; } = new CheckCommand { MenuText = "Always on &Top", ToolBarText = "On Top" };
```

The window is the form's job rather than the view model's, so have the form react when the command is checked:

{{< code-tabs >}}

```csharp {title="MainForm.cs"}
var model = new MainViewModel();
DataContext = model;

model.AlwaysOnTopCommand.CheckedChanged += (sender, e) => Topmost = model.AlwaysOnTopCommand.Checked;
```

```csharp {tab="xaml" title="MainForm.xeto.cs"}
public MainForm()
{
    XamlReader.Load(this);

    var model = new MainViewModel();
    DataContext = model;

    model.AlwaysOnTopCommand.CheckedChanged += (sender, e) => Topmost = model.AlwaysOnTopCommand.Checked;
}
```

```csharp {tab="json" title="MainForm.jeto.cs"}
public MainForm()
{
    JsonReader.Load(this);

    var model = new MainViewModel();
    DataContext = model;

    model.AlwaysOnTopCommand.CheckedChanged += (sender, e) => Topmost = model.AlwaysOnTopCommand.Checked;
}
```

{{< /code-tabs >}}

Then add a View menu to the menu bar:

{{< code-tabs >}}

```csharp {title="MainForm.cs"}
// in the menu bar's Items
new SubMenuItem { Text = "&View", Items = { model.AlwaysOnTopCommand } },
```

```xaml {title="MainForm.xeto"}
<!-- after the File menu -->
<SubMenuItem Text="&amp;View">
  <CheckMenuItem Text="Always on &amp;Top" Command="{Binding AlwaysOnTopCommand}" />
</SubMenuItem>
```

```json {title="MainForm.jeto"}
/* in the menu's Items, after the File menu */
{
  "$type": "SubMenuItem",
  "Text": "&View",
  "Items": [
    { "$type": "CheckMenuItem", "Text": "Always on &Top", "Command": "{Binding AlwaysOnTopCommand}" }
  ]
}
```

{{< /code-tabs >}}

Radio items are grouped by creating them from `RadioCommand`s that share a `Controller`, so radio groups currently need to be built in code.

## Shortcuts

Set `Shortcut` on a command to give it a keyboard shortcut. Use `Application.Instance.CommonModifier` for the key users expect on each platform, Ctrl on Windows and Linux and Cmd on macOS:

```csharp {title="MainViewModel.cs"}
public Command ClickMeCommand { get; } = new Command
{
    MenuText = "Click Me!",
    ToolBarText = "Click Me!",
    Shortcut = Application.Instance.CommonModifier | Keys.K
};
```

In XAML and JSON, set it on the menu item instead, e.g. `Shortcut="CommonModifier+K"`.

## The toolbar

The toolbar uses the same commands, and its On Top button stays in sync with the View menu:

{{< code-tabs >}}

```csharp {title="MainForm.cs"}
ToolBar = new ToolBar
{
    Items = { model.ClickMeCommand, new SeparatorToolItem(), model.AlwaysOnTopCommand }
};
```

```xaml {title="MainForm.xeto"}
<Form.ToolBar>
  <ToolBar>
    <ButtonToolItem Text="Click Me!" Command="{Binding ClickMeCommand}" />
    <SeparatorToolItem />
    <CheckToolItem Text="On Top" Command="{Binding AlwaysOnTopCommand}" />
  </ToolBar>
</Form.ToolBar>
```

```json {title="MainForm.jeto"}
"ToolBar": {
  "Items": [
    { "$type": "ButtonToolItem", "Text": "Click Me!", "Command": "{Binding ClickMeCommand}" },
    { "$type": "SeparatorToolItem" },
    { "$type": "CheckToolItem", "Text": "On Top", "Command": "{Binding AlwaysOnTopCommand}" }
  ]
}
```

{{< /code-tabs >}}

Set `Image` on a command, or on a tool item in XAML and JSON, to show an icon in the toolbar.

Next: [Tutorial 3 Layout](./Tutorial-3-Table-Layout)
