# Overview

Git Extensions supports color customization, specifically a user can choose between
a default (operating system defined), a bright or a dark themes.

There is a [Wiki page on Dark Theme](https://github.com/gitextensions/gitextensions/wiki/Dark-Theme)
with some background information and a list of known issues.

## Themes location

Themes are stored in *.css files. Where Git Extensions looks for theme files depends on whether it
is an 'Installed' or a 'Portable' version. Both come with a predefined set of themes in
`{App install}\Themes` directory.

In addition the Installed version looks for user-defined themes in
`%AppData%\Roaming\GitExtensions\GitExtensions\Themes` directory.

For each found theme file `ColorSettings` page will show an entry in the Theme dropdown list.

### Installed GitExtension application

#### Official themes

The Installed version of Git Extensions stores themes in `{App install}\Themes` directory.
The default theme is in `Themes\invariant.css`.

#### Custom themes

Any modifications for personal use should be saved to
`%AppData%\Roaming\GitExtensions\GitExtensions\Themes` directory, because any changes to
`{App install}\Themes` directory will be lost as soon as user updates or re-installs the app.

### Portable application

The Portable version of Git Extensions does not have a separate directory for user-defined themes.
It only uses pre-installed themes from `{App install}\Themes` directory.

## How to create a custom theme

Copy `Themes\dark.css` or `Theme\bright.css` and change color values. Use a text editor, preferably
with .css syntax highlighting and inline color display.

Currently only application colors like branch labels in the revision grid and
diff colors can be changed, the system colors listed in the templates will not be changed.
Furthermore a dark theme is only usable if the Windows system theme is dark.
(Dark mode in Windows settings is not sufficient to change system colors to be dark.)

There is a plugin for Visual Studio to show .css colors inline.

Visual Studio Code and JetBrains Rider displays .css colors inline out-of-the-box.

### Create a slightly modified theme

Use .css import directive to reuse color values from another theme.

- To import from a preinstalled theme:

```css
@import url("bright.css");
```

- To import from a user-defined theme:

```css
@import url("{UserAppData}/bright_custom.css");
```

### Specify alternative color values for colorblind users

To keep the number of themes small, color variations for colorblind users are specified without
creating a separate theme. See for example `bright.css`:

```css
.Graph            { color: #8a0000 } /* hsl(0,   100%, 27%) */
.Graph.colorblind { color: #0600a8 } /* hsl(242, 100%, 33%) */
```
