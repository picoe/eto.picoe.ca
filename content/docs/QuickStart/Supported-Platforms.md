---
title: "Supported Platforms"
aliases:
  - /docs/guides/running-gtk/
---

These are the platforms your Eto.Forms app can run on. For what you need to *develop* Eto.Forms itself, see [Contributing](../Contributing).

Each platform is a NuGet package. The project templates add the right one for you; if you set up a project yourself, reference `Eto.Forms` plus the platform packages you want.

| Platform | Package | Runs on | Runtime |
| --- | --- | --- | --- |
| WPF (recommended on Windows) | [Eto.Platform.Wpf](https://www.nuget.org/packages/Eto.Platform.Wpf/) | Windows | .NET 6+ or .NET Framework 4.6.2+ |
| WinForms | [Eto.Platform.Windows](https://www.nuget.org/packages/Eto.Platform.Windows/) | Windows | .NET 6+ or .NET Framework 4.6.2+ |
| Mac64 (recommended on macOS) | [Eto.Platform.Mac64](https://www.nuget.org/packages/Eto.Platform.Mac64/) | macOS 10.15+ | .NET 6+ or Mono 6 |
| macOS | [Eto.Platform.macOS](https://www.nuget.org/packages/Eto.Platform.macOS/) | macOS 12+ | .NET 9+ with the `macos` workload |
| GTK | [Eto.Platform.Gtk](https://www.nuget.org/packages/Eto.Platform.Gtk/) | Linux, and Windows or macOS with GTK installed | .NET 6+ or Mono 6, GTK 3.20+ |

## Windows

WPF is the best fit for most apps. WinForms is there if you're embedding Eto.Forms in an existing WinForms app.

## macOS

**Mac64** uses Eto's own bindings to Cocoa. It can build the `.app` bundle on any OS, including Windows and Linux, so it suits cross-platform build machines.

**macOS** uses Microsoft's `macos` workload (`dotnet workload install macos`). It gives you the full Apple APIs, but can only be built on a Mac.

Both create an `.app` bundle in your output folder, and release builds include the .NET runtime so users don't need to install anything.

## Linux

GTK 3 is already installed on most desktop distros. The `WebView` control also needs WebKitGTK (`libwebkit2gtk-4.1` or `4.0`).

## Running GTK on Windows or macOS

For testing, you can run the GTK platform on other systems once GTK 3 is installed:

- **macOS**: `brew install gtk+3`
- **Windows**: follow the [GTK installation guide](https://www.gtk.org/docs/installations/windows/)

Then build with `-p:BuildPlatform=Gtk`.

## In development

**WinUI** (`Eto.WinUI`) is a work in progress and isn't ready for use yet.

## Other platforms

Other platforms or configurations may work but aren't supported. If a bug is reported on one, we'll (kindly) suggest updating to a supported setup. Pull requests for other platforms are welcome, and you can raise new ones for discussion on the [forums](https://github.com/picoe/Eto/discussions).
