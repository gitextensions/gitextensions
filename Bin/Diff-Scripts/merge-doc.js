// extensions: doc;docx;docm
//
// TortoiseSVN Merge script for Word Doc files
//
// Copyright (C) 2004-2008, 2011-2016, 2019-2020 the TortoiseSVN team
// This file is distributed under the same license as TortoiseSVN
//
// Last commit by:
// $Author$
// $Date$
// $Rev$
//
// Authors:
// Dan Sheridan, 2008
// Davide Orlandi and Hans-Emil Skogh, 2005
// Richard Horton, 2011
//
// Modified (removing TSVN in output) by Henk Westhuis for Git Extensions (2010)
//

var objArgs, num, sTheirDoc, sMyDoc, sBaseDoc, sMergedDoc,
    objScript, word, baseDoc, myDoc, WSHShell;

// ----- constants -----
//var vbCritical = 0x10;
var vbExclamation = 0x30;
//var vbInformation = 0x40;

// Microsoft Office versions for Microsoft Windows OS
var vOffice2000 = 9;
var vOffice2002 = 10;
//var vOffice2003 = 11;
var vOffice2007 = 12;
var vOffice2010 = 14;
// WdCompareTarget
var wdCompareTargetSelected = 0;
//var wdCompareTargetCurrent = 1;
var wdCompareTargetNew = 2;
var wdMergeTargetCurrent = 1;

objArgs = WScript.Arguments;
num = objArgs.length;
if (num < 4)
{
    WScript.Echo("Usage: [CScript | WScript] merge-doc.js merged.doc theirs.doc mine.doc base.doc");
    WScript.Quit(1);
}

sMergedDoc = objArgs(0);
sTheirDoc = objArgs(1);
sMyDoc = objArgs(2);
sBaseDoc = objArgs(3);

objScript = new ActiveXObject("Scripting.FileSystemObject");

if (!objScript.FileExists(sTheirDoc))
{
    WScript.Echo("File " + sTheirDoc + " does not exist. Cannot compare the documents.", vbExclamation, "File not found");
    WScript.Quit(1);
}

if (!objScript.FileExists(sMergedDoc))
{
    WScript.Echo("File " + sMergedDoc + " does not exist. Cannot compare the documents.", vbExclamation, "File not found");
    WScript.Quit(1);
}

objScript = null;

try
{
    word = WScript.CreateObject("Word.Application");
    // disable macros
    word.AutomationSecurity = 3; //msoAutomationSecurityForceDisable
}
catch (e)
{
    WScript.Echo("You must have Microsoft Word installed to perform this operation.");
    WScript.Quit(1);
}

word.visible = true;

// Open the base document
baseDoc = word.Documents.Open(sTheirDoc);

// Merge into the "My" document
if (parseInt(word.Version, 10) < vOffice2000)
{
    baseDoc.Compare(sMergedDoc);
}
else if (parseInt(word.Version, 10) < vOffice2007)
{
    baseDoc.Compare(sMergedDoc, "Comparison", wdCompareTargetNew, true, true);
}
else if (parseInt(word.Version, 10) < vOffice2010)
{
    baseDoc.Merge(sMergedDoc);
}
else
{
    // 2010 - handle slightly differently as the basic merge isn't that good.
    // Note this is designed specifically for SVN 3 way merges,
    // during the commit conflict resolution process
    baseDoc = word.Documents.Open(sBaseDoc);
    myDoc = word.Documents.Open(sMyDoc);

    baseDoc.Activate(); // required otherwise it compares the wrong docs !!!
    baseDoc.Compare(sTheirDoc, "theirs", wdCompareTargetSelected, true, true);

    baseDoc.Activate(); // required otherwise it compares the wrong docs !!!
    baseDoc.Compare(sMyDoc, "mine", wdCompareTargetSelected, true, true);

    myDoc.Activate(); // required? just in case
    myDoc.Merge(sTheirDoc, wdMergeTargetCurrent);
}

// Show the merge result
if (parseInt(word.Version, 10) < vOffice2007)
{
    word.ActiveDocument.Windows(1).Visible = 1;
}

// Close the first document
if (parseInt(word.Version, 10) >= vOffice2002 && parseInt(word.Version, 10) < vOffice2010)
{
    baseDoc.Close();
}

// Show usage hint message
WSHShell = WScript.CreateObject("WScript.Shell");
if (WSHShell.Popup("You have to accept or reject the changes before\nsaving the document to prevent future problems.\n\nWould you like to see a help page on how to do this?", 0, "Word Merge", 4 + 64) === 6)
{
    WSHShell.Run("http://office.microsoft.com/en-us/assistance/HP030823691033.aspx");
}
