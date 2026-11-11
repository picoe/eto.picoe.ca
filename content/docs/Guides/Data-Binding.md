---
title: "Data Binding"
---

Eto.Forms provides a very comprehensive binding mechanism that allows for direct and MVVM binding of controls.

All control and model properties can be bound together using direct or extension methods through code. Using expressions you can specify a property to bind to of a particular object, which will automatically hook into its changed event or use `INotifyPropertyChanged` to detect changes. The benefits of using expressions is that they are completely refactorable.

In some cases, you can also use lambda's or delegates to execute code which give greater flexibility for converting value types, or executing methods to get values for the binding.

## Direct vs. Indirect

There are two main types of bindings in Eto.Forms.  A Direct binding can get or set a value directly, whereas an Indirect binding get or set a value on a provided object instance.

**Direct bindings** are used for property bindings, such as when you want to bind to the Text property of a TextBox.

**Indirect bindings** are used when binding column values of a GridView, ListBox, ComboBox, etc.  The binding will retrieve the value from each object of the Control's DataStore.  These are also used for MVVM bindings.

## Property Binding Helpers

Eto.Forms supports binding to any property of a control, however most UI controls have a particular value that the user modifies.  For example, `TextBox.Text`, `DropDown.SelectedValue`, `NumericUpDown.Value`, etc.

Eto.Forms provides helpers for these values to more easily bind to them through code, usually with a 'Binding' suffix of the property. For example, `TextBox.TextBinding`, `DropDown.SelectedValueBinding`, and `NumericUpDown.ValueBinding`.

These can be used for both MVVM and Direct binding, as shown in the following sections.


## MVVM Binding

[Model View ViewModel](https://en.wikipedia.org/wiki/Model_View_ViewModel) binding allows you to bind properties of a bindable widget to properties of a [POCO](https://en.wikipedia.org/wiki/Plain_Old_CLR_Object).  The ViewModel can then be set on your container to update all child UI elements for that instance. This is the most typical case of binding, and arguably the most useful.

MVVM bindings use the `BindableWidget.DataContext` property as the binding source.  All child controls inherit their parent's `DataContext`, unless they have a value set explicitly. This allows different UI elements or sections to have different view models assigned.

MVVM bindings are created using `BindDataContext()` methods which take an indirect binding to your data model class. The control's `DataContext` is passed when getting or setting the value of the binding.

Using MVVM typically requires that your view model implement `INotifyPropertyChanged` so that any chages to its properties can be reflected in the UI.  This is only required if your view model updates its values based on certain logic. Eto.Forms will automatically subscribe to the change events of your view model properties.  *Note:* you can also use an event with a name like `[MyProperty]Changed` instead.

For example, this shows a simple model with a single MyString property and binds it to `TextBox.Text` using the `TextBox.TextBinding` helper:

{{< code-tabs >}}

```cs
public class MyForm : Form
{
    public MyForm()
    {
        var textBox = new TextBox();
        textBox.TextBinding.BindDataContext((MyModel m) => m.MyString);

        Content = textBox;

        // set the view model for the form and all child controls
        var model = new MyModel { MyString = "Hello!" };
        DataContext = model;
    }
}

// typically implemented view model
public class MyModel : INotifyPropertyChanged
{
    string myString;
    public string MyString
    {
        get { return myString; }
        set
        {
            if (myString != value)
            {
                myString = value;
                OnPropertyChanged();
            }
        }
    }

    void OnPropertyChanged([CallerMemberName] string memberName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
    }
    public event PropertyChangedEventHandler PropertyChanged;
}

```

```xaml {title="MyForm.xeto"}
<TextBox Text="{Binding MyString}" />
```

```cs {tab="xaml" title="MyForm.xeto.cs"}
public MyForm()
{
    XamlReader.Load(this);

    // set the view model for the form and all child controls
    DataContext = new MyModel { MyString = "Hello!" };
}
```

```cs {tab="xaml" title="MyModel.cs"}
// typically implemented view model
public class MyModel : INotifyPropertyChanged
{
    string myString;
    public string MyString
    {
        get { return myString; }
        set
        {
            if (myString != value)
            {
                myString = value;
                OnPropertyChanged();
            }
        }
    }

    void OnPropertyChanged([CallerMemberName] string memberName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
    }
    public event PropertyChangedEventHandler PropertyChanged;
}
```

```json {title="MyForm.jeto"}
"Content": { "$type": "TextBox", "Text": "{Binding MyString}" }
```

```cs {tab="json" title="MyForm.jeto.cs"}
public MyForm()
{
    JsonReader.Load(this);

    // set the view model for the form and all child controls
    DataContext = new MyModel { MyString = "Hello!" };
}
```

```cs {tab="json" title="MyModel.cs"}
// typically implemented view model
public class MyModel : INotifyPropertyChanged
{
    string myString;
    public string MyString
    {
        get { return myString; }
        set
        {
            if (myString != value)
            {
                myString = value;
                OnPropertyChanged();
            }
        }
    }

    void OnPropertyChanged([CallerMemberName] string memberName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
    }
    public event PropertyChangedEventHandler PropertyChanged;
}
```

```fsharp
type MyForm() as this =
    inherit Form()
    do
        let textBox = new TextBox()
        textBox.TextBinding.BindDataContext(fun (m: MyModel) -> m.MyString) |> ignore

        this.Content <- textBox

        // set the view model for the form and all child controls
        this.DataContext <- MyModel(MyString = "Hello!")

// typically implemented view model
and MyModel() =
    let propertyChanged = Event<PropertyChangedEventHandler, PropertyChangedEventArgs>()
    let mutable myString = ""

    member this.MyString
        with get () = myString
        and set value =
            if myString <> value then
                myString <- value
                propertyChanged.Trigger(this, PropertyChangedEventArgs(nameof this.MyString))

    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member _.PropertyChanged = propertyChanged.Publish
```

```python
import clr
import Rhino.UI
import Eto.Forms as forms

try:
    prop = clr.clrproperty  # Python 3: bindings need .NET properties
except AttributeError:
    prop = lambda type: property  # IronPython: bindings use Python properties

class MyForm(forms.Form):
    def __init__(self):
        super(MyForm, self).__init__()
        textBox = forms.TextBox()
        textBox.TextBinding.BindDataContext("MyString")

        self.Content = textBox

        # set the view model for the form and all child controls
        model = MyModel()
        model.MyString = "Hello!"
        self.DataContext = model

# typically implemented view model, using Rhino's INotifyPropertyChanged base class
class MyModel(Rhino.UI.ViewModel):
    __namespace__ = "MyApp"  # Python 3: makes this a .NET class

    def __init__(self):
        super(MyModel, self).__init__()
        self._myString = None

    @prop(str)
    def MyString(self):
        return self._myString

    @MyString.setter
    def MyString(self, value):
        if self._myString != value:
            self._myString = value
            self.RaisePropertyChanged("MyString")
```

{{< /code-tabs >}}

Python binds by property name, as it has no expressions like C# and F#. The properties must be declared as above so that .NET can see them.


Many times you would want to control the enabled or readonly state of controls using your view model.  This is easily achived using the `BindDataContext` directly on the control:

{{< code-tabs >}}

```cs
// for your textBox:
textBox.BindDataContext(c => c.Enabled, (MyModel m) => m.IsEnabled);

// and on your model:
public class MyModel : INotifyPropertyChanged
{
    // ... snipped for brevity

    bool isEnabled = true;
    public bool IsEnabled
    {
        get { return isEnabled; }
        set
        {
            if (isEnabled != value)
            {
                isEnabled = value;
                OnPropertyChanged();
            }
        }
    }
}

```

```xaml
<TextBox Text="{Binding MyString}" Enabled="{Binding IsEnabled}" />
```

```cs {tab="xaml" title="MyModel.cs"}
public class MyModel : INotifyPropertyChanged
{
    // ... snipped for brevity

    bool isEnabled = true;
    public bool IsEnabled
    {
        get { return isEnabled; }
        set
        {
            if (isEnabled != value)
            {
                isEnabled = value;
                OnPropertyChanged();
            }
        }
    }
}
```

```json
{ "$type": "TextBox", "Text": "{Binding MyString}", "Enabled": "{Binding IsEnabled}" }
```

```cs {tab="json" title="MyModel.cs"}
public class MyModel : INotifyPropertyChanged
{
    // ... snipped for brevity

    bool isEnabled = true;
    public bool IsEnabled
    {
        get { return isEnabled; }
        set
        {
            if (isEnabled != value)
            {
                isEnabled = value;
                OnPropertyChanged();
            }
        }
    }
}
```

```fsharp
// for your textBox:
textBox.BindDataContext((fun (c: TextBox) -> c.Enabled), (fun (m: MyModel) -> m.IsEnabled)) |> ignore

// and on your model:
and MyModel() =
    // ... snipped for brevity

    let mutable isEnabled = true

    member this.IsEnabled
        with get () = isEnabled
        and set value =
            if isEnabled <> value then
                isEnabled <- value
                propertyChanged.Trigger(this, PropertyChangedEventArgs(nameof this.IsEnabled))
```

```python
# for your textBox:
forms.BindableExtensions.BindDataContext[bool](textBox, "Enabled", "IsEnabled")

# and on your model:
class MyModel(Rhino.UI.ViewModel):
    # ... snipped for brevity

    @prop(bool)
    def IsEnabled(self):
        return self._isEnabled

    @IsEnabled.setter
    def IsEnabled(self, value):
        if self._isEnabled != value:
            self._isEnabled = value
            self.RaisePropertyChanged("IsEnabled")
```

{{< /code-tabs >}}

Here you will note that the first expression of the `BindDataContext` method specifies the property of the TextBox to bind to, and the second expression is the model binding.  An astute reader may realize that you can use the same mechanism to bind the `Text` property instead of using the `TextBinding` binding helper.

## Direct Binding

Direct binding gives you greater flexibility in binding controls together, or when MVVM is not necessary.

This type of binding can be used through the `Bind()` methods of the control or property binding helpers.

You can bind to either a specific object instance, or use [delegates](#delegates).

For example, you can bind to a specific object:

{{< code-tabs >}}

```cs
var textBox = new TextBox();

textBox.TextBinding.Bind(myModel, m => m.MyString);
// or
textBox.Bind(c => c.Enabled, myModel, m => m.MyBool);
```

```fsharp
let textBox = new TextBox()

textBox.TextBinding.Bind(myModel, fun (m: MyModel) -> m.MyString) |> ignore
// or
textBox.Bind((fun (c: TextBox) -> c.Enabled), myModel, (fun (m: MyModel) -> m.MyBool)) |> ignore
```

```python
textBox = forms.TextBox()

textBox.TextBinding.Bind(myModel, "MyString")
# or
forms.BindableExtensions.Bind[bool](textBox, "Enabled", myModel, "MyBool")
```

{{< /code-tabs >}}

## Converting Values

Sometimes the model property doesn't match the control property type, in which you will need to convert the bindings to another type.

To do this, you use the `Binding.Property()` helper along with the `Convert()`, `Cast()`, and `ToBool()` methods on the bindings.

`Binding.Property` can be used to create a property binding similar to the other binding methods.  For example, the following are identical:

{{< code-tabs >}}

```cs
textBox.TextBinding.BindDataContext((MyModel m) => m.MyString);
textBox.TextBinding.BindDataContext(Binding.Property((MyModel m) => m.MyString));
```

```fsharp
textBox.TextBinding.BindDataContext(fun (m: MyModel) -> m.MyString) |> ignore
textBox.TextBinding.BindDataContext(Binding.Property(fun (m: MyModel) -> m.MyString)) |> ignore
```

```python
textBox.TextBinding.BindDataContext("MyString")
textBox.TextBinding.BindDataContext(forms.Binding.Property[str]("MyString"))
```

{{< /code-tabs >}}

With the Binding.Property, we can then call `Convert()`, `Cast()`, etc.  For example, to convert an enum to/from a string:

{{< code-tabs >}}

```cs
textBox.TextBinding.BindDataContext(
    Binding.Property((MyModel m) => m.MyEnum)
    .Convert(r => r.ToString(), v => (MyEnum)Enum.Parse(typeof(MyEnum), v))
);
```

```fsharp
textBox.TextBinding.BindDataContext(
    Binding.Property(fun (m: MyModel) -> m.MyEnum)
        .Convert((fun r -> r.ToString()), (fun (v: string) -> Enum.Parse<MyEnum>(v)))
) |> ignore
```

{{< /code-tabs >}}

Another useful trick is for say the `NumericUpDown.ValueBinding`, which requires a double, but if your model has an `int` you can cast it like so:

{{< code-tabs >}}

```cs
var numericUpDown = new NumericUpDown();
numericUpDown.ValueBinding.Cast<int>().BindDataContext((MyModel m) => m.MyInt);
```

```fsharp
let numericUpDown = new NumericUpDown()
numericUpDown.ValueBinding.Cast<int>().BindDataContext(fun (m: MyModel) -> m.MyInt) |> ignore
```

{{< /code-tabs >}}

## Binding Direction

Binding by default uses `DualBindingMode.TwoWay`, in that view model changes update the UI, and UI changes update the view model keeping everything in sync.  You can also use `DualBindingMode.OneWay` (update UI only), `DualBindingMode.OneWayToSource` (update model only), `DualBindingMode.OneTime` (update UI when initially bound only), and `DualBindingMode.Manual`.

## Advanced Binding Topics

### Delegates

Another option other than binding to properties is that you can use delegates to get/set values.  This works for both direct and indirect bindings.

When using delegate methods, Eto.Forms cannot automatically wire up property changed events, so you must provide a way to subscribe/unsubscribe to change events if you still need this functionality. Typically though, change events may not be necessary in this case.

You can specify either a getter delegate and/or a setter delegate. Note that if you only supply a getter or setter delegate, then the binding will only be one way.

An example of using a delegate to get/set values from a text box:

{{< code-tabs >}}

```cs
var textBox = new TextBox();

// bind to a view model's string GetValue() and void SetValue(string) methods
textBox.TextBinding.BindDataContext((MyModel m) => m.GetValue(), (m, val) => m.SetValue(val));

// Similar to above, but only with a setter so the model will be updated when the UI changes
textBox.TextBinding.BindDataContext<MyModel>(null, (m, val) => m.SetValue(val));

// get/set values from an object directly:
textBox.TextBinding.Bind(() => someObject.GetValue(), val => someObject.SetValue(val));

// get/set values using methods:
string GetValue() { }
void SetValue(string value) { }
...
textBox.TextBinding.Bind(GetValue, SetValue);

// Bind other control properties using the Binding.Delegate helper:
var isEnabled = false;
textBox.Bind(c => c.Enabled, Binding.Delegate(() => isEnabled, val => isEnabled = val));
```

```fsharp
let textBox = new TextBox()

// bind to a view model's string GetValue() and void SetValue(string) methods
textBox.TextBinding.BindDataContext((fun (m: MyModel) -> m.GetValue()), (fun (m: MyModel) value -> m.SetValue(value))) |> ignore

// get/set values from an object directly:
textBox.TextBinding.Bind((fun () -> someObject.GetValue()), (fun value -> someObject.SetValue(value))) |> ignore

// Bind other control properties using the Binding.Delegate helper:
let isEnabled = ref false
textBox.Bind((fun (c: TextBox) -> c.Enabled), Binding.Delegate((fun () -> isEnabled.Value), (fun value -> isEnabled.Value <- value))) |> ignore
```

```python
import System

textBox = forms.TextBox()

# get/set values from an object directly:
textBox.TextBinding.Bind(
    System.Func[str](lambda: someObject.GetValue()),
    System.Action[str](lambda value: someObject.SetValue(value)))
```

{{< /code-tabs >}}

In Python, wrap the functions in `System.Func` and `System.Action` so the right `Bind` is picked.

### BindingCollection

Each `BindableWidget`, such as a Control contains a list of its current bindings in its `BindableWidget.Bindings` collection.  You can use this collection to modify the bindings, update, unbind, etc.

### Updating bindings manually

To update bindings of all child controls, you can use the `BindableWidget.UpdateBindings` method.  This is useful if your ViewModel does not implement `INotifyPropertyChanged` or change events for its properties, or if you just want to update the UI or model programatically with your own logic.

For example, making all your bindings `DualBindingMode.OneWay` will only update the UI when the model changes, but won't update the model when the UI does.  You can then call `myForm.UpdateBindings(BindingUpdateMode.Source)` to update your view model(s).  

### DualBinding

All bindings that are created use a `DualBinding` which specify a source and destination binding to tie them together.  You do not typically need to create or work with the dual binding directly, unless you want to change its mode or bindings after creation.

All `Bind()` and `BindDataContext()` methods return an instance of the DualBinding which you can use to update manually using its `Update()` method, `Unbind()` to unhook the binding, change its mode, etc. 