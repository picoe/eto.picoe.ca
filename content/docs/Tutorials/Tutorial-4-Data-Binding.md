---
title: "Tutorial 4 - Data Binding"
---

This tutorial connects the sign-up form from [Tutorial 3](./Tutorial-3-Table-Layout) to the view model from [Tutorial 2](./Tutorial-2-Menus-and-Toolbars) with data binding.

## Extending the view model

Rather than reading values out of each control, put them in the `MainViewModel` from [Tutorial 2](./Tutorial-2-Menus-and-Toolbars) and bind the controls to it. Add the sign-up properties and a command for the button:

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

`INotifyPropertyChanged` lets the UI update when the view model changes. `SignUpCommand` runs when the button is clicked, and the button is only enabled while the command can run, which here is once a name has been typed.

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

{{< /code-tabs >}}

Every control inside the form shares its `DataContext`, so the menu, toolbar and form all use the same view model. Typing in the text boxes updates the view model, and changes to the view model update the controls.

With XAML or JSON, the [live preview](../DevExtensions/Live-Preview.md#sample-data-in-the-preview) can fill these bindings with sample data, and IntelliSense completes the model's properties inside `{Binding}`.

See [Data Binding](../Guides/Data-Binding) for converting values, binding directions and more.
