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
