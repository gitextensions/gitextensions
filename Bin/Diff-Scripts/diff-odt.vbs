' extensions: odt;ods
'
' TortoiseSVN Diff script for Open Office Text files
'
' Copyright (C) 2004-2009 the TortoiseSVN team
' This file is distributed under the same license as TortoiseSVN
'
' Last commit by:
' $Author: tortoisesvn $
' $Date: 2009-10-18 09:25:24 +0200 (So, 18. Okt 2009) $
' $Rev: 17472 $
'
' Authors:
' Jonathan Ashley, 2007
' Stefan Küng, 2006, 2009
'
dim objArgs,num,sBaseDoc,sNewDoc,objScript,word,destination

Set objArgs = WScript.Arguments
num = objArgs.Count
if num < 2 then
   MsgBox "Usage: [CScript | WScript] diff-odt.vbs base.odt new.odt", vbExclamation, "Invalid arguments"
   WScript.Quit 1
end if

sBaseDoc=objArgs(0)
sNewDoc=objArgs(1)

Set objScript = CreateObject("Scripting.FileSystemObject")
If objScript.FileExists(sBaseDoc) = False Then
    MsgBox "File " + sBaseDoc +" does not exist.  Cannot compare the documents.", vbExclamation, "File not found"
    Wscript.Quit 1
End If
If objScript.FileExists(sNewDoc) = False Then
    MsgBox "File " + sNewDoc +" does not exist.  Cannot compare the documents.", vbExclamation, "File not found"
    Wscript.Quit 1
End If
'remove the file write protection
objScript.GetFile(sBaseDoc).Attributes = objScript.GetFile(sBaseDoc).Attributes And Not 1
objScript.GetFile(sNewDoc).Attributes = objScript.GetFile(sNewDoc).Attributes And Not 1

Set objScript = Nothing

On Error Resume Next
'The service manager is always the starting point
'If there is no office running then an office is started
Set objServiceManager= Wscript.CreateObject("com.sun.star.ServiceManager")
If Err.Number <> 0 Then
   Wscript.Echo "You must have OpenOffice installed to perform this operation."
   Wscript.Quit 1
End If

On Error Goto 0

'Because this is a diff, TortoiseSVN marks the files as read-only.
'However, OpenOffice will not compare any file with that flag set.
'Make sure we un-set that flag.
Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objFile = objFSO.GetFile(sNewDoc)
If (objFile.Attributes AND 1)=1 Then
    objFile.Attributes = objFile.Attributes XOR 1 
End If

'Create the DesktopSet 
Set objDesktop = objServiceManager.createInstance("com.sun.star.frame.Desktop")
Set objUriTranslator = objServiceManager.createInstance("com.sun.star.uri.ExternalUriReferenceTranslator")
'Adjust the paths for OO
sBaseDoc=Replace(sBaseDoc, "\", "/")
sBaseDoc=Replace(sBaseDoc, ":", "|")
sBaseDoc=Replace(sBaseDoc, "%", "%25")
sBaseDoc=Replace(sBaseDoc, " ", "%20")
sBaseDoc="file:///"&sBaseDoc
sBaseDoc=objUriTranslator.translateToInternal(sBaseDoc)
sNewDoc=Replace(sNewDoc, "\", "/")
sNewDoc=Replace(sNewDoc, ":", "|")
sNewDoc=Replace(sNewDoc, "%", "%25")
sNewDoc=Replace(sNewDoc, " ", "%20")
sNewDoc="file:///"&sNewDoc
sNewDoc=objUriTranslator.translateToInternal(sNewDoc)

'Open the %base document
Dim oPropertyValue(0)
Set oPropertyValue(0) = objServiceManager.Bridge_GetStruct("com.sun.star.beans.PropertyValue")
oPropertyValue(0).Name = "ShowTrackedChanges"
oPropertyValue(0).Value = true
Set objDocument=objDesktop.loadComponentFromURL(sNewDoc,"_blank", 0, oPropertyValue)

'Set the frame
Set Frame = objDesktop.getCurrentFrame

Set dispatcher=objServiceManager.CreateInstance("com.sun.star.frame.DispatchHelper")

'Execute the comparison
dispatcher.executeDispatch Frame, ".uno:ShowTrackedChanges", "", 0, oPropertyValue
oPropertyValue(0).Name = "URL"
oPropertyValue(0).Value = sBaseDoc
dispatcher.executeDispatch Frame, ".uno:CompareDocuments", "", 0, oPropertyValue



