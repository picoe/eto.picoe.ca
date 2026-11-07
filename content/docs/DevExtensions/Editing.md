---
title: "IntelliSense and Editing"
weight: 2
---

The extensions understand `.xeto` and `.jeto` files, so you get the same help writing views as you do writing C#.

## IntelliSense

Completion lists controls, properties, events and values as you type, including your own controls once the project is built. Everything matches the Eto.Forms version your project uses, so you only see the API you're building against.

- **Controls**: picking one of your own controls in a `.xeto` file adds the `xmlns` it needs.
- **Values**: enums, booleans and other known values are listed for each property.
- **Bindings**: inside `{Binding ...}`, the properties of the view's data context are listed. Set a [design data context](Live-Preview.md#sample-data-in-the-preview) so the extension knows what type that is.

In VS Code, typing `=` after a property in a `.xeto` file, or `:` in a `.jeto` file, accepts the highlighted suggestion.

## Event handlers

Completing an event, such as `Click`, lists the methods in the code behind that can handle it, or adds a new handler method for you. **Go to Definition** on a handler name jumps to its method in the code behind.

## Hover documentation

Hover over a control, property or event to see its documentation from the Eto.Forms [API reference](/api/).

## Switching between designer and code

Jump between a view and its code behind, e.g. from `MyForm.xeto` to `MyForm.xeto.cs` and back:

- **VS Code**: press **Alt+O**, or click the button in the editor's title bar.
- **Rider**: **Switch Between Eto Designer and Code** in the editor's right-click menu or the **Navigate** menu. Give it a shortcut in **Settings > Keymap**.

## Syntax highlighting

`.xeto` and `.jeto` files are highlighted as XAML and JSON, in every editor.

## Color swatches

In Visual Studio, Eto color values in your code show a swatch of the color beside them.
