//
// TortoiseSVN Merge script for Word Doc files
//
// Copyright (C) 2004-2008 the TortoiseSVN team
// This file is distributed under the same license as TortoiseSVN
//
// Authors:
// Dan Sheridan, 2008
// Davide Orlandi and Hans-Emil Skogh, 2005
//
// Modified by Henk Westhuis for Git Extensions (2010)
//

var objArgs,num,sTheirDoc,sMyDoc,sBaseDoc,sMergedDoc,objScript,word,baseDoc,WSHShell;

var vbExclamation = "!"; //+pa

objArgs = WScript.Arguments;
num = objArgs.length;
if (num < 4)
{
   WScript.Echo("Usage: [CScript | WScript] merge-doc.js merged.doc theirs.doc mine.doc base.doc");
   WScript.Quit(1);
}

sMergedDoc=objArgs(0);
sTheirDoc=objArgs(1);
sMyDoc=objArgs(2);   //?? unused??
sBaseDoc=objArgs(3);

objScript = new ActiveXObject("Scripting.FileSystemObject")
if ( ! objScript.FileExists(sTheirDoc))
{
    WScript.Echo("File " + sTheirDoc +" does not exist.  Cannot compare the documents.", vbExclamation, "File not found");
    WScript.Quit(1);
}
if ( ! objScript.FileExists(sMergedDoc))
{
    WScript.Echo("File " + sMergedDoc +" does not exist.  Cannot compare the documents.", vbExclamation, "File not found");
    WScript.Quit(1);
}

objScript = null

try
{
	word = WScript.CreateObject("Word.Application");
}
catch(e)
{
   WScript.Echo("You must have Microsoft Word installed to perform this operation.");
   WScript.Quit(1);
}

word.visible = true

// Open the base document
baseDoc = word.Documents.Open(sTheirDoc); //?? base or their??

// Merge into the "My" document
if (parseInt(word.Version) < 12)
{
	baseDoc.Compare(sMergedDoc);
} else {
	baseDoc.Merge(sMergedDoc);
}

// Show the merge result
if (parseInt(word.Version) < 12)
{
	word.ActiveDocument.Windows(1).Visible = 1;
}

// Close the first document
if (parseInt(word.Version) >= 10)
{
   baseDoc.Close();
}

// Show usage hint message
WSHShell = WScript.CreateObject("WScript.Shell");
if(WSHShell.Popup("You have to accept or reject the changes before\nsaving the document to prevent future problems.\n\nWould you like to see a help page on how to do this?", 0, "Word Merge", 4 + 64) == 6)
{
    WSHShell.Run("http://office.microsoft.com/en-us/assistance/HP030823691033.aspx");
}
