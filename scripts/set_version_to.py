#!/usr/bin/env python
# -*- coding: utf-8 -*-

"""Update version in GitExtension source files
"""

import argparse, sys
import glob
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

    m = re.match("(\d+\.\d+)", args.version)
    if m:
        short_version = m.group(1)
    else:
        parser.print_help()
        exit(1)

    verData = ["0"] * 4
    verSplitted = args.version.split('.')
    for i in range(len(verSplitted)):
        verData[i] = verSplitted[i]

    if not args.text:
      args.text = args.version
    
    submodules = glob.glob("..\Externals\**\AssemblyInfo.cs", recursive=True)
    filenames = [ "..\CommonAssemblyInfo.cs", "..\CommonAssemblyInfoExternals.cs" ]
    combined = filenames + submodules
    for filename in combined:
        print (filename)
        commonAssemblyInfo = open(filename, "r").readlines()
        o = ""
        for i in commonAssemblyInfo:
            line = i
            if line.find("[assembly: Assembly") != -1:
                if line.find("AssemblyVersion(") != -1 or line.find("AssemblyFileVersion(") != -1:
                    data = line.split('"')
                    data[1] = args.version
                    line = '"'.join(data)
                elif line.find("AssemblyInformationalVersion(") != -1:
                    data = line.split('"')
                    data[1] = args.text
                    line = '"'.join(data)
            o += line
        outfile = open(filename, "w", newline='\n')
        outfile.writelines(o)
    
    filename = "..\GitExtensionsShellEx\GitExtensionsShellEx.rc"
    gitExtensionsShellEx = open(filename, "r").readlines()
    o = ""
    for i in gitExtensionsShellEx:
        line = i
        if line.find("FILEVERSION") != -1:
            data = line.split(' ')
            data[2] = ','.join(verData) + '\n'
            line = ' '.join(data)
        elif line.find("PRODUCTVERSION") != -1:
            data = line.split(' ')
            data[2] = ','.join(verData) + '\n'
            line = ' '.join(data)
        elif line.find('"FileVersion"') != -1:
            data = line.split(', ', 1)
            data[1] = '"' + '.'.join(verSplitted) + '"\n'
            line = ', '.join(data)
        elif line.find('"ProductVersion"') != -1:
            data = line.split(', ', 1)
            data[1] = '"' + args.text + '"\n'
            line = ', '.join(data)
        o += line
    outfile = open(filename, "w", newline='\n')
    outfile.writelines(o)
    
    filename = "..\GitExtSshAskPass\SshAskPass.rc2"
    gitExtSshAskPass = open(filename, "r").readlines()
    o = ""
    for i in gitExtSshAskPass:
        line = i
        if line.find("FILEVERSION") != -1:
            data = line.split(' ')
            data[2] = ','.join(verData) + '\n'
            line = ' '.join(data)
        elif line.find("PRODUCTVERSION") != -1:
            data = line.split(' ')
            data[2] = ','.join(verData) + '\n'
            line = ' '.join(data)
        elif line.find('"FileVersion"') != -1:
            data = line.split(', ', 1)
            data[1] = '"' + '.'.join(verSplitted) + '"\n'
            line = ', '.join(data)
        elif line.find('"ProductVersion"') != -1:
            data = line.split(', ', 1)
            data[1] = '"' + args.text + '"\n'
            line = ', '.join(data)
        o += line
    outfile = open(filename, "w", newline='\n')
    outfile.writelines(o)

    for i in range(1, len(verSplitted)):
        if len(verSplitted[i]) == 1:
            verSplitted[i] = "0" + verSplitted[i]
