---
title: "Tutorial 4 - Data Binding"
---

This tutorial connects the sign-up form from [Tutorial 3](./Tutorial-3-Table-Layout) to a view model with data binding.

## A view model

Rather than reading values out of each control, put them in a view model and bind the controls to it. Add this class to your project:

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Eto.Forms;

public class SignUpModel : INotifyPropertyChanged
{
    string name;

    public SignUpModel()
    {
        SignUpCommand = new RelayCommand(SignUp, () => !string.IsNullOrWhiteSpace(Name));
    }

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

`INotifyPropertyChanged` lets the UI update when the model changes. `SignUpCommand` runs when the button is clicked, and the button is only enabled while the command can run, which here is once a name has been typed.

## Binding the controls

Bind each control to the model, and set the model as the form's `DataContext`:

{{< code-tabs >}}

```csharp {title="MainForm.cs"}
// after creating the layout
nameBox.TextBinding.BindDataContext((SignUpModel m) => m.Name);
emailBox.TextBinding.BindDataContext((SignUpModel m) => m.Email);
newsletterBox.CheckedBinding.BindDataContext((SignUpModel m) => m.Newsletter);
signUpButton.BindDataContext(c => c.Command, (SignUpModel m) => m.SignUpCommand);

DataContext = new SignUpModel();
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

```csharp {tab="xaml" title="MainForm.xeto.cs"}
public MainForm()
{
    XamlReader.Load(this);
    DataContext = new SignUpModel();
}
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

```csharp {tab="json" title="MainForm.jeto.cs"}
public MainForm()
{
    JsonReader.Load(this);
    DataContext = new SignUpModel();
}
```

{{< /code-tabs >}}

Every control inside the form shares its `DataContext`, so the bindings all use the same model. Typing in the text boxes updates the model, and changes to the model update the controls.

With XAML or JSON, the [live preview](../DevExtensions/Live-Preview.md#sample-data-in-the-preview) can fill these bindings with sample data, and IntelliSense completes the model's properties inside `{Binding}`.

See [Data Binding](../Guides/Data-Binding) for converting values, binding directions and more.
