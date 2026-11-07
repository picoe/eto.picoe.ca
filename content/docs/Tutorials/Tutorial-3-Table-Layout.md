---
title: "Tutorial 3 - Layout"
---

This tutorial builds a small sign-up form, and shows how to lay it out so it fits every platform.

## Laying out a form

Controls are different sizes on each platform, so Eto.Forms layouts size them for you rather than placing them at fixed positions. Rows and columns fit their contents, and you choose which ones stretch to fill any extra space.

The `TableLayout` lines controls up in rows and columns, like an HTML table. Replace the content from [Tutorial 1](./Tutorial-1-Hello-Eto.Forms) with:

{{< code-tabs >}}

```csharp {title="MainForm.cs"}
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

```xaml {title="MainForm.xeto"}
<TableLayout Spacing="5,5">
  <TableRow>
    <Label>Name</Label>
    <TableCell ScaleWidth="True">
      <TextBox />
    </TableCell>
  </TableRow>
  <TableRow>
    <Label>Email</Label>
    <TextBox />
  </TableRow>
  <TableRow>
    <TableCell />
    <CheckBox Text="Send me the newsletter" />
  </TableRow>
  <TableRow ScaleHeight="True" />
  <TableRow>
    <TableCell />
    <TableLayout>
      <TableRow>
        <Button Text="Sign Up" />
        <TableCell />
      </TableRow>
    </TableLayout>
  </TableRow>
</TableLayout>
```

```json {title="MainForm.jeto"}
"Content": {
  "$type": "TableLayout",
  "Spacing": "5,5",
  "Rows": [
    [ "Name", { "ScaleWidth": true, "Control": { "$type": "TextBox" } } ],
    [ "Email", { "$type": "TextBox" } ],
    [ null, { "$type": "CheckBox", "Text": "Send me the newsletter" } ],
    null,
    [ null, [ [ { "$type": "Button", "Text": "Sign Up" }, null ] ] ]
  ]
}
```

{{< /code-tabs >}}

In JSON, each row is a list of cells, and a list inside a cell is a nested table.

A few things to note:

- Setting `ScaleWidth` on the text box's cell makes the second column stretch with the window. The labels column stays as wide as its widest label.
- The empty row (`null`, or `ScaleHeight` in XAML) stretches to take up the remaining height. Without it, the last row would stretch instead.
- The button keeps its natural size rather than filling the column. In C#, `TableLayout.AutoSized` does this; in XAML and JSON, a nested table with an empty cell after the button does the same.

See [TableLayout](../Controls/TableLayout) for more, and [DynamicLayout](../Controls/DynamicLayout) and [StackLayout](../Controls/StackLayout) for other ways to arrange controls.

Next: [Tutorial 4 Data Binding](./Tutorial-4-Data-Binding)
