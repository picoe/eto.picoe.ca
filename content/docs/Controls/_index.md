---
title: "Controls"
weight: 5
---

Eto.Forms comes with a standard set of built-in controls that work across all supported platforms.  Controls can be subclassed to override or implement behaviour.  Each control has an implementation for each supported platform, which can be customized or replaced entirely if a particular implementation is not suitable for your use.

Custom controls can either be composed entirely of Eto.Forms controls, which would work across all platforms.  If you wish to create a more complex control, or use functionality of a platform that isn't exposed in Eto.Forms, then you can create your own custom controls with implementations for each platform (see [Custom Platform Controls](../Guides/Custom-Platform-Controls)).

The standard controls are:

* [Button](/api/type/eto.forms.button/) - Standard button with text
* [Calendar](/api/type/eto.forms.calendar/) - Calendar control to pick a date or range of dates
* [CheckBox](/api/type/eto.forms.checkbox/) - Check box with a label
* [ColorPicker](/api/type/eto.forms.colorpicker/) - Pick a single color value
* [ComboBox](/api/type/eto.forms.combobox/) - Text entry with a drop down list of items
* [DateTimePicker](/api/type/eto.forms.datetimepicker/) - Control to enter a date and/or time
* [Drawable](/api/type/eto.forms.drawable/) - Owner-drawn control using [Graphics](/api/type/eto.drawing.graphics/) object
* [DropDown](/api/type/eto.forms.dropdown/) - Drop down with a list of items
* [EnumDropDown](/api/type/eto.forms.enumdropdown-1/) - A simple way to create a drop down with values from an Enumeration
* [EnumRadioButtonList](/api/type/eto.forms.enumradiobuttonlist-1/) - Manages a list of radio buttons from an Enumeration
* [GridView](./GridView) - A virtualized grid of data with editable cells
* [GroupBox](/api/type/eto.forms.groupbox/) - A panel with a border and optional title
* [ImageView](/api/type/eto.forms.imageview/) - A view to display a single image
* [Label](/api/type/eto.forms.label/) - Displays text
* [LinkButton](/api/type/eto.forms.linkbutton/) - A simple label that acts like a button, similar to a hyperlink
* [ListBox](/api/type/eto.forms.listbox/) - A scrollable list of items
* [ListControl](/api/type/eto.forms.listcontrol/) - Base for ListBox, DropDown, ComboBox, and other list-type controls
* [MaskedTextBox](/api/type/eto.forms.maskedtextbox/) - TextBox for variable or fixed length masks
* [Navigation](/api/type/eto.forms.navigation/) - (mobile) - A pane that can present multiple pages
* [NumericUpDown](/api/type/eto.forms.numericupdown/) - Numeric control that allows the user to adjust the value with the mouse
* [Panel](/api/type/eto.forms.panel/) - A blank panel container to add other controls
* [PasswordBox](/api/type/eto.forms.passwordbox/) - Enter passwords or sensitive data
* [ProgressBar](/api/type/eto.forms.progressbar/) - Show progress of long running tasks
* [RadioButton](/api/type/eto.forms.radiobutton/) - Used in a group of radio buttons to allow user to select from values
* [RadioButtonList](/api/type/eto.forms.radiobuttonlist/) - Manages a list of radio buttons
* [RichTextArea](/api/type/eto.forms.richtextarea/) - Multi-line text area with rich text formatting
* [Scrollable](/api/type/eto.forms.scrollable/) - A scrollable container
* [SearchBox](/api/type/eto.forms.searchbox/) - A text box with search-box functionality
* [Slider](/api/type/eto.forms.slider/) - A horizontal or vertical slider to select a value from a range
* [Spinner](/api/type/eto.forms.spinner/) - A spinner to show indeterminate progress in compact space
* [Splitter](/api/type/eto.forms.splitter/) - Splits two panes horizontally or vertically
* [TabControl](/api/type/eto.forms.tabcontrol/) - Presents multiple TabPage containers which the user can select
* [TabPage](/api/type/eto.forms.tabpage/) - A single page of a TabControl
* [TextArea](/api/type/eto.forms.textarea/) - Multi-line text control with scrollbars
* [TextBox](/api/type/eto.forms.textbox/) - Single line text input
* [TextControl](/api/type/eto.forms.textcontrol/) - Base for any control that contains text
* [TreeView](/api/type/eto.forms.treeview/) - A control to present nodes in a tree
* [TreeGridView](/api/type/eto.forms.treegridview/) - A TreeView with columns
* [WebView](/api/type/eto.forms.webview/) - Control to present a web page through a url or static HTML

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

* [Form](/api/type/eto.forms.form/) - A modeless window
* [Dialog](/api/type/eto.forms.dialog/) - A modal dialog with no result value
* [Dialog&lt;T&gt;](/api/type/eto.forms.dialog-1/) - A modal dialog with a custom result value

## Dialogs

* [ColorDialog](/api/type/eto.forms.colordialog/) - Select a color
* [FontDialog](/api/type/eto.forms.fontdialog/) - Choose a font and style
* [OpenFileDialog](/api/type/eto.forms.openfiledialog/) - Open an existing file or files matching a set of patterns
* [SaveFileDialog](/api/type/eto.forms.savefiledialog/) - Choose a file and format to save to
* [SelectFolderDialog](/api/type/eto.forms.selectfolderdialog/) - Select a folder
* [MessageBox](/api/type/eto.forms.messagebox/) - Show a standard message box with specific buttons
