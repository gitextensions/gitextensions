using System.Collections.Generic;
using FluentAssertions;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class CustomDiffMergeToolCacheTests
    {
        [TestCase("'git difftool --tool=<tool>' may be set to one of the following:\n\t\tvimdiff\n\t\tvimdiff2\n\t\tvimdiff3\n\t\twinmerge\n\n\tuser-defined:\n\t\tDiffMerge.cmd \"C:/Program Files/SourceGear/Common/DiffMerge/sgdm.exe\" -merge -result=\"$MERGED\" \"$LOCAL\" \"$BASE\" \"$REMOTE\"\n\t\tdiffmerge.cmd \"C:/Program Files/SourceGear/Common/DiffMerge/sgdm.exe\" \"$LOCAL\" \"$REMOTE\"\n\t\tkdiff3.cmd \"C:/Program Files/KDiff3/kdiff3.exe\" \"$LOCAL\" \"$REMOTE\"\n\t\tmeld.cmd \"C:/Program Files (x86)/Meld/meld.exe\" \"$LOCAL\" \"$BASE\" \"$REMOTE\" --output \"$MERGED\"\n\t\tmeld.cmd \"C:/Program Files (x86)/Meld/meld.exe\" \"$LOCAL\" \"$REMOTE\"\n\t\tp4merge.cmd \"C:/Program Files/Perforce/p4merge.exe\" \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"\n\t\tp4merge.cmd \"C:/Program Files/Perforce/p4merge.exe\" \"$LOCAL\" \"$REMOTE\"\n\t\tsemanticdiff.cmd \"C:/Users/ejgo/AppData/Local/semanticmerge/semanticmergetool.exe\" -s \"$LOCAL\" -d \"$REMOTE\"\n\t\tsemanticmerge.cmd \"C:/Users/ejgo/AppData/Local/semanticmerge/semanticmergetool.exe\" -s \"$LOCAL\" -d \"$REMOTE\"\n\t\tsemanticmerge.cmd \"C:/Users/ejgo/AppData/Local/semanticmerge/semanticmergetool.exe\" -s \"$REMOTE\" -d \"$LOCAL\" -b \"$BASE\" -r \"$MERGED\"\n\t\tsourcetree.cmd 'C:/Program Files/TortoiseGit/bin/TortoiseGitMerge.exe'  -base:\"$BASE\" -mine:\"$LOCAL\" -theirs:\"$REMOTE\" -merged:\"$MERGED\"\n\t\tsourcetree.cmd 'C:/Program Files/TortoiseGit/bin/TortoiseGitMerge.exe' \"$LOCAL\" \"$REMOTE\"\n\t\tssss.cmd ggfjf dhdf df\n\t\ttortoisemerge.cmd \"C:/Program Files/TortoiseGit/bin/TortoiseGitMerge.exe\" -base:\"$BASE\" -mine:\"$LOCAL\"\n\t\ttortoisemerge.cmd \"C:/Program Files/TortoiseGit/bin/TortoiseGitMerge.exe\" -base:\"$BASE\" -mine:\"$LOCAL\" -theirs:\"$REMOTE\" -merged:\"$MERGED\"\n\t\ttortoisemerge2.cmd \"C:/Program Files/TortoiseGit/bin/TortoiseGitMerge.exe\" \"$LOCAL\" \"$REMOTE\"\n\t\tvsdiffmerge.cmd \"F:/Program Files (x86)/Microsoft Visual Studio/2017/Community/Common7/IDE/CommonExtensions/Microsoft/TeamFoundation/Team Explorer/vsDiffMerge.exe\" \"$LOCAL\" \"$REMOTE\"\n\t\twinmerge.cmd \"C:/Program Files/WinMerge/winmergeu.exe\" -e -u  -wl -wr -fm -dl \"Mine: $LOCAL\" -dm \"Merged: $BASE\" -dr \"Theirs: $REMOTE\" \"$LOCAL\" \"$BASE\" \"$REMOTE\" -o \"$MERGED\"\n\t\twinmerge.cmd \"C:/Program Files/WinMerge/winmergeu.exe\" -e -u \"$LOCAL\" \"$REMOTE\"\n\nThe following tools are valid, but not currently available:\n\t\taraxis\n\t\tbc\n\t\tbc3\n\t\tcodecompare\n\t\tdeltawalker\n\t\tdiffmerge\n\t\tdiffuse\n\t\tecmerge\n\t\temerge\n\t\texamdiff\n\t\tguiffy\n\t\tgvimdiff\n\t\tgvimdiff2\n\t\tgvimdiff3\n\t\tkdiff3\n\t\tkompare\n\t\tmeld\n\t\topendiff\n\t\tp4merge\n\t\tsmerge\n\t\ttkdiff\n\t\txxdiff\n\nSome of the tools listed above only work in a windowed\nenvironment. If run in a terminal-only session, they will fail.\n",
            new[] { "winmerge", "diffmerge", "DiffMerge", "kdiff3", "meld", "p4merge", "semanticdiff", "semanticmerge", "sourcetree", "ssss", "tortoisemerge", "tortoisemerge2", "vsdiffmerge" })]
        [TestCase("'git difftool --tool=<tool>' may be set to one of the following:\n\t\tvimdiff\n\t\tvimdiff2\n\t\tvimdiff3\n\t\twinmerge\n\nThe following tools are valid, but not currently available:\n\t\taraxis\n\t\tbc\n\t\tbc3\n\t\tcodecompare\n\t\tdeltawalker\n\t\tdiffmerge\n\t\tdiffuse\n\t\tecmerge\n\t\temerge\n\t\texamdiff\n\t\tguiffy\n\t\tgvimdiff\n\t\tgvimdiff2\n\t\tgvimdiff3\n\t\tkdiff3\n\t\tkompare\n\t\tmeld\n\t\topendiff\n\t\tp4merge\n\t\tsmerge\n\t\ttkdiff\n\t\txxdiff\n\nSome of the tools listed above only work in a windowed\nenvironment. If run in a terminal-only session, they will fail.\n",
            new[] { "winmerge" })]
        [TestCase("'git difftool --tool=<tool>' may be set to one of the following:\n\t\tvimdiff\n\t\tvimdiff2\n\t\tvimdiff3\n\t\twinmerge\n",
            new[] { "winmerge" })]

        public void ParseCustomDiffMergeToolTest(string output, string[] expected)
        {
            IEnumerable<string> tools = CustomDiffMergeToolCache.DiffToolCache.GetTestAccessor().ParseCustomDiffMergeTool(output, "winmerge");

            tools.Should().BeEquivalentTo(expected);
        }
    }
}
