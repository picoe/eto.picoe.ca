---
title: "Eto.Forms"
---

{{< code-tabs >}}

```csharp {title="MainForm.cs"}
public class MainForm : Form
{
    public MainForm()
    {
        Title = "Hello Eto";

        var nameBox = new TextBox { PlaceholderText = "Your name" };
        var helloButton = new Button { Text = "Say Hello" };
        helloButton.Click += (sender, e) =>
            MessageBox.Show(this, $"Hello, {nameBox.Text}!");

        Content = new StackLayout
        {
            Padding = 10,
            Spacing = 5,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Items = { "What's your name?", nameBox, helloButton }
        };
    }
}
```

```xaml {title="MainForm.xeto"}
<Form xmlns="http://schema.picoe.ca/eto.forms"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      Title="Hello Eto" Padding="10">
  <StackLayout Spacing="5" HorizontalContentAlignment="Stretch">
    <Label>What's your name?</Label>
    <TextBox x:Name="nameBox" PlaceholderText="Your name" />
    <Button Text="Say Hello" Click="HandleSayHello" />
  </StackLayout>
</Form>
```

```csharp {tab="xaml" title="MainForm.xeto.cs"}
public class MainForm : Form
{
    TextBox nameBox;

    public MainForm() => XamlReader.Load(this);

    protected void HandleSayHello(object sender, EventArgs e) =>
        MessageBox.Show(this, $"Hello, {nameBox.Text}!");
}
```

```json {title="MainForm.jeto"}
{
  "$type": "Form",
  "Title": "Hello Eto",
  "Padding": "10",
  "Content": {
    "$type": "StackLayout",
    "Spacing": "5",
    "HorizontalContentAlignment": "Stretch",
    "Items": [
      "What's your name?",
      { "$type": "TextBox", "$name": "nameBox", "PlaceholderText": "Your name" },
      { "$type": "Button", "Text": "Say Hello", "Click": "HandleSayHello" }
    ]
  }
}
```

```csharp {tab="json" title="MainForm.jeto.cs"}
public class MainForm : Form
{
    TextBox nameBox;

    public MainForm() => JsonReader.Load(this);

    protected void HandleSayHello(object sender, EventArgs e) =>
        MessageBox.Show(this, $"Hello, {nameBox.Text}!");
}
```

{{< /code-tabs >}}
