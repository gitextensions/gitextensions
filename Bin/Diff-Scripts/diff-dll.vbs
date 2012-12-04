' extensions: dll;exe
'
' TortoiseSVN Diff script for binary files
'
' Copyright (C) 2010 the TortoiseSVN team
' This file is distributed under the same license as TortoiseSVN
'
' Last commit by:
' $Author: tortoisesvn $
' $Date: 2011-03-31 21:18:05 +0200 (Do, 31. Mrz 2011) $
' $Rev: 21089 $
'
' Authors:
' Casey Barton, 2010
' Hans-Emil Skogh, 2011
'
dim objArgs, objFileSystem, sBaseVer, sNewVer, sMessage, sBaseMessage, sNewMessage, bDiffers

bDiffers = False

Set objArgs = WScript.Arguments
num = objArgs.Count
if num < 2 then
   MsgBox "Usage: [CScript | WScript] compare.vbs base.doc new.doc", vbCritical, "Invalid arguments"
   WScript.Quit 1
end if

sBaseDoc = objArgs(0)
sNewDoc = objArgs(1)

Set objFileSystem = CreateObject("Scripting.FileSystemObject")
If objFileSystem.FileExists(sBaseDoc) = False Then
    MsgBox "File " + sBaseDoc +" does not exist.  Cannot compare the files.", vbCritical, "File not found"
    Wscript.Quit 1
End If
If objFileSystem.FileExists(sNewDoc) = False Then
    MsgBox "File " + sNewDoc +" does not exist.  Cannot compare the files.", vbCritical, "File not found"
    Wscript.Quit 1
End If

' Compare file size
dim fBaseFile, fNewFile
Set fBaseFile = objFileSystem.GetFile(sBaseDoc)
Set fNewFile = objFileSystem.GetFile(sNewDoc)

If fBaseFile.size <> fNewFile.size Then
	bDiffers = True
	sBaseMessage = sBaseMessage + "  Size: " + CStr(fBaseFile.Size) + " bytes" + vbCrLf
	sNewMessage = sNewMessage + "  Size: " + CStr(fNewFile.Size) + " bytes" + vbCrLf
Else
	sMessage = sMessage + "File sizes: " + CStr(fNewFile.Size) + " bytes" + vbCrLf
End If

' Compare files using fc.exe
If bDiffers = False Then
	Set WshShell = WScript.CreateObject("WScript.Shell")
	exitStatus = WshShell.Run("fc.exe "+sBaseDoc+" "+sNewDoc, 0, True)
	If exitStatus = 1 Then
		bDiffers = True
		sMessage = sMessage + "File content differs!" + vbCrLf
	ElseIf exitStatus > 1 Then
		' Todo: Handle error!
	End If
End If

' Only compare versions if we are comparing exe:s or dll:s
If LCase(Right(sBaseDoc, 3)) = "exe" or LCase(Right(sNewDoc, 3)) = "exe" or _
	LCase(Right(sBaseDoc, 3)) = "dll" or LCase(Right(sNewDoc, 3)) = "dll" Then

	' Compare version
	sBaseVer = objFileSystem.GetFileVersion(sBaseDoc)
	sNewVer = objFileSystem.GetFileVersion(sNewDoc)

	If Len(sBaseVer) = 0 and Len(sNewVer) = 0 Then
		  sMessage = sMessage + "No version information available."
	ElseIf sBaseVer = sNewVer Then
		sMessage = sMessage + "Version: " + sBaseVer
	Else
		sBaseMessage = sBaseMessage + "  Version: " + sBaseVer + vbCrLf
		sNewMessage = sNewMessage + "  Version: " + sNewVer + vbCrLf
	End If
End If

' Generate result message
sBaseMessage = "Base" + vbCrLf _
    + "  File: " + sBaseDoc + vbCrLf _
	+ sBaseMessage
sNewMessage = + "New" + vbCrLf _
    + "  File: " + sNewDoc + vbCrLf _
	+ sNewMessage

If bDiffers = True Then
    sMessage = "Files differ!" + vbCrLf _
		+ vbCrLf _
        + sBaseMessage + vbCrLf _
        + sNewMessage + vbCrLf _
		+ sMessage

	MsgBox sMessage, vbExclamation, "File Comparison - Differs"
Else
	sMessage = "Files are identical" + vbCrLf _
		+ vbCrLf _
		+ sMessage

	MsgBox sMessage, vbInformation, "File Comparison - Identical"
End If

Wscript.Quit
