---
title: "Controls"
weight: 4
---

# Controls

Eto.Forms comes with a standard set of built-in controls that work across all supported platforms.  Controls can be subclassed to override or implement behaviour.  Each control has an implementation for each supported platform, which can be customized or replaced entirely if a particular implementation is not suitable for your use.

Custom controls can either be composed entirely of Eto.Forms controls, which would work across all platforms.  If you wish to create a more complex control, or use functionality of a platform that isn't exposed in Eto.Forms, then you can create your own custom controls with implementations for each platform (see [Custom Platform Controls](../Guides/Custom-Platform-Controls)).

The standard controls are:

* [Button](/api/Eto/Forms/Button) - Standard button with text
* [Calendar](/api/Eto/Forms/Calendar) - Calendar control to pick a date or range of dates
* [CheckBox](/api/Eto/Forms/CheckBox) - Check box with a label
* [ColorPicker](/api/Eto/Forms/ColorPicker) - Pick a single color value
* [ComboBox](/api/Eto/Forms/ComboBox) - Text entry with a drop down list of items
* [DateTimePicker](/api/Eto/Forms/DateTimePicker) - Control to enter a date and/or time
* [Drawable](/api/Eto/Forms/Drawable) - Owner-drawn control using [Graphics](/api/Eto/Drawing/Graphics) object
* [DropDown](/api/Eto/Forms/DropDown) - Drop down with a list of items
* [EnumDropDown](/api/Eto/Forms/EnumDropDown-1) - A simple way to create a drop down with values from an Enumeration
* [EnumRadioButtonList](/api/Eto/Forms/EnumRadioButtonList-1) - Manages a list of radio buttons from an Enumeration
* [GridView](./GridView) - A virtualized grid of data with editable cells
* [GroupBox](/api/Eto/Forms/GroupBox) - A panel with a border and optional title
* [ImageView](/api/Eto/Forms/ImageView) - A view to display a single image
* [Label](/api/Eto/Forms/Label) - Displays text
* [LinkButton](/api/Eto/Forms/LinkButton) - A simple label that acts like a button, similar to a hyperlink
* [ListBox](/api/Eto/Forms/ListBox) - A scrollable list of items
* [ListControl](/api/Eto/Forms/ListControl) - Base for ListBox, DropDown, ComboBox, and other list-type controls
* [MaskedTextBox](/api/Eto/Forms/MaskedTextBox) - TextBox for variable or fixed length masks
* [Navigation](/api/Eto/Forms/Navigation) - (mobile) - A pane that can present multiple pages
* [NumericUpDown](/api/Eto/Forms/NumericUpDown) - Numeric control that allows the user to adjust the value with the mouse
* [Panel](/api/Eto/Forms/Panel) - A blank panel container to add other controls
* [PasswordBox](/api/Eto/Forms/PasswordBox) - Enter passwords or sensitive data
* [ProgressBar](/api/Eto/Forms/ProgressBar) - Show progress of long running tasks
* [RadioButton](/api/Eto/Forms/RadioButton) - Used in a group of radio buttons to allow user to select from values
* [RadioButtonList](/api/Eto/Forms/RadioButtonList) - Manages a list of radio buttons
* [RichTextArea](/api/Eto/Forms/RichTextArea) - Multi-line text area with rich text formatting
* [Scrollable](/api/Eto/Forms/Scrollable) - A scrollable container
* [SearchBox](/api/Eto/Forms/SearchBox) - A text box with search-box functionality
* [Slider](/api/Eto/Forms/Slider) - A horizontal or vertical slider to select a value from a range
* [Spinner](/api/Eto/Forms/Spinner) - A spinner to show indeterminate progress in compact space
* [Splitter](/api/Eto/Forms/Splitter) - Splits two panes horizontally or vertically
* [TabControl](/api/Eto/Forms/TabControl) - Presents multiple TabPage containers which the user can select
* [TabPage](/api/Eto/Forms/TabPage) - A single page of a TabControl
* [TextArea](/api/Eto/Forms/TextArea) - Multi-line text control with scrollbars
* [TextBox](/api/Eto/Forms/TextBox) - Single line text input
* [TextControl](/api/Eto/Forms/TextControl) - Base for any control that contains text
* [TreeView](/api/Eto/Forms/TreeView) - A control to present nodes in a tree
* [TreeGridView](/api/Eto/Forms/TreeGridView) - A TreeView with columns
* [WebView](/api/Eto/Forms/WebView) - Control to present a web page through a url or static HTML

## Containers

Container controls act as a control and can be added as a child to any other container. The layout controls supported are:

* [Panel](https://github.com/picoe/Eto/blob/develop/src/Eto/Forms/Controls/Panel.cs) - Controls that subclass ``Panel``, such as ``Window``, ``GroupBox``, ``Scrollable``, etc allow you to specify a single child Content control
* [TableLayout](./TableLayout) - Similar to how an HTML table works, with a single control per cell
* [PixelLayout](./PixelLayout) - Specify X,Y co-ordinates for the position of each control (from Upper-Left)
* [DynamicLayout](./DynamicLayout) - Dynamic hierarchical horizontal and vertical layout
* [StackLayout](./StackLayout) - Horizontal or Vertical list of controls

Containers are implemented so that you do *not* need to specify a size for the container (although you can if you want).  This allows containers to automatically size to fit the child controls, which works nicely across different platforms.

For example, if you created a ``Dialog`` or ``Form`` and didn't specify any sizes for any controls, it should automatically size to fit its contents.

## Windows

* [Form](/api/Eto/Forms/Form) - A modeless window
* [Dialog](/api/Eto/Forms/Dialog) - A modal dialog with no result value
* [Dialog&lt;T&gt;](/api/Eto/Forms/Dialog-1) - A modal dialog with a custom result value

## Dialogs

* [ColorDialog](/api/Eto/Forms/ColorDialog) - Select a color
* [FontDialog](/api/Eto/Forms/FontDialog) - Choose a font and style
* [OpenFileDialog](/api/Eto/Forms/OpenFileDialog) - Open an existing file or files matching a set of patterns
* [SaveFileDialog](/api/Eto/Forms/SaveFileDialog) - Choose a file and format to save to
* [SelectFolderDialog](/api/Eto/Forms/SelectFolderDialog) - Select a folder
* [MessageBox](/api/Eto/Forms/MessageBox) - Show a standard message box with specific buttons
