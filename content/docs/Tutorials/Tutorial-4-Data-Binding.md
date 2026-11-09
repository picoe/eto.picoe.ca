---
title: "Tutorial 4 - Data Binding"
---

This tutorial connects the sign-up form from [Tutorial 3](./Tutorial-3-Table-Layout) to the view model from [Tutorial 2](./Tutorial-2-Menus-and-Toolbars) with data binding.

## Extending the view model

Rather than reading values out of each control, put them in the `MainViewModel` from [Tutorial 2](./Tutorial-2-Menus-and-Toolbars) and bind the controls to it. Add the sign-up properties and a command for the button:

{{< code-tabs >}}

```csharp {title="MainViewModel.cs"}
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Eto.Forms;

public class MainViewModel : INotifyPropertyChanged
{
    string name;

    public MainViewModel()
    {
        // ... menu commands from Tutorial 2

        SignUpCommand = new RelayCommand(SignUp, () => !string.IsNullOrWhiteSpace(Name));
    }

    // ... command properties from Tutorial 2

    public string Name
    {
        get => name;
        set
        {
            name = value;
            OnPropertyChanged();
            SignUpCommand.UpdateCanExecute(); // enables the button once there's a name
        }
    }

    public string Email { get; set; }

    public bool Newsletter { get; set; } = true;

    public RelayCommand SignUpCommand { get; }

    void SignUp() => MessageBox.Show($"Welcome, {Name}!");

    public event PropertyChangedEventHandler PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

```fsharp {title="MainViewModel.fs"}
open System
open System.ComponentModel
open Eto.Forms

type MainViewModel() as this =
    let propertyChanged = Event<PropertyChangedEventHandler, PropertyChangedEventArgs>()
    let mutable name = ""

    // ... menu commands from Tutorial 2

    let signUpCommand =
        new RelayCommand(
            (fun () -> MessageBox.Show($"Welcome, {this.Name}!") |> ignore),
            (fun () -> not (String.IsNullOrWhiteSpace this.Name)))

    // ... command properties from Tutorial 2

    member _.Name
        with get () = name
        and set value =
            name <- value
            propertyChanged.Trigger(this, PropertyChangedEventArgs(nameof this.Name))
            signUpCommand.UpdateCanExecute() // enables the button once there's a name

    member val Email = "" with get, set

    member val Newsletter = true with get, set

    member _.SignUpCommand = signUpCommand

    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member _.PropertyChanged = propertyChanged.Publish
```

```python {title="MainViewModel.py"}
import clr
import Rhino.UI
import Eto.Forms as forms

try:
    prop = clr.clrproperty  # Python 3: bindings need .NET properties
except AttributeError:
    prop = lambda type: property  # IronPython: bindings use Python properties

class MainViewModel(Rhino.UI.ViewModel):
    __namespace__ = "MyApp"  # Python 3: makes this a .NET class

    def __init__(self):
        super(MainViewModel, self).__init__()
        # ... menu commands from Tutorial 2

        self._name = ""
        self._email = ""
        self._newsletter = True
        self._signUpCommand = forms.RelayCommand(self.SignUp, lambda: bool(self.Name and self.Name.strip()))

    @prop(str)
    def Name(self):
        return self._name

    @Name.setter
    def Name(self, value):
        self._name = value
        self.RaisePropertyChanged("Name")
        self.SignUpCommand.UpdateCanExecute()  # enables the button once there's a name

    @prop(str)
    def Email(self):
        return self._email

    @Email.setter
    def Email(self, value):
        self._email = value

    @prop(bool)
    def Newsletter(self):
        return self._newsletter

    @Newsletter.setter
    def Newsletter(self, value):
        self._newsletter = value

    @prop(forms.RelayCommand)
    def SignUpCommand(self):
        return self._signUpCommand

    def SignUp(self):
        forms.MessageBox.Show("Welcome, {}!".format(self.Name))
```

{{< /code-tabs >}}

`INotifyPropertyChanged` lets the UI update when the view model changes. `SignUpCommand` runs when the button is clicked, and the button is only enabled while the command can run, which here is once a name has been typed.

In Python, `Rhino.UI.ViewModel` provides `INotifyPropertyChanged`, and the properties are declared so .NET can see them to bind to.

## Binding the controls

The form's `DataContext` is already the view model from Tutorial 2, so bind each control to its properties:

{{< code-tabs >}}

```csharp {title="MainForm.cs"}
// after creating the layout
nameBox.TextBinding.BindDataContext((MainViewModel m) => m.Name);
emailBox.TextBinding.BindDataContext((MainViewModel m) => m.Email);
newsletterBox.CheckedBinding.BindDataContext((MainViewModel m) => m.Newsletter);
signUpButton.BindDataContext(c => c.Command, (MainViewModel m) => m.SignUpCommand);
```

```xaml {title="MainForm.xeto"}
<TableLayout Spacing="5,5">
  <TableRow>
    <Label>Name</Label>
    <TableCell ScaleWidth="True">
      <TextBox Text="{Binding Name}" />
    </TableCell>
  </TableRow>
  <TableRow>
    <Label>Email</Label>
    <TextBox Text="{Binding Email}" />
  </TableRow>
  <TableRow>
    <TableCell />
    <CheckBox Text="Send me the newsletter" Checked="{Binding Newsletter}" />
  </TableRow>
  <TableRow ScaleHeight="True" />
  <TableRow>
    <TableCell />
    <TableLayout>
      <TableRow>
        <Button Text="Sign Up" Command="{Binding SignUpCommand}" />
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
    [ "Name", { "ScaleWidth": true, "Control": { "$type": "TextBox", "Text": "{Binding Name}" } } ],
    [ "Email", { "$type": "TextBox", "Text": "{Binding Email}" } ],
    [ null, { "$type": "CheckBox", "Text": "Send me the newsletter", "Checked": "{Binding Newsletter}" } ],
    null,
    [ null, [ [ { "$type": "Button", "Text": "Sign Up", "Command": "{Binding SignUpCommand}" }, null ] ] ]
  ]
}
```

```fsharp {title="MainForm.fs"}
// after creating the layout
nameBox.TextBinding.BindDataContext(fun (m: MainViewModel) -> m.Name) |> ignore
emailBox.TextBinding.BindDataContext(fun (m: MainViewModel) -> m.Email) |> ignore
newsletterBox.CheckedBinding.Cast<bool>().BindDataContext(fun (m: MainViewModel) -> m.Newsletter) |> ignore
signUpButton.BindDataContext((fun (c: Button) -> c.Command), (fun (m: MainViewModel) -> m.SignUpCommand :> System.Windows.Input.ICommand)) |> ignore
```

```python {title="MainForm.py"}
from System.Windows.Input import ICommand

# after creating the layout
nameBox.TextBinding.BindDataContext("Name")
emailBox.TextBinding.BindDataContext("Email")
newsletterBox.CheckedBinding.BindDataContext("Newsletter")
forms.BindableExtensions.BindDataContext[ICommand](signUpButton, "Command", "SignUpCommand")
```

{{< /code-tabs >}}

Every control inside the form shares its `DataContext`, so the menu, toolbar and form all use the same view model. Typing in the text boxes updates the view model, and changes to the view model update the controls.

With XAML or JSON, the [live preview](../DevExtensions/Live-Preview.md#sample-data-in-the-preview) can fill these bindings with sample data, and IntelliSense completes the model's properties inside `{Binding}`.

See [Data Binding](../Guides/Data-Binding) for converting values, binding directions and more.
