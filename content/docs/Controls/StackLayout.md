---
title: "StackLayout"
---

The `StackLayout` places controls one after another, either top to bottom or left to right. It's the simplest layout, and handy for lists of controls or rows of buttons.

{{< code-tabs >}}

```csharp
Content = new StackLayout
{
    Padding = 10, // space around the edge
    Spacing = 5,  // space between each item
    Items =
    {
        "Enter your name:", // strings become labels
        new TextBox(),
        new Button { Text = "OK" }
    }
};
```

```xaml
<StackLayout Padding="10" Spacing="5">
  <Label>Enter your name:</Label>
  <TextBox />
  <Button Text="OK" />
</StackLayout>
```

```json
"Content": {
  "$type": "StackLayout",
  "Padding": "10",
  "Spacing": "5",
  "Items": [
    "Enter your name:",
    { "$type": "TextBox" },
    { "$type": "Button", "Text": "OK" }
  ]
}
```

```fsharp
let layout = new StackLayout(Padding = 10, Spacing = 5)
layout.Items.Add("Enter your name:") // strings become labels
layout.Items.Add(StackLayoutItem(new TextBox()))
layout.Items.Add(StackLayoutItem(new Button(Text = "OK")))
this.Content <- layout
```

```python
label = forms.Label()
label.Text = "Enter your name:"
button = forms.Button()
button.Text = "OK"

layout = forms.StackLayout(
    forms.StackLayoutItem(label),
    forms.StackLayoutItem(forms.TextBox()),
    forms.StackLayoutItem(button))
layout.Padding = drawing.Padding(10)  # space around the edge
layout.Spacing = 5                    # space between each item
self.Content = layout
```

{{< /code-tabs >}}

## Orientation and alignment

Set `Orientation` to `Orientation.Horizontal` to place items left to right.

By default each item is placed at the start (top or left) and keeps its natural size. To change that, use `HorizontalContentAlignment` for a vertical stack, or `VerticalContentAlignment` for a horizontal one. `Stretch` makes every item fill the width (or height) of the layout:

{{< code-tabs >}}

```csharp
new StackLayout
{
    HorizontalContentAlignment = HorizontalAlignment.Stretch,
    Items = { new TextBox(), new TextArea() }
};
```

```xaml
<StackLayout HorizontalContentAlignment="Stretch">
  <TextBox />
  <TextArea />
</StackLayout>
```

```json
{
  "$type": "StackLayout",
  "HorizontalContentAlignment": "Stretch",
  "Items": [
    { "$type": "TextBox" },
    { "$type": "TextArea" }
  ]
}
```

```fsharp
let layout = new StackLayout(HorizontalContentAlignment = HorizontalAlignment.Stretch)
layout.Items.Add(StackLayoutItem(new TextBox()))
layout.Items.Add(StackLayoutItem(new TextArea()))
```

```python
layout = forms.StackLayout(
    forms.StackLayoutItem(forms.TextBox()),
    forms.StackLayoutItem(forms.TextArea()))
layout.HorizontalContentAlignment = forms.HorizontalAlignment.Stretch
```

{{< /code-tabs >}}

## Expanding an item

Wrap a control in a `StackLayoutItem` to set its alignment, or to make it take up the remaining space:

{{< code-tabs >}}

```csharp
new StackLayout
{
    Orientation = Orientation.Horizontal,
    Spacing = 5,
    Items =
    {
        new StackLayoutItem(new TextBox(), expand: true),
        new Button { Text = "Search" }
    }
};
```

```xaml
<StackLayout Orientation="Horizontal" Spacing="5">
  <StackLayoutItem Expand="True">
    <TextBox />
  </StackLayoutItem>
  <Button Text="Search" />
</StackLayout>
```

```json
{
  "$type": "StackLayout",
  "Orientation": "Horizontal",
  "Spacing": "5",
  "Items": [
    { "Expand": true, "Control": { "$type": "TextBox" } },
    { "$type": "Button", "Text": "Search" }
  ]
}
```

```fsharp
let layout = new StackLayout(Orientation = Orientation.Horizontal, Spacing = 5)
layout.Items.Add(StackLayoutItem(new TextBox(), true))
layout.Items.Add(StackLayoutItem(new Button(Text = "Search")))
```

```python
button = forms.Button()
button.Text = "Search"

layout = forms.StackLayout(
    forms.StackLayoutItem(forms.TextBox(), True),
    forms.StackLayoutItem(button))
layout.Orientation = forms.Orientation.Horizontal
layout.Spacing = 5
```

{{< /code-tabs >}}

A `null` item that expands is an easy way to push the rest to the end, e.g. for buttons aligned to the right:

{{< code-tabs >}}

```csharp
Items = { new StackLayoutItem(null, expand: true), cancelButton, okButton }
```

```xaml
<StackLayout Orientation="Horizontal" Spacing="5">
  <StackLayoutItem Expand="True" />
  <Button Text="Cancel" />
  <Button Text="OK" />
</StackLayout>
```

```json
"Items": [
  { "Expand": true },
  { "$type": "Button", "Text": "Cancel" },
  { "$type": "Button", "Text": "OK" }
]
```

```fsharp
layout.Items.Add(StackLayoutItem(null, true))
layout.Items.Add(StackLayoutItem(cancelButton))
layout.Items.Add(StackLayoutItem(okButton))
```

```python
layout.Items.Add(forms.StackLayoutItem(None, True))
layout.Items.Add(forms.StackLayoutItem(cancelButton))
layout.Items.Add(forms.StackLayoutItem(okButton))
```

{{< /code-tabs >}}

For forms with labels and fields that need to line up across rows, use a [TableLayout](./TableLayout) or [DynamicLayout](./DynamicLayout) instead.
