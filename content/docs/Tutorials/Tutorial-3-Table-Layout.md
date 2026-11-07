---
title: "Tutorial 3 - Layout"
---

This tutorial builds a small sign-up form, and shows how to lay it out so it fits every platform.

## Laying out a form

Controls are different sizes on each platform, so Eto.Forms layouts size them for you rather than placing them at fixed positions. Rows and columns fit their contents, and you choose which ones stretch to fill any extra space.

The `TableLayout` lines controls up in rows and columns, like an HTML table. Replace the `Content` from [Tutorial 1](./Tutorial-1-Hello-Eto.Forms) with:

```csharp
var nameBox = new TextBox();
var emailBox = new TextBox();
var newsletterBox = new CheckBox { Text = "Send me the newsletter" };
var signUpButton = new Button { Text = "Sign Up" };

Content = new TableLayout
{
    Padding = 10,             // space around the edge
    Spacing = new Size(5, 5), // space between cells
    Rows =
    {
        new TableRow("Name", new TableCell(nameBox, true)),
        new TableRow("Email", emailBox),
        new TableRow(null, newsletterBox),
        null, // an empty row that takes up the remaining height
        new TableRow(null, TableLayout.AutoSized(signUpButton))
    }
};
```

A few things to note:

- `new TableCell(nameBox, true)` makes the second column stretch with the window. The labels column stays as wide as its widest label.
- A `null` row or cell is empty space that stretches. Without the `null` row, the last row would stretch instead.
- `TableLayout.AutoSized` keeps the button at its natural size rather than filling the column.

See [TableLayout](../Controls/TableLayout) for more, and [DynamicLayout](../Controls/DynamicLayout) and [StackLayout](../Controls/StackLayout) for other ways to arrange controls.

Next: [Tutorial 4 Data Binding](./Tutorial-4-Data-Binding)
