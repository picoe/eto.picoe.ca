---
title: "TableLayout"
---

The ```TableLayout``` is useful to align controls in a grid-like fashion and works similarly to an HTML table.  Rows and columns can be *scaled* to equally share the flexible space of the container. When a row or column is not scaled, it will only take up space necessary for the contained controls in that row and/or column.

To define space around the border of the table, use the ```Padding``` property to specify the top/left/bottom/right padding.  The ```Spacing``` property is used to specify the vertical/horizontal space between each cell.

## Declarative Approach

The ```TableLayout``` allows you to declaratively define a table layout, either via one of the constructors that take a set of ```TableRow``` objects, or via the ```Rows``` property.
Each ```TableRow``` can take an array of ```TableCell``` objects via its constructor or ```Cells``` property.  Each ```TableCell``` can only contain a single control.

Instead of having to create TableRow and TableCell objects directly, you can specify any ```Control``` class which will convert implicitly.

For example, the following three variations are identical:

{{< code-tabs >}}

```csharp
var layout = new TableLayout(new Label(), new TextBox());

var layout = new TableLayout { Rows = { new Label(), new TextBox() } };

var layout = new TableLayout();
layout.Rows.Add(new TableRow(new TableCell(new Label())));
layout.Rows.Add(new TableRow(new TableCell(new TextBox())));
```

```xaml
<TableLayout>
  <TableRow>
    <Label />
  </TableRow>
  <TableRow>
    <TextBox />
  </TableRow>
</TableLayout>
```

```json
{
  "$type": "TableLayout",
  "Rows": [
    [ { "$type": "Label" } ],
    [ { "$type": "TextBox" } ]
  ]
}
```

```fsharp
let layout = new TableLayout(TableRow(TableCell(new Label())), TableRow(TableCell(new TextBox())))

let layout = new TableLayout()
layout.Rows.Add(TableRow(TableCell(new Label())))
layout.Rows.Add(TableRow(TableCell(new TextBox())))
```

```python
layout = forms.TableLayout(
    forms.TableRow(forms.TableCell(forms.Label())),
    forms.TableRow(forms.TableCell(forms.TextBox())))

layout = forms.TableLayout()
layout.Rows.Add(forms.TableRow(forms.TableCell(forms.Label())))
layout.Rows.Add(forms.TableRow(forms.TableCell(forms.TextBox())))
```

{{< /code-tabs >}}

In XAML and JSON, a control in a row is put in a cell for you. F# and Python need the `TableRow` and `TableCell` written out.

## Scaling

Each row or column in a ```TableLayout``` can be scaled so that the controls expand to fit the height or width of the remaining space for the table.  If none are specified, the **last** row or column will automatically be scaled, as all controls must fill the space of its container.

To specify that a *row* should scale (expand/contract with the height of the form) other than the last row, set `TableRow.ScaleHeight` to `true`:

{{< code-tabs >}}

```csharp
new TableRow { ScaleHeight = true, Cells = { myCell, myControl, etc } }
```

```xaml
<TableRow ScaleHeight="True">
  <Label>My label</Label>
  <TextBox />
</TableRow>
```

```json
{ "ScaleHeight": true, "Cells": [ "My label", { "$type": "TextBox" } ] }
```

```fsharp
TableRow(myCell, TableCell(myControl), ScaleHeight = true)
```

```python
row = forms.TableRow(myCell, forms.TableCell(myControl))
row.ScaleHeight = True
```

{{< /code-tabs >}}

To specify that a *column* should scale (expand/contract with the width of the form) other than the last column, set `TableCell.ScaleWidth` to `true`:

{{< code-tabs >}}

```csharp
new TableCell { ScaleWidth = true, Control = myControl }
// or 
new TableCell(myControl, true)
```

```xaml
<TableCell ScaleWidth="True">
  <TextBox />
</TableCell>
```

```json
{ "ScaleWidth": true, "Control": { "$type": "TextBox" } }
```

```fsharp
TableCell(myControl, true)
```

```python
forms.TableCell(myControl, True)
```

{{< /code-tabs >}}

To easily define an empty space that scales, you can specify `null` for either a `TableRow` or `TableCell` when defining your table:

{{< code-tabs >}}

```csharp
new TableLayout { Rows = { topRow, null, bottomRow } } 
// or
new TableRow { Cells = { controlOnLeft, null, controlOnRight } }
```

```xaml
<TableLayout>
  <TableRow>
    <Label>Top</Label>
  </TableRow>
  <TableRow ScaleHeight="True" />
  <TableRow>
    <Label>Left</Label>
    <TableCell ScaleWidth="True" />
    <Label>Right</Label>
  </TableRow>
</TableLayout>
```

```json
{
  "$type": "TableLayout",
  "Rows": [
    [ "Top" ],
    null,
    [ "Left", null, "Right" ]
  ]
}
```

```fsharp
new TableLayout(topRow, null, bottomRow)
// or
TableRow(TableCell(controlOnLeft), null, TableCell(controlOnRight))
```

```python
forms.TableLayout(topRow, None, bottomRow)
# or
forms.TableRow(forms.TableCell(controlOnLeft), None, forms.TableCell(controlOnRight))
```

{{< /code-tabs >}}

In XAML, write the empty space as an empty row or cell set to scale.

## Helpers

There are a few helper methods that provide a way to organize your layout:

### ```TableLayout.AutoSized()```

This method creates a new table that auto-sizes your control in its container.  This creates extra space to the right and bottom to fill the container.  You can also center the control by setting the centered parameter to true.

Example:

{{< code-tabs >}}

```csharp
var layout = new TableLayout(
    new TableRow(
        new Label { Text = "Auto-Sized drop down" },
        TableLayout.AutoSized(new DropDown { Items = { "Item 1", "Item 2" } })
    )
);
```

```xaml
<TableLayout>
  <TableRow>
    <Label>Auto-Sized drop down</Label>
    <TableLayout>
      <TableRow>
        <DropDown>
          <ListItem Text="Item 1" />
          <ListItem Text="Item 2" />
        </DropDown>
        <TableCell />
      </TableRow>
      <TableRow />
    </TableLayout>
  </TableRow>
</TableLayout>
```

```json
{
  "$type": "TableLayout",
  "Rows": [
    [ "Auto-Sized drop down", [ [ { "$type": "DropDown", "Items": [ "Item 1", "Item 2" ] }, null ], null ] ]
  ]
}
```

```fsharp
let dropDown = new DropDown()
dropDown.Items.Add("Item 1")
dropDown.Items.Add("Item 2")

let layout =
    new TableLayout(
        TableRow(
            TableCell(new Label(Text = "Auto-Sized drop down")),
            TableCell(TableLayout.AutoSized(dropDown))))
```

```python
label = forms.Label()
label.Text = "Auto-Sized drop down"
dropDown = forms.DropDown()
dropDown.Items.Add("Item 1")
dropDown.Items.Add("Item 2")

layout = forms.TableLayout(
    forms.TableRow(
        forms.TableCell(label),
        forms.TableCell(forms.TableLayout.AutoSized(dropDown))))
```

{{< /code-tabs >}}

XAML and JSON have no `AutoSized`, so they use a nested table with an empty cell to the right and an empty row below.

### ```TableLayout.Horizontal()```

Creates a new table with the specified cells in a new row.  This is equivalent to:

{{< code-tabs >}}

```csharp
new TableLayout(new TableRow(cells));
```

```fsharp
new TableLayout(TableRow(cells))
```

```python
forms.TableLayout(forms.TableRow(cells))
```

{{< /code-tabs >}}

## Example 1

This example shows how you can create a table with 2 columns and 3 rows

{{< code-tabs >}}

```csharp
// This creates a layout like this:
//
//  (auto sized)  (expands to container's width)
//  ----------------------------------------------
// | First Row  | [Text Box                     ] |
// |------------|---------------------------------|
// | Second Row | [                             ] |
// |            | [List Box                     ] |
// |            | [                             ] |
// |------------|---------------------------------|
// | Third Row  | [Drop Down]                     |
// |------------|---------------------------------|
// | <empty>    | <empty>                         | expands to
// |            |                                 | container's
// |            |                                 | height
//  ----------------------------------------------

var layout = new TableLayout
{
    Padding = new Padding(10), // padding around cells
    Spacing = new Size(5, 5), // spacing between each cell
    Rows =
    {
        new TableRow(new Label {Text = "First Row"}, new TextBox()),
        new TableRow(new Label {Text = "Second Row"}, new ListBox()),
        new TableRow(
            new Label {Text = "Third Row"},
            TableLayout.AutoSized(new DropDown {Items = {"Item 1", "Item 2"}})
        ),
        null
    }
};
```

```xaml
<TableLayout Padding="10" Spacing="5,5">
  <TableRow>
    <Label>First Row</Label>
    <TextBox />
  </TableRow>
  <TableRow>
    <Label>Second Row</Label>
    <ListBox />
  </TableRow>
  <TableRow>
    <Label>Third Row</Label>
    <TableLayout>
      <TableRow>
        <DropDown>
          <ListItem Text="Item 1" />
          <ListItem Text="Item 2" />
        </DropDown>
        <TableCell />
      </TableRow>
      <TableRow />
    </TableLayout>
  </TableRow>
  <TableRow />
</TableLayout>
```

```json
{
  "$type": "TableLayout",
  "Padding": "10",
  "Spacing": "5,5",
  "Rows": [
    [ "First Row", { "$type": "TextBox" } ],
    [ "Second Row", { "$type": "ListBox" } ],
    [ "Third Row", [ [ { "$type": "DropDown", "Items": [ "Item 1", "Item 2" ] }, null ], null ] ],
    null
  ]
}
```

```fsharp
let dropDown = new DropDown()
dropDown.Items.Add("Item 1")
dropDown.Items.Add("Item 2")

let layout =
    new TableLayout(
        Padding = Padding(10), // padding around cells
        Spacing = Size(5, 5)   // spacing between each cell
    )
layout.Rows.Add(TableRow(TableCell(new Label(Text = "First Row")), TableCell(new TextBox())))
layout.Rows.Add(TableRow(TableCell(new Label(Text = "Second Row")), TableCell(new ListBox())))
layout.Rows.Add(TableRow(TableCell(new Label(Text = "Third Row")), TableCell(TableLayout.AutoSized(dropDown))))
layout.Rows.Add(null)
```

```python
def label(text):
    l = forms.Label()
    l.Text = text
    return l

dropDown = forms.DropDown()
dropDown.Items.Add("Item 1")
dropDown.Items.Add("Item 2")

layout = forms.TableLayout()
layout.Padding = drawing.Padding(10)  # padding around cells
layout.Spacing = drawing.Size(5, 5)   # spacing between each cell
layout.Rows.Add(forms.TableRow(forms.TableCell(label("First Row")), forms.TableCell(forms.TextBox())))
layout.Rows.Add(forms.TableRow(forms.TableCell(label("Second Row")), forms.TableCell(forms.ListBox())))
layout.Rows.Add(forms.TableRow(forms.TableCell(label("Third Row")), forms.TableCell(forms.TableLayout.AutoSized(dropDown))))
layout.Rows.Add(None)
```

{{< /code-tabs >}}

Notes:

1. If you do not include the ```null``` as the end, the third row with the ```ListBox``` will expand instead of having blank space at the bottom.
2. This takes advantage of the fact that the last column is automatically scaled. If you want the first column to expand, then you would have to do that explicitly.

## Example 2

This example shows how to scale different rows/columns

{{< code-tabs >}}

```csharp
// This creates a layout like this:
//
//  (auto sized)  (expands to container's width)  (auto sized)
//  ---------------------------------------------------------
// | First Row  | [Text Box                     ] | [Button] |
// |------------|---------------------------------|----------|
// | Second Row | [                             ] |          | expands to
// |            | [List Box                     ] |          | container's
// |            | [                             ] |          | height
// |------------|---------------------------------|----------|
// | Third Row  | [Drop Down]                     |          |
//  ---------------------------------------------------------

var layout = new TableLayout
{
    Padding = new Padding(10), // padding around cells
    Spacing = new Size(5, 5), // spacing between each cell
    Rows =
    {
        new TableRow(
            new Label { Text = "First Row" }, 
            new TableCell(new TextBox(), true), 
            new Button { Text = "Button" }
        ),
        new TableRow {
            ScaleHeight = true,
            Cells = {
                new Label { Text = "Second Row" }, 
                new ListBox()
            }
        },
        new TableRow(
            new Label { Text = "Third Row" }, 
            TableLayout.AutoSized(new DropDown())
        ),
    }
};
```

```xaml
<TableLayout Padding="10" Spacing="5,5">
  <TableRow>
    <Label>First Row</Label>
    <TableCell ScaleWidth="True">
      <TextBox />
    </TableCell>
    <Button Text="Button" />
  </TableRow>
  <TableRow ScaleHeight="True">
    <Label>Second Row</Label>
    <ListBox />
  </TableRow>
  <TableRow>
    <Label>Third Row</Label>
    <TableLayout>
      <TableRow>
        <DropDown />
        <TableCell />
      </TableRow>
      <TableRow />
    </TableLayout>
  </TableRow>
</TableLayout>
```

```json
{
  "$type": "TableLayout",
  "Padding": "10",
  "Spacing": "5,5",
  "Rows": [
    [ "First Row", { "ScaleWidth": true, "Control": { "$type": "TextBox" } }, { "$type": "Button", "Text": "Button" } ],
    { "ScaleHeight": true, "Cells": [ "Second Row", { "$type": "ListBox" } ] },
    [ "Third Row", [ [ { "$type": "DropDown" }, null ], null ] ]
  ]
}
```

```fsharp
let layout =
    new TableLayout(
        Padding = Padding(10), // padding around cells
        Spacing = Size(5, 5)   // spacing between each cell
    )
layout.Rows.Add(
    TableRow(
        TableCell(new Label(Text = "First Row")),
        TableCell(new TextBox(), true),
        TableCell(new Button(Text = "Button"))))
layout.Rows.Add(
    TableRow(
        TableCell(new Label(Text = "Second Row")),
        TableCell(new ListBox()),
        ScaleHeight = true))
layout.Rows.Add(
    TableRow(
        TableCell(new Label(Text = "Third Row")),
        TableCell(TableLayout.AutoSized(new DropDown()))))
```

```python
# label() as in Example 1
button = forms.Button()
button.Text = "Button"

secondRow = forms.TableRow(forms.TableCell(label("Second Row")), forms.TableCell(forms.ListBox()))
secondRow.ScaleHeight = True

layout = forms.TableLayout()
layout.Padding = drawing.Padding(10)  # padding around cells
layout.Spacing = drawing.Size(5, 5)   # spacing between each cell
layout.Rows.Add(forms.TableRow(
    forms.TableCell(label("First Row")),
    forms.TableCell(forms.TextBox(), True),
    forms.TableCell(button)))
layout.Rows.Add(secondRow)
layout.Rows.Add(forms.TableRow(
    forms.TableCell(label("Third Row")),
    forms.TableCell(forms.TableLayout.AutoSized(forms.DropDown()))))
```

{{< /code-tabs >}}

Here's a breakdown of what the above is doing:

1. `new TableCell(new TextBox(), true)` makes the middle column expand/contract to the available width of the form, by passing `true` to the constructor (which sets `TableCell.ScaleWidth` to `true`).
2. `ScaleHeight = true` makes the 2nd row expand/contract to the available height of the form.
3. `TableLayout.AutoSized(new DropDown())` makes it so the drop down sizes itself to its contents and will not expand to the width of the column it is contained in.
