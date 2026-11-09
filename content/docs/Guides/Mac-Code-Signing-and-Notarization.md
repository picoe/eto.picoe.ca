---
title: "Mac Code Signing and Notarization"
---

To distribute a Mac app to other people it needs to be code signed and [notarized](https://developer.apple.com/documentation/security/notarizing-macos-software-before-distribution) by Apple, otherwise macOS will refuse to open it. This needs Xcode's command line tools, so it can only be done on a Mac.

Eto.Forms can sign, notarize and package your app into a `.dmg` as part of a release build, for both the Mac64 and macOS platforms:

1. Get an [Apple Developer account](https://developer.apple.com/programs/enroll/).
2. Install your **Developer ID Application** certificate in your keychain. On GitHub Actions, [`apple-actions/import-codesign-certs`](https://github.com/apple-actions/import-codesign-certs) works well.
3. Create an [app-specific password](https://account.apple.com/account/manage) for your Apple account.
4. Store your notarization credentials in the keychain under the name `AC_PASSWORD`:

    ```sh
    xcrun notarytool store-credentials "AC_PASSWORD" --apple-id "<your Apple ID>" --team-id "<your Team ID>" --password "<app-specific password>"
    ```

5. Turn it on in your `.csproj`:

    ```xml
    <PropertyGroup Condition="$([MSBuild]::IsOSPlatform(OSX)) AND $(Configuration) == 'Release'">
      <EnableCodeSigning>True</EnableCodeSigning>
      <EnableNotarization>True</EnableNotarization>
      <EnableDmgBuild>True</EnableDmgBuild>
    </PropertyGroup>
    ```

6. Build:

    ```sh
    dotnet build -c Release
    ```

## Options

| Property | What it does |
| --- | --- |
| `CodeSigningKey` | Which certificate to sign with, if you have more than one. Any part of its name, or its SHA-1. |
| `NotarizationKeychainProfile` | The keychain profile from step 4, if you used a name other than `AC_PASSWORD`. |
| `EnableNotarizationStapler` | Set to `False` to skip waiting for notarization to finish. The app still runs for users who are online, but the build is much faster. |
