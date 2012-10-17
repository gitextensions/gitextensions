' extensions: xls;xlsx;xlsm;xlsb;xlam
'
' TortoiseSVN Diff script for Excel files
'
' Copyright (C) 2004-2008 the TortoiseSVN team
' This file is distributed under the same license as TortoiseSVN
'
' Last commit by:
' $Author: tortoisesvn $
' $Date: 2009-10-18 09:25:24 +0200 (So, 18. Okt 2009) $
' $Rev: 17472 $
'
' Authors:
' Michael Joras <michael@joras.net>, 2008
' Suraj Barkale, 2006
'

dim objExcelApp, objArgs, objScript, objBaseDoc, objNewDoc, objWorkSheet, i

Set objArgs = WScript.Arguments
num = objArgs.Count
if num < 2 then
   MsgBox "Usage: [CScript | WScript] compare.vbs base.doc new.doc", vbExclamation, "Invalid arguments"
   WScript.Quit 1
end if

sBaseDoc = objArgs(0)
sNewDoc = objArgs(1)

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
Set objExcelApp = Wscript.CreateObject("Excel.Application")
If Err.Number <> 0 Then
   Wscript.Echo "You must have Excel installed to perform this operation."
   Wscript.Quit 1
End If

'Open base excel sheet
objExcelApp.Workbooks.Open sBaseDoc
'Open new excel sheet
objExcelApp.Workbooks.Open sNewDoc
'Show Excel window
objExcelApp.Visible = True
'Create a compare side by side view
objExcelApp.Windows.CompareSideBySideWith(objExcelApp.Windows(2).Caption)
If Err.Number <> 0 Then
	objExcelApp.Application.WindowState = xlMaximized
	objExcelApp.Windows.Arrange(-4128)
End If

'Mark differences in sNewDoc red
i = 1

For Each objWorkSheet In objExcelApp.Workbooks(2).Worksheets

		objworksheet.Cells.FormatConditions.Delete

		objExcelApp.Workbooks(1).Sheets(i).Copy ,objExcelApp.Workbooks(2).Sheets(objExcelApp.Workbooks(2).Sheets.Count)

		objExcelApp.Workbooks(2).Sheets(objExcelApp.Workbooks(2).Sheets.Count).Name = "Dummy_for_Comparison" & i

		objworksheet.Activate
		'To create a local formula the cell A1 is used
		original_content = objworksheet.Cells(1,1).Formula
		String sFormula
		'objworksheet.Cells(1,1).Formula = "=INDIRECT(""" & objExcelApp.Workbooks(2).Sheets(i).name & " (2)"& "!""&ADDRESS(ROW(),COLUMN()))"
		objworksheet.Cells(1,1).Formula = "=INDIRECT(""Dummy_for_Comparison" & i & "!""&ADDRESS(ROW(),COLUMN()))" 
		sFormula = objworksheet.Cells(1,1).FormulaLocal

		objworksheet.Cells(1,1).Formula = original_content
		'with the local formula the conditional formatting is used to mark the cells that are different
		const xlCellValue = 1
		const xlNotEqual = 4
		objworksheet.Cells.FormatConditions.Add xlCellValue, xlNotEqual, sFormula
		objworksheet.Cells.FormatConditions(1).Interior.ColorIndex = 3

	i = i + 1 
next
