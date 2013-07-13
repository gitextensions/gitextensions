import argparse, sys

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('-v', '--version',
                       help='product version')
    args = parser.parse_args()
    
    if not args.version:    
        parser.print_help()
        exit(1)
    
    filename = "..\CommonAssemblyInfo.cs"
    commonAssemblyInfo = open(filename, "r").readlines()
    for i in range(len(commonAssemblyInfo)):
        line = commonAssemblyInfo[i]
        if line.find("[assembly: Assembly") != -1 and line.find("Version(") != -1:
           data = line.split('"')
           data[1] = args.version
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
           data[1] = '"' + '.'.join(verSplitted) + '"\n'
           gitExtensionsShellEx[i] = ', '.join(data)
    outfile = open(filename, "w")
    outfile.writelines(gitExtensionsShellEx)
    
    filename = "MakeInstallers.bat"
    makeInstallers = open(filename, "r").readlines()
    for i in range(1, len(verSplitted)):
        if len(verSplitted[i]) == 1:
            verSplitted[i] = "0" + verSplitted[i]
    for i in range(len(makeInstallers)):
        line = makeInstallers[i]
        if line.find("set version=") != -1:
           data = line.split('=')
           data[1] = '.'.join(verSplitted) + '\n'
           makeInstallers[i] = '='.join(data)
    outfile = open(filename, "w")
    outfile.writelines(makeInstallers)
    
    filename = "BuildInstallers.Mono.cmd"
    makeInstallers = open(filename, "r").readlines()
    for i in range(1, len(verSplitted)):
        if len(verSplitted[i]) == 1:
            verSplitted[i] = "0" + verSplitted[i]
    for i in range(len(makeInstallers)):
        line = makeInstallers[i]
        if line.find("set version=") != -1:
           data = line.split('=')
           data[1] = '.'.join(verSplitted) + '\n'
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
           data[1] = '.'.join([verSplitted[0], verSplitted[1]])
           docoConf[i] = " = '".join(data) + "'\n"
    outfile = open(filename, "w")
    outfile.writelines(docoConf)
