---
title: "Feature Detection"
---

Some features of Eto.Forms are only available on certain platforms. Check that a feature is supported before relying on it, so your app still works everywhere.

## Control support

To check whether the current platform has a control, use `Platform.Supports<T>()`:

{{< code-tabs >}}

```csharp
public class MyForm : Form
{
    public MyForm()
    {
        if (Platform.Supports<TrayIndicator>())
        {
            // add a tray icon
        }
    }
}
```

```fsharp
type MyForm() as this =
    inherit Form()
    do
        if this.Platform.Supports<TrayIndicator>() then
            // add a tray icon
            ()
```

```python
class MyForm(forms.Form):
    def __init__(self):
        super(MyForm, self).__init__()
        if self.Platform.Supports[forms.TrayIndicator]():
            # add a tray icon
            pass
```

{{< /code-tabs >}}

## Feature support

Controls with features that only some platforms support have a property to check first, named like `SupportsFeature` or `FeatureIsSupported`. For example:

- `TextArea.SpellCheckIsSupported`
- `ColorPicker.SupportsAllowAlpha`
- `Drawable.SupportsCreateGraphics`
- `Application.QuitIsSupported`

Some properties are hints rather than hard requirements. For example, `Dialog.DisplayMode` attaches a dialog to its parent window on platforms that can, and otherwise shows it the way that platform normally would.
