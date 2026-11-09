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

{{< code-tabs >}}

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

```fsharp {title="MainViewModel.fs"}
type MainViewModel() =
    let clickMeCommand = new Command(MenuText = "Click Me!", ToolBarText = "Click Me!")
    let quitCommand = new Command(MenuText = "Quit", Shortcut = (Application.Instance.CommonModifier ||| Keys.Q))
    let aboutCommand = new Command(MenuText = "About...")

    do
        clickMeCommand.Executed.Add(fun _ -> MessageBox.Show("I was clicked!") |> ignore)
        quitCommand.Executed.Add(fun _ -> Application.Instance.Quit())
        aboutCommand.Executed.Add(fun _ -> (new AboutDialog()).ShowDialog(Application.Instance.MainForm) |> ignore)

    member _.ClickMeCommand = clickMeCommand
    member _.QuitCommand = quitCommand
    member _.AboutCommand = aboutCommand
```

```python {title="MainViewModel.py"}
import Eto.Forms as forms

class MainViewModel(object):
    def __init__(self):
        self.ClickMeCommand = forms.Command()
        self.ClickMeCommand.MenuText = "Click Me!"
        self.ClickMeCommand.ToolBarText = "Click Me!"
        self.ClickMeCommand.Executed += lambda sender, e: forms.MessageBox.Show("I was clicked!")

        self.QuitCommand = forms.Command()
        self.QuitCommand.MenuText = "Quit"
        self.QuitCommand.Shortcut = forms.Application.Instance.CommonModifier | forms.Keys.Q
        self.QuitCommand.Executed += lambda sender, e: forms.Application.Instance.Quit()

        self.AboutCommand = forms.Command()
        self.AboutCommand.MenuText = "About..."
        self.AboutCommand.Executed += lambda sender, e: forms.AboutDialog().ShowDialog(forms.Application.Instance.MainForm)
```

{{< /code-tabs >}}

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

```fsharp {title="MainForm.fs"}
let model = new MainViewModel()
this.DataContext <- model

let fileMenu = new SubMenuItem(Text = "&File")
fileMenu.Items.Add(model.ClickMeCommand) |> ignore

let menu = new MenuBar(QuitItem = model.QuitCommand.CreateMenuItem(), AboutItem = model.AboutCommand.CreateMenuItem())
menu.Items.Add(fileMenu)
menu.ApplicationItems.Add(new ButtonMenuItem(Text = "&Preferences..."))
this.Menu <- menu
```

```python {title="MainForm.py"}
model = MainViewModel()
self.DataContext = model

fileMenu = forms.SubMenuItem()
fileMenu.Text = "&File"
fileMenu.Items.Add(model.ClickMeCommand)

preferencesItem = forms.ButtonMenuItem()
preferencesItem.Text = "&Preferences..."

menu = forms.MenuBar()
menu.Items.Add(fileMenu)
menu.ApplicationItems.Add(preferencesItem)
menu.QuitItem = model.QuitCommand
menu.AboutItem = model.AboutCommand
self.Menu = menu
```

{{< /code-tabs >}}

- `Items` are the top-level menus. `&` marks the letter used for keyboard access on Windows and Linux.
- `ApplicationItems` go in the application menu on macOS, and in the File menu on other platforms.
- `QuitItem` and `AboutItem` are put in the right place for each platform. If you don't set `QuitItem`, macOS still gets one so users can quit your app.

In XAML and JSON, `{Binding ClickMeCommand}` finds the command in the form's `DataContext`. A bound command supplies what the item does and whether it's checked or enabled, so set the item's `Text` and `Shortcut` in the markup.

## Add a View menu

Add a check command to the view model that keeps the window on top:

{{< code-tabs >}}

```csharp {title="MainViewModel.cs"}
public CheckCommand AlwaysOnTopCommand { get; } = new CheckCommand { MenuText = "Always on &Top", ToolBarText = "On Top" };
```

```fsharp {title="MainViewModel.fs"}
let alwaysOnTopCommand = new CheckCommand(MenuText = "Always on &Top", ToolBarText = "On Top")
// ...
member _.AlwaysOnTopCommand = alwaysOnTopCommand
```

```python {title="MainViewModel.py"}
# in __init__
self.AlwaysOnTopCommand = forms.CheckCommand()
self.AlwaysOnTopCommand.MenuText = "Always on &Top"
self.AlwaysOnTopCommand.ToolBarText = "On Top"
```

{{< /code-tabs >}}

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

```fsharp {title="MainForm.fs"}
let model = new MainViewModel()
this.DataContext <- model

model.AlwaysOnTopCommand.CheckedChanged.Add(fun _ -> this.Topmost <- model.AlwaysOnTopCommand.Checked)
```

```python {title="MainForm.py"}
model = MainViewModel()
self.DataContext = model

def onTopChanged(sender, e):
    self.Topmost = model.AlwaysOnTopCommand.Checked
model.AlwaysOnTopCommand.CheckedChanged += onTopChanged
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

```fsharp {title="MainForm.fs"}
// after adding the File menu
let viewMenu = new SubMenuItem(Text = "&View")
viewMenu.Items.Add(model.AlwaysOnTopCommand) |> ignore
menu.Items.Add(viewMenu)
```

```python {title="MainForm.py"}
# after adding the File menu
viewMenu = forms.SubMenuItem()
viewMenu.Text = "&View"
viewMenu.Items.Add(model.AlwaysOnTopCommand)
menu.Items.Add(viewMenu)
```

{{< /code-tabs >}}

Radio items are grouped by creating them from `RadioCommand`s that share a `Controller`, so radio groups currently need to be built in code.

## Shortcuts

Set `Shortcut` on a command to give it a keyboard shortcut. Use `Application.Instance.CommonModifier` for the key users expect on each platform, Ctrl on Windows and Linux and Cmd on macOS:

{{< code-tabs >}}

```csharp {title="MainViewModel.cs"}
public Command ClickMeCommand { get; } = new Command
{
    MenuText = "Click Me!",
    ToolBarText = "Click Me!",
    Shortcut = Application.Instance.CommonModifier | Keys.K
};
```

```fsharp {title="MainViewModel.fs"}
let clickMeCommand =
    new Command(
        MenuText = "Click Me!",
        ToolBarText = "Click Me!",
        Shortcut = (Application.Instance.CommonModifier ||| Keys.K))
```

```python {title="MainViewModel.py"}
self.ClickMeCommand.Shortcut = forms.Application.Instance.CommonModifier | forms.Keys.K
```

{{< /code-tabs >}}

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

```fsharp {title="MainForm.fs"}
let toolBar = new ToolBar()
toolBar.Items.Add(model.ClickMeCommand)
toolBar.Items.Add(new SeparatorToolItem())
toolBar.Items.Add(model.AlwaysOnTopCommand)
this.ToolBar <- toolBar
```

```python {title="MainForm.py"}
toolBar = forms.ToolBar()
toolBar.Items.Add(model.ClickMeCommand)
toolBar.Items.Add(forms.SeparatorToolItem())
toolBar.Items.Add(model.AlwaysOnTopCommand)
self.ToolBar = toolBar
```

{{< /code-tabs >}}

Set `Image` on a command, or on a tool item in XAML and JSON, to show an icon in the toolbar.

Next: [Tutorial 3 Layout](./Tutorial-3-Table-Layout)
