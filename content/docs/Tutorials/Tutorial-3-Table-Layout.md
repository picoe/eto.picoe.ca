---
title: "Tutorial 3 - Layout and Data Binding"
---

This tutorial builds a small sign-up form. It shows how to lay out a form so it fits every platform, and how to connect its controls to a view model with data binding.

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

## A view model

Rather than reading values out of each control, put them in a view model and bind the controls to it. Add this class to your project:

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Eto.Forms;

public class SignUpModel : INotifyPropertyChanged
{
    string name;
    public string Name
    {
        get => name;
        set { name = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanSignUp)); }
    }

    public string Email { get; set; }

    public bool Newsletter { get; set; } = true;

    public bool CanSignUp => !string.IsNullOrWhiteSpace(Name);

    public RelayCommand SignUpCommand => new RelayCommand(() =>
        MessageBox.Show($"Welcome, {Name}!"));

    public event PropertyChangedEventHandler PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

`INotifyPropertyChanged` lets the UI update when the model changes. Here, typing a name updates `CanSignUp`, which enables the button.

## Binding the controls

After creating the layout, bind each control to the model and set it as the form's `DataContext`:

```csharp
nameBox.TextBinding.BindDataContext((SignUpModel m) => m.Name);
emailBox.TextBinding.BindDataContext((SignUpModel m) => m.Email);
newsletterBox.CheckedBinding.BindDataContext((SignUpModel m) => m.Newsletter);
signUpButton.BindDataContext(c => c.Enabled, (SignUpModel m) => m.CanSignUp);
signUpButton.BindDataContext(c => c.Command, (SignUpModel m) => m.SignUpCommand);

DataContext = new SignUpModel();
```

Every control inside the form shares its `DataContext`, so the bindings all use the same model. Typing in the text boxes updates the model, and changes to the model update the controls.

If you write your UI in a `.xeto` file instead, the same bindings look like `<TextBox Text="{Binding Name}" />`, and the [live preview](../DevExtensions/Live-Preview.md#sample-data-in-the-preview) can fill them with sample data.

See [Data Binding](../Guides/Data-Binding) for converting values, binding directions and more.
