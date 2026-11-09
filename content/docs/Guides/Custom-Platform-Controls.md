---
title: "Custom Platform Controls"
---

Eto.Forms comes with many standard controls, however sometimes you may want to make full use of each platform's abilities, or if you want to re-use platform specific code already written. Custom platform controls can be created for this purpose.

Custom platform controls can have implementations for each platform, and are implemented exactly the way standard Eto.Forms controls are. You can either extend an existing control and provide a custom handler for each platform, or create a completely custom control.

All supportable Eto.Forms control handlers are registered through the ``Platform`` object.  When creating a new Eto.Forms control (custom or not), the ``Platform`` looks up the control handler via its interface type specified in the ``HandlerAttribute``.

The drawback of creating custom platform controls is that you have to create a handler implementation of the control for each platform you wish to use it with.

## Example

This is an example of creating a custom text box control.

In your cross-platform library:

{{< code-tabs >}}

```csharp
// control to use in your eto.forms code
[Handler(typeof(IMyCustomControl))]
public class MyCustomControl : Control
{
    IMyCustomControl Handler { get { return (IMyCustomControl)base.Handler; } }

    public string MyProperty
    {
        get { return Handler.MyProperty; }
        set { Handler.MyProperty = value; }
    }

    // interface to the platform implementations
    public new interface IMyCustomControl : Control.IHandler
    {
        string MyProperty { get; set; }
    }
}
```

```fsharp
// interface to the platform implementations
type IMyCustomControl =
    inherit Control.IHandler
    abstract MyProperty: string with get, set

// control to use in your eto.forms code
[<Handler(typeof<IMyCustomControl>)>]
type MyCustomControl() =
    inherit Control()

    member private this.CustomHandler = this.Handler :?> IMyCustomControl

    member this.MyProperty
        with get () = this.CustomHandler.MyProperty
        and set value = this.CustomHandler.MyProperty <- value
```

{{< /code-tabs >}}

F# can't nest types, so the interface sits beside the control rather than inside it.

In your Mac platform assembly:

{{< code-tabs >}}

```csharp
public class MyCustomControlHandler : MacControl<NSTextField, MyCustomControl, MyCustomControl.ICallback>, MyCustomControl.IMyCustomControl
{
    public MyCustomControlHandler ()
    {
        this.Control = new NSTextField ();
    }

    public string MyProperty
    {
        get { return this.Control.StringValue; }
        set { this.Control.StringValue = value ?? string.Empty; }
    }
}
```

```fsharp
type MyCustomControlHandler() as this =
    inherit MacControl<NSTextField, MyCustomControl, Control.ICallback>()

    do this.Control <- new NSTextField()

    interface IMyCustomControl with
        member this.MyProperty
            with get () = this.Control.StringValue
            and set value = this.Control.StringValue <- (if isNull value then "" else value)
```

{{< /code-tabs >}}

In your WPF assembly:

{{< code-tabs >}}

```csharp
public class MyCustomControlHandler : WpfControl<System.Windows.Controls.TextBox, MyCustomControl, MyCustomControl.ICallback>, MyCustomControl.IMyCustomControl
{
    public MyCustomControlHandler ()
    {
        this.Control = new System.Windows.Controls.TextBox ();
    }

    public string MyProperty
    {
        get { return this.Control.Text; }
        set { this.Control.Text = value; }
    }
}
```

```fsharp
type MyCustomControlHandler() as this =
    inherit WpfControl<System.Windows.Controls.TextBox, MyCustomControl, Control.ICallback>()

    do this.Control <- new System.Windows.Controls.TextBox()

    interface IMyCustomControl with
        member this.MyProperty
            with get () = this.Control.Text
            and set value = this.Control.Text <- value
```

{{< /code-tabs >}}

To register your new control, you must add it to the platform instance.  This can be done *before* creating your application in your main() method.

{{< code-tabs >}}

```csharp
var platform = new Eto.Mac.Platform(); // mac platform

// to register your custom control handler, call this before using your class:
platform.Add<MyCustomControl.IMyCustomControl>(() => new MyCustomControlHandler());

// create application object, and run
new Application(platform).Run(
    new Form
    {
        Title = "Hello!",
        Content = new MyCustomControl { MyProperty = "There" }
    }
);
```

```fsharp
let platform = new Eto.Mac.Platform() // mac platform

// to register your custom control handler, call this before using your class:
platform.Add<IMyCustomControl>(fun () -> new MyCustomControlHandler())

// create application object, and run
(new Application(platform)).Run(new Form(Title = "Hello!", Content = new MyCustomControl(MyProperty = "There")))
```

{{< /code-tabs >}}
