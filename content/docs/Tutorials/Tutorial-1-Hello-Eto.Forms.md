---
title: "Tutorial 1 - Your First App"
---

This tutorial walks through the app the template creates, then adds a text box and a button to it.

## Create the app

Follow the [Quick Start](../QuickStart) to create a new app. You can write your UI in code or in a markup file, so pick the form type you prefer:

| Form type | `dotnet new` | UI written in |
| --- | --- | --- |
| **Code** | `dotnet new etoapp -o MyApp` | C#, in `MainForm.cs` |
| **Code Preview** | `dotnet new etoapp -m preview -o MyApp` | C#, in `MainForm.eto.cs`, which the [live preview](../DevExtensions/Live-Preview.md) can draw as you type |
| **Xaml** | `dotnet new etoapp -m xaml -o MyApp` | XAML, in `MainForm.xeto`, with code behind in `MainForm.xeto.cs` |
| **Json** | `dotnet new etoapp -m json -o MyApp` | JSON, in `MainForm.jeto`, with code behind in `MainForm.jeto.cs` |

The samples in these tutorials have a tab for each. Pick one and every sample on the site will show it. For **Code Preview**, use the C# tab and put the UI in `MainForm.eto.cs`.

Run the app with `dotnet run` (or F5 in your IDE) and you'll see a window with a menu, a toolbar and some text.

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

**MainForm** is your main window. Its content is the control shown inside the window, here a `StackLayout` that lists controls from top to bottom:

{{< code-tabs >}}

```csharp {title="MainForm.cs"}
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

```xaml {title="MainForm.xeto"}
<StackLayout>
  <Label>Some Content</Label>
</StackLayout>
```

```json {title="MainForm.jeto"}
"Content": {
  "$type": "StackLayout",
  "Items": [
    "Some Content"
  ]
}
```

{{< /code-tabs >}}

In C# and JSON, a string in a layout is shorthand for a `Label`. The rest of the form sets up the menu and toolbar, which [Tutorial 2](./Tutorial-2-Menus-and-Toolbars) covers.

## Add some controls

Replace the content with a text box and a button that greets whoever's name is typed in:

{{< code-tabs >}}

```csharp {title="MainForm.cs"}
public partial class MainForm : Form
{
    TextBox nameBox;

    public MainForm()
    {
        // ... title, menu and toolbar from the template

        nameBox = new TextBox { PlaceholderText = "Your name" };

        var helloButton = new Button { Text = "Say Hello" };
        helloButton.Click += HandleSayHello;

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
    }

    void HandleSayHello(object sender, EventArgs e)
    {
        MessageBox.Show(this, $"Hello, {nameBox.Text}!");
    }
}
```

```xaml {title="MainForm.xeto"}
<StackLayout Spacing="5">
  <Label>What's your name?</Label>
  <TextBox x:Name="nameBox" PlaceholderText="Your name" />
  <Button Text="Say Hello" Click="HandleSayHello" />
</StackLayout>
```

```csharp {tab="xaml" title="MainForm.xeto.cs"}
public class MainForm : Form
{
    TextBox nameBox; // set from x:Name in MainForm.xeto

    public MainForm()
    {
        XamlReader.Load(this);
    }

    protected void HandleSayHello(object sender, EventArgs e)
    {
        MessageBox.Show(this, $"Hello, {nameBox.Text}!");
    }

    // ... menu handlers from the template
}
```

```json {title="MainForm.jeto"}
"Content": {
  "$type": "StackLayout",
  "Spacing": "5",
  "Items": [
    "What's your name?",
    { "$type": "TextBox", "$name": "nameBox", "PlaceholderText": "Your name" },
    { "$type": "Button", "Text": "Say Hello", "Click": "HandleSayHello" }
  ]
}
```

```csharp {tab="json" title="MainForm.jeto.cs"}
public class MainForm : Form
{
    TextBox nameBox; // set from $name in MainForm.jeto

    public MainForm()
    {
        JsonReader.Load(this);
    }

    protected void HandleSayHello(object sender, EventArgs e)
    {
        MessageBox.Show(this, $"Hello, {nameBox.Text}!");
    }

    // ... menu handlers from the template
}
```

{{< /code-tabs >}}

In XAML and JSON, naming a control (`x:Name` or `$name`) fills the field of the same name in the code behind, and events like `Click` name the method that handles them.

Run the app again and try it out. Each control uses the native one for the platform, so it looks at home on Windows, macOS and Linux.

Next: [Tutorial 2 Menus and Toolbars](./Tutorial-2-Menus-and-Toolbars)
