![Git Extensions logo](https://cdn.rawgit.com/gitextensions/gitextensions/master/setup/assets/Logo/git-extensions-logo.svg)

# Git Extensions

Git Extensions is a standalone UI tool for managing git repositories.
It also integrates with Windows Explorer and Microsoft Visual Studio (2015/2017/2019).

Have a question? Come and talk to us: [![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/gitextensions/gitextensions?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) or send us a tweet [![@git_extensions](https://img.shields.io/badge/twitter-%40git__extensions-blue)](https://twitter.com/git_extensions)

## Current Status

<a href="#backers" alt="sponsors on Open Collective"><img src="https://opencollective.com/gitextensions/backers/badge.svg" /></a> <a href="#sponsors" alt="Sponsors on Open Collective"><img src="https://opencollective.com/gitextensions/sponsors/badge.svg" /></a>

### Next Version ([build instructions](https://github.com/gitextensions/gitextensions/wiki/How-To%3A-build-instructions))

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
      MS Windows 7SP1+ // <a href="https://dotnet.microsoft.com/download/dotnet/8.0" target=_blank>.NET 8.0 SDK</a>
    </td>
  </tr>
  <tr>
    <td>
      Development
    </td>
    <td>
      MS Visual Studio 2022 (v17.8+), C# 12 // VC++ (inc. ATL for x86/x64 for installer)
    </td>
  </tr>
  <tr>
    <td>
      Current dev status
    </td>
    <td>
      <a href="https://ci.appveyor.com/project/gitextensions/gitextensions/branch/master"><img alt="Build status" src="https://ci.appveyor.com/api/projects/status/yo5kw7sl6da8danr/branch/master?svg=true" style="max-width:100%;"></a>
    </td>
  </tr>
  <tr>
    <td>
      Translations
    </td>
    <td>
      <a target="_blank" style="text-decoration:none; color:black; font-size:66%" href="https://github.com/gitextensions/gitextensions/wiki/Translations" title="More information in the wiki"><img src="https://img.shields.io/badge/tranlations-Transifex-blue" ></a>
    </td>
  </tr>
</table>

### Older versions 

See [build instructions](https://github.com/gitextensions/gitextensions/wiki/How-To%3A-build-instructions)


## Downloads

<a href="https://github.com/gitextensions/gitextensions/releases" rel="nofollow" style="vertical-align: -webkit-baseline-middle;"><img src="https://img.shields.io/github/downloads/gitextensions/gitextensions/total.svg?label=GitHub%20downloads%20(total)&cacheSeconds=86400"></a> <a href="https://sourceforge.net/projects/gitextensions/files" rel="nofollow" style="vertical-align: -webkit-baseline-middle;"><img src="https://img.shields.io/sourceforge/dt/gitextensions.svg?label=SourceForge%20downloads%20(total)&cacheSeconds=86400"></a> <a href="https://chocolatey.org/packages/gitextensions" rel="nofollow" style="vertical-align: -webkit-baseline-middle;"><img src="https://img.shields.io/chocolatey/dt/gitextensions.svg?label=Chocolatey%20downloads%20(total)&cacheSeconds=86400"></a>

**[Download it now](https://github.com/gitextensions/gitextensions/releases/latest)** or install it with [Chocolatey](https://chocolatey.org/packages/gitextensions) or [Winget](https://winget.run/pkg/GitExtensionsTeam/GitExtensions).

If you want to **update a portable version**, you should delete all the files and the subfolders from the existing folder except:

* _GitExtensions.settings_
* _WindowPositions.xml_
* User defined themes in folder _Themes_

<table>
  <tr>
    <td>
      <strong>Latest Release: v5.0</strong>
    </td>
    <td>
      <a href="https://github.com/gitextensions/gitextensions/releases/latest">[ Download ]</a><br />
      <a href="https://github.com/gitextensions/gitextensions/releases/latest" rel="nofollow" style="vertical-align: -webkit-baseline-middle;"><img src="https://img.shields.io/github/downloads/gitextensions/gitextensions/latest/total.svg?label=GitHub%20downloads%20(latest)&cacheSeconds=3600"></a>
    </td>
  </tr>
  <tr>
    <td>
      <strong>Current dev stream</strong><br />
      NB: expect :unicorn: :unicorn: and :dragon: :dragon:
    </td>
    <td>
      <a href="https://ci.appveyor.com/project/gitextensions/gitextensions/branch/master/artifacts">[ Download ]</a><br />
      <a href="https://ci.appveyor.com/project/gitextensions/gitextensions/branch/master"><img alt="Build status" src="https://ci.appveyor.com/api/projects/status/yo5kw7sl6da8danr/branch/master?svg=true" style="max-width:100%;"></a>
    </td>
  </tr>
  <tr>
    <td>
      <strong>Visual Studio VSIX (2022)</strong>
    </td>
    <td>
      <a href="https://marketplace.visualstudio.com/items?itemName=GitExtensionsApp.VS2022">[ Download ]</a> or install from Visual Studio via Extensions
    </td>
  </tr>
  <tr>
    <td>
      <strong>Visual Studio VSIX (2015/2017/2019)</strong>
    </td>
    <td>
      <a href="https://marketplace.visualstudio.com/items?itemName=GitExtensionsApp.v341">[ Download ]</a> or install from Visual Studio via Extensions
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
* We thank [Transifex](https://www.transifex.com/) for helping us with translations<br />
  <a href="https://www.transifex.com/" target="_blank"><img src="./src/app/GitUI/Resources/Icons/originals/transifex.svg"></a>
* We thank [SignPath.io](https://signpath.io/?utm_source=foundation&utm_medium=github&utm_campaign=gitextension) for the free code signing<br />
  <a href="https://signpath.io/?utm_source=foundation&utm_medium=github&utm_campaign=gitextension" target="_blank"><img src="./src/app/GitUI/Resources/Icons/originals/signpath_logo.png"></a>
* We thank [SignPath Foundation](https://signpath.org/?utm_source=foundation&utm_medium=github&utm_campaign=gitextension) for the signing certificate
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
