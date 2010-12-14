'
' TortoiseSVN Diff script for Open Office Calc files
'
' Copyright (C) 2004-2008 the TortoiseSVN team
' This file is distributed under the same license as TortoiseSVN
'
' Last commit by:
' $Author: tortoisesvn $
' $Date: 2008-06-16 21:36:28 +0200 (Mo, 16 Jun 2008) $
' $Rev: 13285 $
'
' Authors:
' Jonathan Ashley, 2007
' Stefan Küng, 2006
'
dim objArgs,num,sBaseDoc,sNewDoc,objScript,word,destination

Set objArgs = WScript.Arguments
num = objArgs.Count
if num < 2 then
   MsgBox "Usage: [CScript | WScript] compare.vbs base.doc new.doc", vbExclamation, "Invalid arguments"
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
'Create the DesktopSet 
Set objDesktop = objServiceManager.createInstance("com.sun.star.frame.Desktop")
'Adjust the paths for OO
sBaseDoc=Replace(sBaseDoc, "\", "/")
sBaseDoc=Replace(sBaseDoc, ":", "|")
sBaseDoc=Replace(sBaseDoc, "%", "%25")
sBaseDoc=Replace(sBaseDoc, " ", "%20")
sBaseDoc="file:///"&sBaseDoc
sNewDoc=Replace(sNewDoc, "\", "/")
sNewDoc=Replace(sNewDoc, ":", "|")
sNewDoc=Replace(sNewDoc, "%", "%25")
sNewDoc=Replace(sNewDoc, " ", "%20")
sNewDoc="file:///"&sNewDoc

'Open the %base document
Dim oPropertyValue(0)
Set oPropertyValue(0) = objServiceManager.Bridge_GetStruct("com.sun.star.beans.PropertyValue")
oPropertyValue(0).Name = "ShowTrackedChanges"
oPropertyValue(0).Value = true
Set objDocument=objDesktop.loadComponentFromURL(sNewDoc,"_blank", 0, oPropertyValue)
Set objDocument2=objDesktop.loadComponentFromURL(sBaseDoc,"_blank",0, oPropertyValue)

