![Git Extensions logo](https://cdn.rawgit.com/gitextensions/gitextensions/master/Bin/Logo/git-extensions-logo-final.svg) 

# Git Extensions


It also integrates with Windows Explorer and Microsoft Visual Studio (2015/2017).


<a href="https://github.com/gitextensions/gitextensions/releases/latest" rel="nofollow" style="vertical-align: -webkit-baseline-middle;"><img src="https://img.shields.io/github/downloads/gitextensions/gitextensions/total.svg?maxAge=86400" alt="Github Releases (latest)"></a>

### Current Status

The work has been split into two streams:
 * version 3.0x
 * version 2.5x

#### Version 3.0x

This stream contains the code for the new major version with a number of significant changes&mdash;both to the app and the codebase.
For more details please refer to [versions comparison](https://github.com/gitextensions/gitextensions/wiki/Compare-versions).

<table>
  <tr>
    <th>&nbsp;</th>
    <th>Windows</th>
    <th>Linux/Mac</th>
  </tr>
  <tr>
    <td>Runtime environment</td>
    <td>MS Windows 7SP1+ <br/>MS .NET Framework 4.6.1+</td>
    <td>No official support</td>
  </tr>
  <tr>
    <td>Development</td>
    <td>MS VS 2017 v15.5+, C#7.2</td>
    <td>No official support</td>
  </tr>
  <tr>
    <td>Current dev status</td>
    <td><a href="https://ci.appveyor.com/project/gitextensions/gitextensions/branch/master"><img alt="Build status" src="https://ci.appveyor.com/api/projects/status/yo5kw7sl6da8danr/branch/master?svg=true" style="max-width:100%;"></a> <a href="https://codecov.io/gh/gitextensions/gitextensions"><img alt="codecov.io" src="https://codecov.io/gh/gitextensions/gitextensions/branch/master/graph/badge.svg" style="max-width:100%;"></a><br /><a href="https://ci.appveyor.com/project/gitextensions/gitextensions/branch/master/artifacts">[ Download ]</a></td>
    <td>&nbsp;</td>
  </tr>
  <tr>
    <td>Translations</td>
    <td colspan=2><a target="_blank" style="text-decoration:none; color:black; font-size:66%" href="https://www.transifex.com/projects/p/git-extensions" 
title="See more information on Transifex.com"><img src="https://ds0k0en9abmn1.cloudfront.net/static/charts/images/tx-logo-micro.646b0065fce6.png" ></a></td>
  </tr>
</table>


#### Version 2.5x

This stream contains the last cross-platform version running both on Windows (MS .NET Framework) and on Linux/Mac (Mono).
The code is in maintenance mode with no significant active development planned. Only certain bug fixes are currently ported across, however there may be consideration given for certain features to be ported across from the v3.0x stream.

<table>
  <tr>
    <th>&nbsp;</th>
    <th>Windows</th>
    <th>Linux/Mac</th>
  </tr>
  <tr>
    <td>Runtime environment</td>
    <td>MS Windows 7SP1+ <br/>MS .NET Framework 4.6.1+</td>
    <td><a href="https://github.com/gitextensions/gitextensions/wiki/How-To:-run-Git-Extensions-on-Linux">Linux / Mac (possible)</a><br /><a href="http://www.mono-project.com/download/">Mono 5.0+</a></td>
  </tr>
  <tr>
    <td>Development</td>
    <td>MS VS 2015/2017, C#6</td>
    <td>MonoDevelop / JetBrains Rider / MS VS for Mac</td>
  </tr>
  <tr>
    <td>Current dev status</td>
    <td><a href="https://ci.appveyor.com/project/gitextensions/gitextensions/branch/release/2.51"><img alt="Build status" src="https://ci.appveyor.com/api/projects/status/yo5kw7sl6da8danr/branch/release/2.51?svg=true" style="max-width:100%;"></a></td>
    <td><a href="https://travis-ci.org/gitextensions/gitextensions"><img alt="Build status" src="https://travis-ci.org/gitextensions/gitextensions.svg?branch=release/2.51" style="max-width:100%;"></a></td>
  </tr>
  <tr>
    <td>Translations</td>
    <td colspan=2><a target="_blank" style="text-decoration:none; color:black; font-size:66%" href="https://www.transifex.com/projects/p/git-extensions" 
title="See more information on Transifex.com"><img src="https://ds0k0en9abmn1.cloudfront.net/static/charts/images/tx-logo-micro.646b0065fce6.png" ></a></td>
  </tr>
  <tr>
    <td><strong>Latest Release (v2.51.01)</strong></td>
    <td colspan=2><a href="https://github.com/gitextensions/gitextensions/releases/tag/v2.51.01">[ Download ]</a> / <a href="https://github.com/gitextensions/gitextensions/blob/release/2.51/GitUI/Resources/ChangeLog.md#version-25101-11-mar-2018">Changelog</a><br />
        GitHub: <a href="https://github.com/gitextensions/gitextensions/releases/latest" rel="nofollow" style="vertical-align: -webkit-baseline-middle;"><img src="https://img.shields.io/github/downloads/gitextensions/gitextensions/latest/total.svg?maxAge=86400" alt="Github Releases (latest)"></a> | SourceForge: <a href="https://sourceforge.net/projects/gitextensions/" rel="nofollow" style="vertical-align: -webkit-baseline-middle;"><img src="https://img.shields.io/sourceforge/dm/gitextensions.svg" alt="SourceForge"></a></td>
  </tr>
</table>


## Download

<table>
  <tr>
    <td><strong>Visual Studio VSIX (2015/2017)</strong></td>
    <td><a href="https://marketplace.visualstudio.com/items?itemName=HenkWesthuis.GitExtensions">[ Download ]</a> or install via Visual Studio</td>
  </tr>
  <tr>
    <td><strong>Visual Studio addin (2010/2012/2013)</strong></td>
    <td>Included with installer for the 2.x branch. <a href="https://github.com/gitextensions/gitextensions/releases/latest">Download and run setup.exe</a></td>
  </tr>
  <tr>
    <td><strong>Visual Studio Code VSIX</strong><br />Kudos to <a href="/pmiossec" class="author text-inherit">@pmiossec</a></td>
    <td><a href="https://marketplace.visualstudio.com/items?itemName=pmiossec.vscode-gitextensions">[ Download ]</a> or install via VSCode<br />
      NB: Please direct all discussions about the VSIX to <a href="https://github.com/pmiossec/vscode-gitextensions">its own repo</a>.</td>
  </tr>
</table>


## Shoutouts
 * Special thanks to JetBRAINS for [ReSharper](https://www.jetbrains.com/resharper/) licenses.


## Links

* Website: [gitextensions.github.io](https://gitextensions.github.io/)
* Source code: [github.com/gitextensions/gitextensions](https://github.com/gitextensions/gitextensions)
* Online manual: [git-extensions-documentation.readthedocs.org](https://git-extensions-documentation.readthedocs.org/en/latest/)
* Issue tracker: [github.com/gitextensions/gitextensions/issues](https://github.com/gitextensions/gitextensions/issues)
* Wiki: [github.com/gitextensions/gitextensions/wiki](https://github.com/gitextensions/gitextensions/wiki)
* Gitter chat: [gitter.im/gitextensions/gitextensions](https://gitter.im/gitextensions/gitextensions?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) [![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/gitextensions/gitextensions?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) 
* Mailing list: [groups.google.com/group/gitextensions](https://groups.google.com/group/gitextensions)

