---
title: "PixelLayout"
---

The Pixel Layout provides a way to position your controls based on pixel co-ordinates.

*Note*: Using the pixel layout to position standard controls is highly discouraged, as each control may be different sizes based on the platform. It is highly recommended to use the [TableLayout](./TableLayout) or [DynamicLayout](./DynamicLayout) when using standard controls, as they will auto fit to the size of each control.

{{< code-tabs >}}

```csharp
using Eto.Forms;

public class MyForm : Form
{
    public MyForm()
    {
        var layout = new PixelLayout();

        var label = new Label { Text = "Hello!" };
        layout.Add(label, 10, 10);

        // during runtime you can move controls
        layout.Move(label, 10, 100);
        
        Content = layout;
    }
}
```

```xaml
<PixelLayout>
  <Label PixelLayout.Location="10,10">Hello!</Label>
</PixelLayout>
```

```fsharp
open Eto.Forms

type MyForm() as this =
    inherit Form()
    do
        let layout = new PixelLayout()

        let label = new Label(Text = "Hello!")
        layout.Add(label, 10, 10)

        // during runtime you can move controls
        layout.Move(label, 10, 100)

        this.Content <- layout
```

```python
import Eto.Forms as forms

class MyForm(forms.Form):
    def __init__(self):
        super(MyForm, self).__init__()
        layout = forms.PixelLayout()

        label = forms.Label()
        label.Text = "Hello!"
        layout.Add(label, 10, 10)

        # during runtime you can move controls
        layout.Move(label, 10, 100)

        self.Content = layout
```

{{< /code-tabs >}}

In XAML, `PixelLayout.Location` sets where each control starts.
