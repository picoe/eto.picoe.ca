---
title: "GridView"
---

A GridView is bound by assigning its `DataStore` property to a collection.  The control supports MVVM in such that if the collection implements `INotifyCollectionChanged`, such as the `ObservableCollection<T>`, it will update as changes are made to the underlying collection.

There are a number of available Cell types:

* CheckBoxCell
* ComboBoxCell
* ImageTextCell
* ImageViewCell
* TextBoxCell

When creating cells, you must set its binding which can be done via index, property via reflection, or delegate/lambda methods.

[API Documentation](/api/type/eto.forms.gridview/)

## POCO

The best way to use the GridView is to use a [POCO](http://en.wikipedia.org/wiki/Plain_Old_CLR_Object) or [DTO](http://en.wikipedia.org/wiki/Data_transfer_object).  Cells can reference properties of your objects to populate its data.

As an example:

{{< code-tabs >}}

```csharp
using Eto.Forms;
using Eto;
using System.Collections.ObjectModel;

class MyPoco
{
    public string Text { get; set; }

    public bool Check { get; set; }
}

class MyForm : Form
{
    public MyForm()
    {
        var collection = new ObservableCollection<MyPoco>();
        collection.Add(new MyPoco { Text = "Row 1", Check = true });
        collection.Add(new MyPoco { Text = "Row 2", Check = false });

        var grid = new GridView { DataStore = collection };

        grid.Columns.Add(new GridColumn {
            DataCell = new TextBoxCell { Binding = Binding.Property<MyPoco, string>(r => r.Text) },
            HeaderText = "Text"
        });

        grid.Columns.Add(new GridColumn {
            DataCell = new CheckBoxCell { Binding = Binding.Property<MyPoco, bool?>(r => r.Check) },
            HeaderText = "Check"
        });
        Content = grid;
    }
}
```

```xaml {title="MyForm.xeto"}
<GridView x:Name="grid">
  <GridColumn HeaderText="Text">
    <TextBoxCell Binding="{Binding Text}" />
  </GridColumn>
  <GridColumn HeaderText="Check">
    <CheckBoxCell Binding="{Binding Check}" />
  </GridColumn>
</GridView>
```

```csharp {tab="xaml" title="MyForm.xeto.cs"}
class MyForm : Form
{
    GridView grid; // set from x:Name in MyForm.xeto

    public MyForm()
    {
        XamlReader.Load(this);

        grid.DataStore = new ObservableCollection<MyPoco>
        {
            new MyPoco { Text = "Row 1", Check = true },
            new MyPoco { Text = "Row 2", Check = false }
        };
    }
}
```

```fsharp
open System
open System.Collections.ObjectModel
open Eto.Forms

type MyPoco() =
    member val Text = "" with get, set
    member val Check = false with get, set

type MyForm() as this =
    inherit Form()
    do
        let collection = ObservableCollection<MyPoco>()
        collection.Add(MyPoco(Text = "Row 1", Check = true))
        collection.Add(MyPoco(Text = "Row 2", Check = false))

        // GridView takes a seq<obj>, which F# needs to be cast to
        let grid = new GridView(DataStore = (box collection :?> seq<obj>))

        grid.Columns.Add(
            new GridColumn(
                DataCell = new TextBoxCell(Binding = Binding.Property(fun (r: MyPoco) -> r.Text)),
                HeaderText = "Text"))

        grid.Columns.Add(
            new GridColumn(
                DataCell = new CheckBoxCell(Binding = Binding.Property(fun (r: MyPoco) -> r.Check).Cast<Nullable<bool>>()),
                HeaderText = "Check"))

        this.Content <- grid
```

```python
import clr
import Eto.Forms as forms
from System.Collections.ObjectModel import ObservableCollection

try:
    prop = clr.clrproperty  # Python 3: bindings need .NET properties
except AttributeError:
    prop = lambda type: property  # IronPython: bindings use Python properties

class MyPoco(object):
    __namespace__ = "MyApp"  # Python 3: makes this a .NET class

    def __init__(self, text, check):
        self._text = text
        self._check = check

    @prop(str)
    def Text(self):
        return self._text

    @Text.setter
    def Text(self, value):
        self._text = value

    @prop(bool)
    def Check(self):
        return self._check

    @Check.setter
    def Check(self, value):
        self._check = value

class MyForm(forms.Form):
    def __init__(self):
        super(MyForm, self).__init__()
        collection = ObservableCollection[object]()
        collection.Add(MyPoco("Row 1", True))
        collection.Add(MyPoco("Row 2", False))

        grid = forms.GridView()
        grid.DataStore = collection

        textColumn = forms.GridColumn()
        textColumn.DataCell = forms.TextBoxCell("Text")
        textColumn.HeaderText = "Text"
        grid.Columns.Add(textColumn)

        checkColumn = forms.GridColumn()
        checkColumn.DataCell = forms.CheckBoxCell("Check")
        checkColumn.HeaderText = "Check"
        grid.Columns.Add(checkColumn)

        self.Content = grid
```

{{< /code-tabs >}}

## DataTable

To bind to a `DataTable` (as commonly provided by SQL sources), you can use the following strategy by converting each row to an [anonymous type](http://msdn.microsoft.com/en-us/library/bb397696.aspx) (or POCO).

{{< code-tabs >}}

```csharp
using System.Data;
using System.Linq;
using Eto.Forms;


class MyForm : Form
{
    public MyForm()
    {
        var table = new DataTable();
        table.Columns.Add("TextColumn");
        table.Columns.Add("CheckColumn", typeof(bool));

        table.Rows.Add("Row 1", true);
        table.Rows.Add("Row 2", false);

        var collection = table.Rows.Cast<DataRow>()
            .Select(x => new { Text = x[0], Check = x[1] })
            .ToList();

        var grid = new GridView { DataStore = collection };

        grid.Columns.Add(new GridColumn
        { 
            DataCell = new TextBoxCell("Text"),
            HeaderText = "Text"
        });
        grid.Columns.Add(new GridColumn
        { 
            DataCell = new CheckBoxCell("Check"),
            HeaderText = "Check"
        });

        Content = grid;
    }
}
```

```fsharp
open System.Data
open Eto.Forms

type MyForm() as this =
    inherit Form()
    do
        let table = new DataTable()
        table.Columns.Add("TextColumn") |> ignore
        table.Columns.Add("CheckColumn", typeof<bool>) |> ignore

        table.Rows.Add("Row 1", true) |> ignore
        table.Rows.Add("Row 2", false) |> ignore

        let collection =
            table.Rows
            |> Seq.cast<DataRow>
            |> Seq.map (fun x -> box {| Text = x.[0]; Check = x.[1] |})
            |> Seq.toArray

        let grid = new GridView(DataStore = collection)

        grid.Columns.Add(new GridColumn(DataCell = new TextBoxCell("Text"), HeaderText = "Text"))
        grid.Columns.Add(new GridColumn(DataCell = new CheckBoxCell("Check"), HeaderText = "Check"))

        this.Content <- grid
```

```python
import System
import Eto.Forms as forms
from System.Data import DataTable

class MyForm(forms.Form):
    def __init__(self):
        super(MyForm, self).__init__()
        table = DataTable()
        table.Columns.Add("TextColumn")
        table.Columns.Add("CheckColumn", System.Boolean)

        table.Rows.Add("Row 1", True)
        table.Rows.Add("Row 2", False)

        # each row becomes a list, and the cells bind to its items by index
        collection = [[row[0], row[1]] for row in table.Rows]

        grid = forms.GridView()
        grid.DataStore = collection

        textColumn = forms.GridColumn()
        textColumn.DataCell = forms.TextBoxCell(0)
        textColumn.HeaderText = "Text"
        grid.Columns.Add(textColumn)

        checkColumn = forms.GridColumn()
        checkColumn.DataCell = forms.CheckBoxCell(1)
        checkColumn.HeaderText = "Check"
        grid.Columns.Add(checkColumn)

        self.Content = grid
```

{{< /code-tabs >}}
