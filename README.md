![Git Extensions logo](https://cdn.rawgit.com/gitextensions/gitextensions/master/Logo/git-extensions-logo.svg)

# Git Extensions

Git Extensions is a standalone UI tool for managing git repositories.
It also integrates with Windows Explorer and Microsoft Visual Studio (2015/2017/2019).

Have a question? Come and talk to us: [![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/gitextensions/gitextensions?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) or send us a tweet [![@git_extensions](https://img.shields.io/badge/twitter-%40git__extensions-blue)](https://twitter.com/git_extensions)

## Current Status

<a href="#backers" alt="sponsors on Open Collective"><img src="https://opencollective.com/gitextensions/backers/badge.svg" /></a> <a href="#sponsors" alt="Sponsors on Open Collective"><img src="https://opencollective.com/gitextensions/sponsors/badge.svg" /></a>

### Version 3.x

<table>
  <tr>
    <th>&nbsp;</th>
    <th>Windows only</th>
  </tr>
  <tr>
    <td>
      Runtime environment
    </td>
    <td>
      MS Windows 7SP1+ <br />
      MS .NET Framework 4.6.1+
    </td>
  </tr>
  <tr>
    <td>
      Development
    </td>
    <td>
      MS VS 2019 (v16.8+), C# 9<br />
      VC++ (inc. ATL for x86/x64 for installer)
    </td>
  </tr>
  <tr>
    <td>
      Current dev status
    </td>
    <td>
      <a href="https://ci.appveyor.com/project/gitextensions/gitextensions/branch/master"><img alt="Build status" src="https://ci.appveyor.com/api/projects/status/yo5kw7sl6da8danr/branch/master?svg=true" style="max-width:100%;"></a> <a href="https://codecov.io/gh/gitextensions/gitextensions"><img alt="codecov.io" src="https://codecov.io/gh/gitextensions/gitextensions/branch/master/graph/badge.svg" style="max-width:100%;"></a>
    </td>
  </tr>
  <tr>
    <td>
      Translations
    </td>
    <td>
      <a target="_blank" style="text-decoration:none; color:black; font-size:66%" href="https://github.com/gitextensions/gitextensions/wiki/Translations" title="More information in the wiki"><img src="https://ds0k0en9abmn1.cloudfront.net/static/charts/images/tx-logo-micro.646b0065fce6.png" ></a>
    </td>
  </tr>
</table>

#### [Build instructions](https://github.com/gitextensions/gitextensions/wiki/How-To%3A-build-instructions)

### Version 2.5x

<details>
<summary>This stream contains the last cross-platform version running both on Windows (MS .NET Framework) and on Linux/Mac (Mono). [More...]</summary>
This stream contains the last cross-platform version running both on Windows (MS .NET Framework) and on Linux/Mac (Mono).<br />
The code is in maintenance mode with no significant active development planned. Only certain bug fixes are currently ported across, however there may be consideration given for certain features to be ported across from the v3.x stream.

For more details please refer to [versions comparison](https://github.com/gitextensions/gitextensions/wiki/Compare-versions).

<table>
  <tr>
    <th>&nbsp;</th>
    <th>Windows</th>
    <th>Linux/Mac</th>
  </tr>
  <tr>
    <td>
      Runtime environment
    </td>
    <td>
      MS Windows 7SP1+ <br />
      MS .NET Framework 4.6.1+
    </td>
    <td>
      <a href="https://github.com/gitextensions/gitextensions/wiki/How-To:-run-Git-Extensions-on-Linux">Linux / Mac (possible)</a><br />
      <a href="http://www.mono-project.com/download/">Mono 5.0+ (Recommended 5.14.0 or later)</a>
    </td>
  </tr>
  <tr>
    <td>
      Development
    </td>
    <td>
      MS VS 2015/2017, C#6
    </td>
    <td>
      MonoDevelop / JetBrains Rider / MS VS for Mac
    </td>
  </tr>
  <tr>
    <td>
      Current dev status
    </td>
    <td colspan=2>
      <a href="https://ci.appveyor.com/project/gitextensions/gitextensions/branch/release/2.51"><img alt="Build status" src="https://ci.appveyor.com/api/projects/status/yo5kw7sl6da8danr/branch/release/2.51?svg=true" style="max-width:100%;"></a> <a href="https://codecov.io/gh/gitextensions/gitextensions"><img alt="codecov.io" src="https://codecov.io/gh/gitextensions/gitextensions/branch/release/2.51/graph/badge.svg" style="max-width:100%;"></a>
    </td>
  </tr>
  <tr>
    <td>
      Translations
    </td>
    <td colspan=2>
      <a target="_blank" style="text-decoration:none; color:black; font-size:66%" href="https://github.com/gitextensions/gitextensions/wiki/Translations" title="More information in the wiki"><img src="https://ds0k0en9abmn1.cloudfront.net/static/charts/images/tx-logo-micro.646b0065fce6.png" ></a>
    </td>
  </tr>
</table>

#### [Build instructions](https://github.com/gitextensions/gitextensions/wiki/How-To:-build-instructions#v3x-and-older)

</details>

## Downloads

<a href="https://github.com/gitextensions/gitextensions/releases" rel="nofollow" style="vertical-align: -webkit-baseline-middle;"><img src="https://img.shields.io/github/downloads/gitextensions/gitextensions/total.svg?label=GitHub%20downloads%20(total)&cacheSeconds=86400"></a> <a href="https://sourceforge.net/projects/gitextensions/files" rel="nofollow" style="vertical-align: -webkit-baseline-middle;"><img src="https://img.shields.io/sourceforge/dt/gitextensions.svg?label=SourceForge%20downloads%20(total)&cacheSeconds=86400"></a> <a href="https://chocolatey.org/packages/gitextensions" rel="nofollow" style="vertical-align: -webkit-baseline-middle;"><img src="https://img.shields.io/chocolatey/dt/gitextensions.svg?label=Chocolatey%20downloads%20(total)&cacheSeconds=86400"></a>

**[Download it now](https://github.com/gitextensions/gitextensions/releases/latest)** or install it with [Chocolatey](https://chocolatey.org/packages/gitextensions) or [AppGet](https://appget.net/packages/i/gitextensions).

If you want to **update a portable version**, you should delete all the files and the subfolders from the existing folder except:

* _GitExtensions.settings_
* _WindowPositions.xml_
* User defined themes in folder _Themes_

<table>
  <tr>
    <td>
      <strong>v3.5</strong>
    </td>
    <td>
      <a href="https://github.com/gitextensions/gitextensions/releases/latest">[ Download ]</a><br />
      <a href="https://github.com/gitextensions/gitextensions/releases/latest" rel="nofollow" style="vertical-align: -webkit-baseline-middle;"><img src="https://img.shields.io/github/downloads/gitextensions/gitextensions/latest/total.svg?label=GitHub%20downloads%20(latest)&cacheSeconds=3600"></a> <a href="https://sourceforge.net/projects/gitextensions/" rel="nofollow" style="vertical-align: -webkit-baseline-middle;"><img src="https://img.shields.io/sourceforge/dt/gitextensions/v3.4.3.svg?label=SourceForge%20downloads%20(latest)&cacheSeconds=3600"></a>
    </td>
  </tr>
  <tr>
    <td>
      <strong>Current dev stream</strong><br />
      NB: expect :unicorn: :unicorn: :dragon: :dragon: and :bug: :bug:
    </td>
    <td>
      <a href="https://ci.appveyor.com/project/gitextensions/gitextensions/branch/master/artifacts">[ Download ]</a><br />
      <a href="https://ci.appveyor.com/project/gitextensions/gitextensions/branch/master"><img alt="Build status" src="https://ci.appveyor.com/api/projects/status/yo5kw7sl6da8danr/branch/master?svg=true" style="max-width:100%;"></a> <a href="https://codecov.io/gh/gitextensions/gitextensions"><img alt="codecov.io" src="https://codecov.io/gh/gitextensions/gitextensions/branch/master/graph/badge.svg" style="max-width:100%;"></a>
    </td>
  </tr>
  <tr>
    <td>
      <strong>Visual Studio VSIX (2015/2017/2019)</strong>
    </td>
    <td>
      <a href="https://marketplace.visualstudio.com/items?itemName=HenkWesthuis.GitExtensions">[ Download ]</a> or install from Visual Studio via Extensions
    </td>
  </tr>
  <tr>
    <td>
      <strong>Visual Studio addin (2010/2012/2013)</strong>
    </td>
    <td>
      Included with installer for the 2.x branch. <a href="https://github.com/gitextensions/gitextensions/releases/tag/v2.51.05">Download and run setup.exe</a><br />
      <ul>
        <li><a href="https://github.com/gitextensions/gitextensions/files/2646204/Visual.Studio.2010.Addin.zip">Visual Studio 2010 Addin.zip</a></li>
        <li><a href="https://github.com/gitextensions/gitextensions/files/2646205/Visual.Studio.2012.Addin.zip">Visual Studio 2012 Addin.zip</a></li>
        <li><a href="https://github.com/gitextensions/gitextensions/files/2646203/Visual.Studio.2013.Addin.zip">Visual Studio 2013 Addin.zip</a></li>
      </ul>
    </td>
  </tr>
  <tr>
    <td>
      <strong>Visual Studio Code VSIX</strong><br />
      Kudos to <a href="https://github.com/pmiossec" class="author text-inherit">@pmiossec</a>
    </td>
    <td>
      <a href="https://marketplace.visualstudio.com/items?itemName=pmiossec.vscode-gitextensions">[ Download ]</a> or install via VSCode<br />
      NB: Please direct all discussions about the VSIX to <a href="https://github.com/pmiossec/vscode-gitextensions">its own repo</a>.
    </td>
  </tr>
  <tr>
    <td>
      <strong>IntelliJ platform IDEs</strong><br />
      Kudos to <a href="https://github.com/DmitryZhelnin" class="author text-inherit">@DmitryZhelnin</a>
    </td>
    <td>
      <a href="https://plugins.jetbrains.com/plugin/11511-gitextensions">[ Download ]</a> or install via IDE Plugins settings<br />
      NB: Please direct all discussions about this plugin to <a href="https://github.com/DmitryZhelnin/git-extensions-intellij">its own repo</a>.
    </td>
  </tr>
  <tr>
    <td>
      <strong>Git Extensions for VS Code</strong><br />
      Kudos to <a href="https://github.com/Carl-Hugo" class="author text-inherit">@Carl-Hugo</a>
    </td>
    <td>
        <p>This extension allows users to <strong>Browse with Git Extensions</strong> from the <em>Explorer</em> and the <em>Editor</em>. It supports a single folder and workspaces. Nothing fancier.</p>
        <p><a href="https://marketplace.visualstudio.com/items?itemName=forevolve.git-extensions-for-vs-code">[ Download ]</a> from the Marketplace or install via VS Code<br />
        NB: Please direct all discussions about this extension to <a href="https://github.com/ForEvolve/git-extensions-for-vs-code">its own repo</a>.</p>
    </td>
  </tr>
</table>

# Conduct

Project maintainers pledge to foster an open and welcoming environment, and ask contributors to do the same.

For more information see our [code of conduct](CODE_OF_CONDUCT.md).

# Shoutouts

* We thank all the people who contribute, the project exists because of you<br />
  <a href="https://github.com/gitextensions/gitextensions/contributors"><img src="https://opencollective.com/gitextensions/contributors.svg?width=890&button=false" /></a>
* We thank JetBRAINS for [ReSharper](https://www.jetbrains.com/resharper/) licenses<br />
  <a href="https://www.jetbrains.com/?from=gitextensions" target="_blank"><img src="./GitUI/Resources/Icons/originals/jetbrains-variant-4.png"></a>
* We thank [SemanticMerge](https://www.semanticmerge.com) for licenses<br />
  <a href="https://www.semanticmerge.com" target="_blank"><img src="./GitUI/Resources/Icons/originals/semanticmerge-logo.svg"></a>
* We thank [SignPath.io](https://signpath.io/) for the free code signing<br />
  <a href="https://signpath.io/" target="_blank"><img src="./GitUI/Resources/Icons/originals/signpath_logo.png"></a>
* We thank [SignPath Foundation](https://signpath.org/) for the signing certificate
* We thank [Yusuke Kamiyamane](http://p.yusukekamiyamane.com/) for the icons ([CCA/3.0](http://creativecommons.org/licenses/by/3.0/))

## Backers

Thank you to all our backers! 🙏 [[Become a backer](https://opencollective.com/gitextensions#backer)]

<a href="https://opencollective.com/gitextensions#backers" target="_blank"><img src="https://opencollective.com/gitextensions/backers.svg?width=890"></a>

## Sponsors

Support this project by becoming a sponsor. Your logo will show up here with a link to your website. [[Become a sponsor](https://opencollective.com/gitextensions#sponsor)]

<a href="https://opencollective.com/gitextensions/sponsor/0/website" target="_blank"><img src="https://opencollective.com/gitextensions/sponsor/0/avatar.svg"></a>
<a href="https://opencollective.com/gitextensions/sponsor/1/website" target="_blank"><img src="https://opencollective.com/gitextensions/sponsor/1/avatar.svg"></a>
<a href="https://opencollective.com/gitextensions/sponsor/2/website" target="_blank"><img src="https://opencollective.com/gitextensions/sponsor/2/avatar.svg"></a>
<a href="https://opencollective.com/gitextensions/sponsor/3/website" target="_blank"><img src="https://opencollective.com/gitextensions/sponsor/3/avatar.svg"></a>
<a href="https://opencollective.com/gitextensions/sponsor/4/website" target="_blank"><img src="https://opencollective.com/gitextensions/sponsor/4/avatar.svg"></a>
<a href="https://opencollective.com/gitextensions/sponsor/5/website" target="_blank"><img src="https://opencollective.com/gitextensions/sponsor/5/avatar.svg"></a>
<a href="https://opencollective.com/gitextensions/sponsor/6/website" target="_blank"><img src="https://opencollective.com/gitextensions/sponsor/6/avatar.svg"></a>
<a href="https://opencollective.com/gitextensions/sponsor/7/website" target="_blank"><img src="https://opencollective.com/gitextensions/sponsor/7/avatar.svg"></a>
<a href="https://opencollective.com/gitextensions/sponsor/8/website" target="_blank"><img src="https://opencollective.com/gitextensions/sponsor/8/avatar.svg"></a>
<a href="https://opencollective.com/gitextensions/sponsor/9/website" target="_blank"><img src="https://opencollective.com/gitextensions/sponsor/9/avatar.svg"></a>


# Useful Links

* Website: [gitextensions.github.io](https://gitextensions.github.io/) [Git repo](https://github.com/gitextensions/gitextensions.github.io)
* Source code: [github.com/gitextensions/gitextensions](https://github.com/gitextensions/gitextensions)
* Online manual: [git-extensions-documentation.readthedocs.org](https://git-extensions-documentation.readthedocs.org/) [Git repo](https://github.com/gitextensions/GitExtensionsDoc)
* Issue tracker: [github.com/gitextensions/gitextensions/issues](https://github.com/gitextensions/gitextensions/issues)
* Wiki: [github.com/gitextensions/gitextensions/wiki](https://github.com/gitextensions/gitextensions/wiki)
* Gitter chat: [gitter.im/gitextensions/gitextensions](https://gitter.im/gitextensions/gitextensions?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)


# Release History (3.0.1)
Watch us work on this project.

[![Click to play](https://img.youtube.com/vi/k6DhHbzkZvM/0.jpg)](https://www.youtube.com/watch?v=k6DhHbzkZvM)

# Code Quality Monitor

[![Code quality status](https://codescene.io/projects/5326/status.svg) Full analysis results at **codescene.io**.](https://codescene.io/projects/5326/jobs/latest-successful/results)
