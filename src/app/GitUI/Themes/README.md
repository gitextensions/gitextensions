# Overview

Git Extensions supports color customization to some extent:

- Activate Windows light or dark app mode
- Customize app spceific colors

There is no full theme support, system colors cannot be changed.

Git Extensions includes a light+ theme (slightly darker panels compared to the built-in light theme),
a dark theme for system dark mode and a dark+ mode with slightly darker panels.

## Themes location

Themes are stored in *.css files. Where Git Extensions looks for theme files depends on whether it
is an 'Installed' or a 'Portable' version. Both come with a predefined set of themes in
`{App install}\Themes` directory.

In addition the Installed version looks for user-defined themes in
`%AppData%\GitExtensions\GitExtensions\Themes` directory.

For each found theme file `ColorSettings` page will show an entry in the Theme dropdown list.

### Installed GitExtension application

#### Official themes

The Installed version of Git Extensions stores themes in `{App install}\Themes` directory.
The default theme is in `Themes\invariant.css`.

#### Custom themes

Any modifications for personal use should be saved to
`%AppData%\GitExtensions\GitExtensions\Themes` directory, because any changes to
`{App install}\Themes` directory will be lost as soon as user updates or re-installs the app.

### Portable application

The Portable version of Git Extensions does not have a separate directory for user-defined themes.
It only uses pre-installed themes from `{App install}\Themes` directory.

## How to create a custom theme

Copy `Themes\dark.css` or `Theme\invariant.css` and change color values. Use a text editor, preferably
with .css syntax highlighting and inline color display.

Only application colors like branch labels in the revision grid and
diff colors can be changed, the system colors cannot be changed.

There is an extension for Visual Studio to show .css colors inline.

Visual Studio Code and JetBrains Rider displays .css colors inline out-of-the-box.

### Create a slightly modified theme

Use .css import directive to reuse color values from another theme.

- To import from a preinstalled theme:

```css
@import url("dark.css");
```

- To import from a user-defined theme:

```css
@import url("{UserAppData}/dark_custom.css");
```

### Specify alternative color values for colorblind users

To keep the number of themes small, color variations for colorblind users are specified without
creating a separate theme. See for example `dark.css`:

```css
.AnsiTerminalRedBackNormal { color: #620707; }
.AnsiTerminalRedBackNormal.colorblind { color: #080646; }
```
