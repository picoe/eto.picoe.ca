---
title: "Publishing Your Application"
---

How you package your app depends on the OS you're shipping to. With the template's single project, build each one with `-p:BuildPlatform=<platform>` (see [Quick Start](../QuickStart#building-for-each-platform)).

## Windows

Build your project with `dotnet publish -c Release`, or from your IDE. To build Windows apps on macOS or Linux, add `<EnableWindowsTargeting>true</EnableWindowsTargeting>` to your project.

Users usually install Windows apps with an MSIX or MSI installer. See [Microsoft's packaging guide](https://learn.microsoft.com/en-us/windows/msix/desktop/desktop-to-uwp-packaging-dot-net) to set one up.

## macOS

A Mac build creates an `.app` bundle in your output folder, and release builds include the .NET runtime so users don't need to install anything. To turn this off, set `<MacBundleDotNet>False</MacBundleDotNet>`.

To ship it, the app needs to be signed, notarized and packaged in a `.dmg`. Eto.Forms can do all of this as part of your build; see [Mac Code Signing and Notarization](./Mac-Code-Signing-and-Notarization).

Mac64 apps can be built on any OS, but signing and notarizing has to be done on a Mac. The macOS platform can only be built on a Mac.

## Linux

Build your project with `dotnet publish -c Release`, or from your IDE. Keep these in mind:

- **Dependencies**: users need GTK 3, plus WebKitGTK if you use `WebView`. These come with most desktop distros. They also need the .NET runtime, unless you publish self-contained with `--self-contained`.
- **Icons**: GNOME-based distros (such as Ubuntu and Fedora) only show your app's icon if you include a [.desktop file](https://specifications.freedesktop.org/desktop-entry-spec/latest/).
- **Executable bit**: if you build on Windows, the executable bit is lost. Users will need to run `chmod +x MyApp` before it will start.

To package your app with its dependencies:

- **Distro packages**: [.deb](https://www.internalpointers.com/post/build-binary-deb-package-practical-guide) for Debian and Ubuntu, or [.rpm](https://www.redhat.com/sysadmin/create-rpm-package) for Fedora and Red Hat. These can list .NET as a dependency where the distro provides it.
- **Universal packages**: [AppImage](https://appimage.org) for a single file that runs anywhere, or [Flatpak](https://docs.flatpak.org/en/latest/first-build.html) for a sandboxed app installed from a store such as Flathub.
