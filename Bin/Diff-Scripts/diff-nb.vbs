'
' TortoiseSVN Diff script for Mathematica notebooks
'
' Last commit by:
' $Author$
' $Date$
' $Rev$
'
' Authors:
' Szabolcs Horvát, 2008
' Chris Rodgers http://rodgers.org.uk/, 2008
' (Based on diff-xlsx.vbs)
'

dim objArgs, objScript, objDiffNotebook

Set objArgs = WScript.Arguments
num = objArgs.Count
if num < 2 then
   MsgBox "Usage: [CScript | WScript] compare.vbs base.nb new.nb", vbExclamation, "Invalid arguments"
   WScript.Quit 1
end if

sBaseDoc = objArgs(0)
sNewDoc = objArgs(1)

Set objScript = CreateObject("Scripting.FileSystemObject")

If objScript.FileExists(sBaseDoc) = False Then
    MsgBox "File " + sBaseDoc +" does not exist.  Cannot compare the notebooks.", vbExclamation, "File not found"
    Wscript.Quit 1
Else
    sBaseDoc = objScript.GetAbsolutePathName(sBaseDoc)
End If

If objScript.FileExists(sNewDoc) = False Then
    MsgBox "File " + sNewDoc +" does not exist.  Cannot compare the notebooks.", vbExclamation, "File not found"
    Wscript.Quit 1
Else
    sNewDoc = objScript.GetAbsolutePathName(sNewDoc)
End If

On Error Resume Next
Dim tfolder, tname, tfile
Const TemporaryFolder = 2

Set tfolder = objScript.GetSpecialFolder(TemporaryFolder)

tname = objScript.GetTempName + ".nb"
Set objDiffNotebook = tfolder.CreateTextFile(tname)

'Output a Mathematica notebook that will do the diff for us
objDiffNotebook.WriteLine "Notebook[{" + vbCrLf + _
"Cell[BoxData[ButtonBox[""\<\""Compare Notebooks\""\>""," + vbCrLf + _
"ButtonFrame->""DialogBox"", Active->True, ButtonEvaluator->Automatic," + vbCrLf + _
"ButtonFunction:>(Needs[""AuthorTools`""];" + vbCrLf + _
"NotebookPut[Symbol[""NotebookDiff""][" + vbCrLf + _
"""" + Replace(sBaseDoc,"\","\\") + """," + vbCrLf + _
"""" + Replace(sNewDoc,"\","\\") + """" + vbCrLf + _
"]])]], NotebookDefault]" + vbCrLf + _
"}, Saveable->False, Editable->False, WindowToolbars->{}, WindowFrame->ModelessDialog, WindowElements->{}, WindowFrameElements->CloseBox, WindowTitle->""Diff"", ShowCellBracket->False, WindowSize->{Fit,Fit}]"


objDiffNotebook.Close

Set objShell = CreateObject("WScript.Shell")
objShell.Run tfolder + "\" + tname
