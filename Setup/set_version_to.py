#!/usr/bin/env python
# -*- coding: utf-8 -*-

"""Update version in GitExtension source files
"""

import argparse, sys
import re

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('-v', '--version',
                       help='numeric product version')
    parser.add_argument('-t', '--text', default=None,
                       help='text product version')
    args = parser.parse_args()
    
    if not args.version:
        parser.print_help()
        exit(1)

    if not args.text:
      args.text = args.version
    
    filename = "..\CommonAssemblyInfo.cs"
    commonAssemblyInfo = open(filename, "r").readlines()
    for i in range(len(commonAssemblyInfo)):
        line = commonAssemblyInfo[i]
        if line.find("[assembly: Assembly") != -1:
            if line.find("AssemblyVersion(") != -1 or line.find("AssemblyFileVersion(") != -1:
                data = line.split('"')
                data[1] = args.version
                commonAssemblyInfo[i] = '"'.join(data)
            if line.find("AssemblyInformationalVersion(") != -1:
                data = line.split('"')
                data[1] = args.text
                commonAssemblyInfo[i] = '"'.join(data)
    outfile = open(filename, "w")
    outfile.writelines(commonAssemblyInfo)
    
    filename = "..\GitExtensionsShellEx\GitExtensionsShellEx.rc"
    gitExtensionsShellEx = open(filename, "r").readlines()
    verData = ["0"] * 4
    verSplitted = args.version.split('.')
    for i in range(len(verSplitted)):
        verData[i] = verSplitted[i]
    for i in range(len(gitExtensionsShellEx)):
        line = gitExtensionsShellEx[i]
        if line.find("FILEVERSION") != -1:
            data = line.split(' ')
            data[2] = ','.join(verData) + '\n'
            gitExtensionsShellEx[i] = ' '.join(data)
        elif line.find("PRODUCTVERSION") != -1:
            data = line.split(' ')
            data[2] = ','.join(verData) + '\n'
            gitExtensionsShellEx[i] = ' '.join(data)
        elif line.find('"FileVersion"') != -1:
            data = line.split(', ', 1)
            data[1] = '"' + '.'.join(verSplitted) + '"\n'
            gitExtensionsShellEx[i] = ', '.join(data)
        elif line.find('"ProductVersion"') != -1:
            data = line.split(', ', 1)
            data[1] = '"' + args.text + '"\n'
            gitExtensionsShellEx[i] = ', '.join(data)
    outfile = open(filename, "w")
    outfile.writelines(gitExtensionsShellEx)
    
    filename = "..\GitExtSshAskPass\SshAskPass.rc2"
    gitExtSshAskPass = open(filename, "r").readlines()
    verData = ["0"] * 4
    verSplitted = args.version.split('.')
    for i in range(len(verSplitted)):
        verData[i] = verSplitted[i]
    for i in range(len(gitExtSshAskPass)):
        line = gitExtSshAskPass[i]
        if line.find("FILEVERSION") != -1:
            data = line.split(' ')
            data[2] = ','.join(verData) + '\n'
            gitExtSshAskPass[i] = ' '.join(data)
        elif line.find("PRODUCTVERSION") != -1:
            data = line.split(' ')
            data[2] = ','.join(verData) + '\n'
            gitExtSshAskPass[i] = ' '.join(data)
        elif line.find('"FileVersion"') != -1:
            data = line.split(', ', 1)
            data[1] = '"' + '.'.join(verSplitted) + '"\n'
            gitExtSshAskPass[i] = ', '.join(data)
        elif line.find('"ProductVersion"') != -1:
            data = line.split(', ', 1)
            data[1] = '"' + args.text + '"\n'
            gitExtSshAskPass[i] = ', '.join(data)
    outfile = open(filename, "w")
    outfile.writelines(gitExtSshAskPass)

    for i in range(1, len(verSplitted)):
        if len(verSplitted[i]) == 1:
            verSplitted[i] = "0" + verSplitted[i]
    
    filename = "MakeInstallers.cmd"
    makeInstallers = open(filename, "r").readlines()
    for i in range(len(makeInstallers)):
        line = makeInstallers[i]
        if line.find("set numericVersion=") == 0:
            data = line.split('=')
            data[1] = '.'.join(verSplitted) + '\n'
            makeInstallers[i] = '='.join(data)
        if line.find("set version=") == 0:
            data = line.split('=')
            data[1] = args.text + '\n'
            makeInstallers[i] = '='.join(data)
    outfile = open(filename, "w")
    outfile.writelines(makeInstallers)
    
    filename = "..\GitExtensionsDoc\source\conf.py"
    docoConf = open(filename, "r").readlines()
    for i in range(len(docoConf)):
        line = docoConf[i]
        if line.find("release = ") != -1:
            data = line.split(' = ')
            data[1] = '.'.join(verSplitted)
            docoConf[i] = " = '".join(data) + "'\n"
        if line.find("version = ") != -1:
            data = line.split(' = ')
            data[1] = args.text
            docoConf[i] = " = '".join(data) + "'\n"
    outfile = open(filename, "w")
    outfile.writelines(docoConf)
    
    filename = "..\GitExtensionsVSIX\source.extension.vsixmanifest"
    vsixManifest = open(filename, "r").readlines()
    for i in range(len(vsixManifest)):
        line = vsixManifest[i]
        if line.find("<Identity Publisher=\"GitExt Team\" Version=") != -1:
            line = re.sub("<Identity Publisher=\"GitExt Team\" Version=\"[0-9\.]+", "<Identity Publisher=\"GitExt Team\" Version=\"" + '.'.join(verSplitted), line)
            vsixManifest[i] = line
    outfile = open(filename, "w")
    outfile.writelines(vsixManifest)    