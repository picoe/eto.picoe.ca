---
title: "StackLayout"
---

The `StackLayout` places controls one after another, either top to bottom or left to right. It's the simplest layout, and handy for lists of controls or rows of buttons.

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

## Orientation and alignment

Set `Orientation` to `Orientation.Horizontal` to place items left to right.

By default each item is placed at the start (top or left) and keeps its natural size. To change that, use `HorizontalContentAlignment` for a vertical stack, or `VerticalContentAlignment` for a horizontal one. `Stretch` makes every item fill the width (or height) of the layout:

```csharp
new StackLayout
{
    HorizontalContentAlignment = HorizontalAlignment.Stretch,
    Items = { new TextBox(), new TextArea() }
};
```

## Expanding an item

Wrap a control in a `StackLayoutItem` to set its alignment, or to make it take up the remaining space:

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

A `null` item that expands is an easy way to push the rest to the end, e.g. for buttons aligned to the right:

```csharp
Items = { new StackLayoutItem(null, expand: true), cancelButton, okButton }
```

For forms with labels and fields that need to line up across rows, use a [TableLayout](./TableLayout) or [DynamicLayout](./DynamicLayout) instead.
