---
title: "Tutorial 2 - Menus and Toolbars"
---

This tutorial builds on [Tutorial 1](./Tutorial-1-Hello-Eto.Forms) and explains the menu and toolbar the template creates, then adds to them.

Menus and toolbars work much the same on every desktop platform, with a few macOS differences that Eto.Forms handles for you:

- macOS has an application menu (named after your app) that holds About, Preferences and Quit.
- macOS needs certain menu items, like Cut, Copy, Paste and Undo, for those standard actions to work. Eto.Forms adds them unless you turn this off.

## Commands

The template builds its menu and toolbar from `Command` objects. A command holds the text, shortcut and action in one place, so the same command can appear in both the menu and the toolbar, and enabling or disabling it updates both.

```csharp
var clickMe = new Command { MenuText = "Click Me!", ToolBarText = "Click Me!" };
clickMe.Executed += (sender, e) => MessageBox.Show(this, "I was clicked!");
```

There are three kinds:

- `Command`: runs an action when clicked.
- `CheckCommand`: toggles on or off.
- `RadioCommand`: picks one of a group of options.

You can also add `ButtonMenuItem`, `CheckMenuItem`, `RadioMenuItem` and `SeparatorMenuItem` directly to menus, and `ButtonToolItem`, `CheckToolItem`, `RadioToolItem` and `SeparatorToolItem` to toolbars, when you don't need to share them.

## The menu bar

This is the menu the template creates. In XAML and JSON it uses menu items with `Click` handlers in the code behind, rather than commands:

{{< code-tabs >}}

```csharp {title="MainForm.cs"}
Menu = new MenuBar
{
    Items =
    {
        new SubMenuItem { Text = "&File", Items = { clickMe } },
    },
    ApplicationItems =
    {
        new ButtonMenuItem { Text = "&Preferences..." },
    },
    QuitItem = quitCommand,
    AboutItem = aboutCommand
};
```

```xaml {title="MainForm.xeto"}
<Form.Menu>
  <MenuBar>
    <SubMenuItem Text="&amp;File">
      <ButtonMenuItem Text="Click Me!" Click="HandleClickMe" />
    </SubMenuItem>
    <MenuBar.ApplicationItems>
      <ButtonMenuItem Text="Preferences..." />
    </MenuBar.ApplicationItems>
    <MenuBar.QuitItem>
      <ButtonMenuItem Text="Quit" Shortcut="CommonModifier+Q" Click="HandleQuit" />
    </MenuBar.QuitItem>
    <MenuBar.AboutItem>
      <ButtonMenuItem Text="About..." Click="HandleAbout" />
    </MenuBar.AboutItem>
  </MenuBar>
</Form.Menu>
```

```json {title="MainForm.jeto"}
"Menu": {
  "Items": [
    {
      "$type": "SubMenuItem",
      "Text": "&File",
      "Items": [
        { "$type": "ButtonMenuItem", "Text": "Click Me!", "Click": "HandleClickMe" }
      ]
    }
  ],
  "ApplicationItems": [
    { "$type": "ButtonMenuItem", "Text": "Preferences..." }
  ],
  "QuitItem": { "$type": "ButtonMenuItem", "Text": "Quit", "Shortcut": "CommonModifier+Q", "Click": "HandleQuit" },
  "AboutItem": { "$type": "ButtonMenuItem", "Text": "About...", "Click": "HandleAbout" }
}
```

{{< /code-tabs >}}

- `Items` are the top-level menus. `&` marks the letter used for keyboard access on Windows and Linux.
- `ApplicationItems` go in the application menu on macOS, and in the File menu on other platforms.
- `QuitItem` and `AboutItem` are put in the right place for each platform. If you don't set `QuitItem`, macOS still gets one so users can quit your app.

## Add a View menu

Add a check command to keep the window on top, and radio commands to pick a size. Commands are always created in code, so with XAML or JSON put these in the code behind, after the form is loaded. Otherwise, put them before the `Menu` is created:

```csharp
var alwaysOnTop = new CheckCommand { MenuText = "Always on &Top", ToolBarText = "On Top" };
alwaysOnTop.CheckedChanged += (sender, e) => Topmost = alwaysOnTop.Checked;

var small = new RadioCommand { MenuText = "&Small" };
var large = new RadioCommand { MenuText = "&Large", Controller = small, Checked = true };
small.CheckedChanged += (sender, e) => { if (small.Checked) ClientSize = new Size(400, 300); };
large.CheckedChanged += (sender, e) => { if (large.Checked) ClientSize = new Size(800, 600); };
```

`Controller` groups radio commands together, so checking one unchecks the others.

Then add a View menu to the menu bar:

{{< code-tabs >}}

```csharp {title="MainForm.cs"}
// in the menu bar's Items
new SubMenuItem
{
    Text = "&View",
    Items = { alwaysOnTop, new SeparatorMenuItem(), small, large }
},
```

```csharp {tab="xaml" title="MainForm.xeto.cs"}
// after XamlReader.Load(this)
Menu.Items.Add(new SubMenuItem
{
    Text = "&View",
    Items = { alwaysOnTop, new SeparatorMenuItem(), small, large }
});
```

```csharp {tab="json" title="MainForm.jeto.cs"}
// after JsonReader.Load(this)
Menu.Items.Add(new SubMenuItem
{
    Text = "&View",
    Items = { alwaysOnTop, new SeparatorMenuItem(), small, large }
});
```

{{< /code-tabs >}}

## Shortcuts

Set `Shortcut` on a command to give it a keyboard shortcut. Use `Application.Instance.CommonModifier` for the key users expect on each platform, Ctrl on Windows and Linux and Cmd on macOS:

```csharp
clickMe.Shortcut = Application.Instance.CommonModifier | Keys.K;
```

## The toolbar

The toolbar takes the same commands:

{{< code-tabs >}}

```csharp {title="MainForm.cs"}
ToolBar = new ToolBar
{
    Items = { clickMe, new SeparatorToolItem(), alwaysOnTop }
};
```

```csharp {tab="xaml" title="MainForm.xeto.cs"}
// after XamlReader.Load(this)
ToolBar.Items.Add(new SeparatorToolItem());
ToolBar.Items.Add(alwaysOnTop);
```

```csharp {tab="json" title="MainForm.jeto.cs"}
// after JsonReader.Load(this)
ToolBar.Items.Add(new SeparatorToolItem());
ToolBar.Items.Add(alwaysOnTop);
```

{{< /code-tabs >}}

Set `Image` on a command to show an icon in the toolbar, e.g. `Image = Bitmap.FromResource("MyApp.Images.ClickMe.png")` for an embedded resource.

Next: [Tutorial 3 Layout](./Tutorial-3-Table-Layout)
