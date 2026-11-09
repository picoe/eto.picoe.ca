---
title: "DynamicLayout"
---

The DynamicLayout is the most flexible layout for Eto.Forms.  Internally, it uses [TableLayout](./TableLayout) Panels to create its content. Unlike other layouts, the DynamicLayout can only be modified during creation.  Once generated, the controls cannot be moved or re-positioned like a [TableLayout](./TableLayout) or [PixelLayout](./PixelLayout)

Controls in a dynamic layout can be positioned vertically and horizontally.  Each vertical set of controls can be aligned with controls in previous horizontal sections, giving a very easy way to build forms.

## Example

This example creates a top section with two columns (one for the field label, and the other for the data entry), and a second section that has three columns (a spacer and two buttons aligned to the right).

{{< code-tabs >}}

```csharp
var layout = new DynamicLayout();

layout.BeginVertical (); // create a fields section
layout.BeginHorizontal ();
layout.Add (new Label { Text = "Field 1" });
layout.Add (new TextBox (), true); // true == scale horizontally
layout.EndHorizontal ();

layout.BeginHorizontal ();
layout.Add (new Label { Text = "Field 2" });
layout.Add (new ComboBox (), true);
layout.EndHorizontal ();
layout.EndVertical ();

layout.BeginVertical (); // buttons section
layout.BeginHorizontal ();
layout.Add (null, true); // add a blank space scaled horizontally to fill space
layout.Add (new Button { Text = "Cancel" });
layout.Add (new Button { Text = "Ok" });
layout.EndHorizontal ();
layout.EndVertical ();
```

```xaml
<DynamicLayout>
  <DynamicRow>
    <DynamicTable> <!-- fields section -->
      <DynamicRow>
        <Label>Field 1</Label>
        <DynamicControl XScale="True"> <!-- scale horizontally -->
          <TextBox />
        </DynamicControl>
      </DynamicRow>
      <DynamicRow>
        <Label>Field 2</Label>
        <DynamicControl XScale="True">
          <ComboBox />
        </DynamicControl>
      </DynamicRow>
    </DynamicTable>
  </DynamicRow>
  <DynamicRow>
    <DynamicTable> <!-- buttons section -->
      <DynamicRow>
        <DynamicControl XScale="True" /> <!-- a blank space scaled horizontally to fill space -->
        <Button Text="Cancel" />
        <Button Text="Ok" />
      </DynamicRow>
    </DynamicTable>
  </DynamicRow>
</DynamicLayout>
```

```json
{
  "$type": "DynamicLayout",
  "Rows": [
    {
      /* fields section */
      "Rows": [
        [ "Field 1", { "XScale": true, "Control": { "$type": "TextBox" } } ],
        [ "Field 2", { "XScale": true, "Control": { "$type": "ComboBox" } } ]
      ]
    },
    {
      /* buttons section, where null is a blank space scaled horizontally to fill space */
      "Rows": [
        [ null, { "$type": "Button", "Text": "Cancel" }, { "$type": "Button", "Text": "Ok" } ]
      ]
    }
  ]
}
```

```fsharp
let layout = new DynamicLayout()

layout.BeginVertical() |> ignore // create a fields section
layout.BeginHorizontal() |> ignore
layout.Add(new Label(Text = "Field 1")) |> ignore
layout.Add(new TextBox(), true) |> ignore // true == scale horizontally
layout.EndHorizontal()

layout.BeginHorizontal() |> ignore
layout.Add(new Label(Text = "Field 2")) |> ignore
layout.Add(new ComboBox(), true) |> ignore
layout.EndHorizontal()
layout.EndVertical()

layout.BeginVertical() |> ignore // buttons section
layout.BeginHorizontal() |> ignore
layout.Add(null, true) |> ignore // add a blank space scaled horizontally to fill space
layout.Add(new Button(Text = "Cancel")) |> ignore
layout.Add(new Button(Text = "Ok")) |> ignore
layout.EndHorizontal()
layout.EndVertical()
```

```python
def label(text):
    l = forms.Label()
    l.Text = text
    return l

def button(text):
    b = forms.Button()
    b.Text = text
    return b

layout = forms.DynamicLayout()

layout.BeginVertical()  # create a fields section
layout.BeginHorizontal()
layout.Add(label("Field 1"))
layout.Add(forms.TextBox(), True)  # True == scale horizontally
layout.EndHorizontal()

layout.BeginHorizontal()
layout.Add(label("Field 2"))
layout.Add(forms.ComboBox(), True)
layout.EndHorizontal()
layout.EndVertical()

layout.BeginVertical()  # buttons section
layout.BeginHorizontal()
layout.Add(None, True)  # add a blank space scaled horizontally to fill space
layout.Add(button("Cancel"))
layout.Add(button("Ok"))
layout.EndHorizontal()
layout.EndVertical()
```

{{< /code-tabs >}}

Or, in short form:

{{< code-tabs >}}

```csharp
var layout = new DynamicLayout();

layout.BeginVertical (); // fields section
layout.AddRow (new Label { Text = "Field 1" }, new TextBox ());
layout.AddRow (new Label { Text = "Field 2" }, new ComboBox ());
layout.EndVertical ();

layout.BeginVertical (); // buttons section
// passing null in AddRow () creates a scaled column
layout.AddRow (null, new Button { Text = "Cancel" }, new Button { Text = "Ok" });
layout.EndVertical ();
```

```fsharp
let layout = new DynamicLayout()

layout.BeginVertical() |> ignore // fields section
layout.AddRow(new Label(Text = "Field 1"), new TextBox()) |> ignore
layout.AddRow(new Label(Text = "Field 2"), new ComboBox()) |> ignore
layout.EndVertical()

layout.BeginVertical() |> ignore // buttons section
// passing null in AddRow() creates a scaled column
layout.AddRow(null, new Button(Text = "Cancel"), new Button(Text = "Ok")) |> ignore
layout.EndVertical()
```

```python
# label() and button() as in the example above
layout = forms.DynamicLayout()

layout.BeginVertical()  # fields section
layout.AddRow(label("Field 1"), forms.TextBox())
layout.AddRow(label("Field 2"), forms.ComboBox())
layout.EndVertical()

layout.BeginVertical()  # buttons section
# passing None in AddRow() creates a scaled column
layout.AddRow(None, button("Cancel"), button("Ok"))
layout.EndVertical()
```

{{< /code-tabs >}}

Each row can also be written out in markup:

{{< code-tabs >}}

```xaml
<DynamicLayout Spacing="10,5">
    <DynamicRow>
        <Label VerticalAlignment="Center">Server url:</Label>
        <TextBox PlaceholderText="https://192.168.1.101:8443" />
    </DynamicRow>
    <DynamicRow>
        <Label VerticalAlignment="Center">Username:</Label>
        <TextBox PlaceholderText="myusername" />
    </DynamicRow>
    <DynamicRow>
        <Label VerticalAlignment="Center">Password:</Label>
        <PasswordBox Height="25" />
    </DynamicRow>
    <DynamicRow>
        <Label VerticalAlignment="Center">Client certificate fingerprint:</Label>
        <TextBox PlaceholderText="c94955f0e345b1e1248aa97abf01ec0e8f0f9dd7" />
    </DynamicRow>
    <DynamicRow>
        <CheckBox Text="Use values next time" />
    </DynamicRow>
</DynamicLayout>
```

```json
{
  "$type": "DynamicLayout",
  "Spacing": "10,5",
  "Rows": [
    [ { "$type": "Label", "Text": "Server url:", "VerticalAlignment": "Center" }, { "$type": "TextBox", "PlaceholderText": "https://192.168.1.101:8443" } ],
    [ { "$type": "Label", "Text": "Username:", "VerticalAlignment": "Center" }, { "$type": "TextBox", "PlaceholderText": "myusername" } ],
    [ { "$type": "Label", "Text": "Password:", "VerticalAlignment": "Center" }, { "$type": "PasswordBox", "Height": "25" } ],
    [ { "$type": "Label", "Text": "Client certificate fingerprint:", "VerticalAlignment": "Center" }, { "$type": "TextBox", "PlaceholderText": "c94955f0e345b1e1248aa97abf01ec0e8f0f9dd7" } ],
    [ { "$type": "CheckBox", "Text": "Use values next time" } ]
  ]
}
```

{{< /code-tabs >}}
