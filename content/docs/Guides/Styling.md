---
title: "Styling"
---

Styling allows you to have platform-specific or common code applied to controls.  The framework allows you to easily separate the non-platform specific Eto.Forms code from the platform-specific code, usually in different assemblies.

This also allows you to add custom platform-specific controls to your UI, but it is recommended to create Eto.Forms [Custom Controls](./Custom-Platform-Controls) to allow interaction with the platform-specific code.

The style is applied using the Style property for each control.  In the platform-specific assembly/launcher, you then add styles using the static Style.Add() methods.

To make your style apply to all created controls, pass a ``null`` as the style alias.

Note that spaces delimit multiple styles, so your style identifiers should not include a space.

## Example

This example shows how to change the highlight style of a list box on macOS.  Each time a list box is created with the style of "main-list", it will execute the platform-specific code.

In your Eto.Forms UI code:

{{< code-tabs >}}

```csharp
var mylist = new ListBox { Style = "main-list" };
```

```xaml
<ListBox Style="main-list" />
```

```json
{ "$type": "ListBox", "Style": "main-list" }
```

```fsharp
let mylist = new ListBox(Style = "main-list")
```

```python
mylist = forms.ListBox()
mylist.Style = "main-list"
```

{{< /code-tabs >}}

In your Mac platform code (usually called before the application starts):

{{< code-tabs >}}

```csharp
Eto.Style.Add<ListBoxHandler>("main-list", handler => {
    handler.Control.SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.SourceList;
});
```

```fsharp
Eto.Style.Add<ListBoxHandler>("main-list", fun (handler: ListBoxHandler) ->
    handler.Control.SelectionHighlightStyle <- NSTableViewSelectionHighlightStyle.SourceList)
```

{{< /code-tabs >}}

You can also apply styles for any control without any native code:

{{< code-tabs >}}

```csharp
Eto.Style.Add<TableLayout>("padded-table", table => {
    table.Padding = new Padding(10);
    table.Spacing = new Size(5, 5);
});

// will automatically get the padding & spacing applied
var myTable = new TableLayout { Style = "padded-table" };
```

```fsharp
Eto.Style.Add<TableLayout>("padded-table", fun (table: TableLayout) ->
    table.Padding <- Padding(10)
    table.Spacing <- Size(5, 5))

// will automatically get the padding & spacing applied
let myTable = new TableLayout(Style = "padded-table")
```

```python
import Eto

def padded_table(table):
    table.Padding = drawing.Padding(10)
    table.Spacing = drawing.Size(5, 5)

Eto.Style.Add[forms.TableLayout]("padded-table", padded_table)

# will automatically get the padding & spacing applied
myTable = forms.TableLayout()
myTable.Style = "padded-table"
```

{{< /code-tabs >}}

The Python sample needs Python 3 in Rhino, as IronPython can't tell the two `Style.Add` methods apart.
