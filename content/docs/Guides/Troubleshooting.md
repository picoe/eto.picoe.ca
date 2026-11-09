---
title: "Troubleshooting"
---

When troubleshooting errors in Eto.Forms, be sure to check the inner exception(s). You can do this by adding `$exception.GetBaseException()` to the watch window after an exception is thrown.

## WPF doesn't run, but other platforms do

**Exception thrown**: `Eto.HandlerInvalidException: Could not create instance of type Eto.Forms.IForm`  
**Base exception**: `InvalidOperationException: The calling thread must be STA, because many UI components require this.`  
**Solution**: Add `[STAThread]` to your `Main()` method. The project templates already include it.

## A Mac app built on Windows doesn't run on a Mac

Windows file systems don't keep the executable bit, so it's lost on the executable inside the `.app` bundle. For testing, you can fix it on the Mac with:

```sh
chmod +x MyApp.app/Contents/MacOS/MyApp
xattr -c MyApp.app
```

To give your app to other people, [sign and notarize it](./Mac-Code-Signing-and-Notarization) instead.

## A Mac app doesn't run from the command line

You can't start an `.app` bundle with `dotnet MyApp.dll`. Use `dotnet run`, `open MyApp.app`, or run the executable inside the bundle directly:

```sh
./MyApp.app/Contents/MacOS/MyApp
```

## GTK doesn't run on Windows or macOS

GTK isn't included with Windows or macOS. See [Running GTK on Windows or macOS](../QuickStart/Supported-Platforms#running-gtk-on-windows-or-macos).
