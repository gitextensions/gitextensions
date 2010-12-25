//
// TortoiseSVN Diff script for Word Doc files
//
// Copyright (C) 2004-2008 the TortoiseSVN team
// This file is distributed under the same license as TortoiseSVN
//
// Last commit by:
// $Author: tortoisesvn $
// $Date: 2008-12-05 17:38:43 +0100 (Fr, 05 Dez 2008) $
// $Rev: 14781 $
//
// Authors:
// Jared Silva, 2008
// Davide Orlandi and Hans-Emil Skogh, 2005
//

var objArgs,num,sBaseDoc,sNewDoc,sTempDoc,objScript,word,destination;
// Microsoft Office versions for Microsoft Windows OS
var vOffice2000 = 9;
var vOffice2002 = 10;
var vOffice2003 = 11;
var vOffice2007 = 12;
// WdCompareTarget
var wdCompareTargetSelected = 0;
var wdCompareTargetCurrent = 1;
var wdCompareTargetNew = 2;
// WdViewType
var wdMasterView = 5;
var wdNormalView = 1;
var wdOutlineView = 2;
// WdSaveOptions
var wdDoNotSaveChanges = 0;
var wdPromptToSaveChanges = -2;
var wdSaveChanges = -1;

objArgs = WScript.Arguments;
num = objArgs.length;
if (num < 2)
{
   WScript.Echo("Usage: [CScript | WScript] diff-doc.js base.doc new.doc");
   WScript.Quit(1);
}

sBaseDoc = objArgs(0);
sNewDoc = objArgs(1);

objScript = new ActiveXObject("Scripting.FileSystemObject");
if ( ! objScript.FileExists(sBaseDoc))
{
    WScript.Echo("File " + sBaseDoc + " does not exist.  Cannot compare the documents.");
    WScript.Quit(1);
}
if ( ! objScript.FileExists(sNewDoc))
{
    WScript.Echo("File " + sNewDoc + " does not exist.  Cannot compare the documents.");
    WScript.Quit(1);
}

try
{
   word = WScript.CreateObject("Word.Application");
}
catch(e)
{
	// before giving up, try with OpenOffice
	try
	{
		var OO;
		OO = WScript.CreateObject("com.sun.star.ServiceManager");
	}
	catch(e)
	{
		WScript.Echo("You must have Microsoft Word or OpenOffice installed to perform this operation.");
		WScript.Quit(1);
	}
	// yes, OO is installed - do the diff with that one instead
	var objFile = objScript.GetFile(sNewDoc);
	if ((objFile.Attributes & 1)==1)
	{
		// reset the readonly attribute
		objFile.Attributes = objFile.Attributes & (~1);
	}
	//Create the DesktopSet 
	var objDesktop = OO.createInstance("com.sun.star.frame.Desktop");
	var objUriTranslator = OO.createInstance("com.sun.star.uri.ExternalUriReferenceTranslator");
	//Adjust the paths for OO
	sBaseDoc = sBaseDoc.replace(/\\/g, "/");
	sBaseDoc = sBaseDoc.replace(/:/g, "|");
	sBaseDoc = sBaseDoc.replace(/ /g, "%20");
	sBaseDoc="file:///" + sBaseDoc;
	sBaseDoc=objUriTranslator.translateToInternal(sBaseDoc);
	sNewDoc = sNewDoc.replace(/\\/g, "/");
	sNewDoc = sNewDoc.replace(/:/g, "|");
	sNewDoc = sNewDoc.replace(/ /g, "%20");
	sNewDoc="file:///" + sNewDoc;
	sNewDoc=objUriTranslator.translateToInternal(sNewDoc);

	//Open the %base document
	var oPropertyValue = new Array();
	oPropertyValue[0] = OO.Bridge_GetStruct("com.sun.star.beans.PropertyValue");
	oPropertyValue[0].Name = "ShowTrackedChanges";
	oPropertyValue[0].Value = true;
	var objDocument=objDesktop.loadComponentFromURL(sNewDoc,"_blank", 0, oPropertyValue);
	
	//Set the frame
	var Frame = objDesktop.getCurrentFrame();
	
	var dispatcher=OO.CreateInstance("com.sun.star.frame.DispatchHelper");
	
	//Execute the comparison
	dispatcher.executeDispatch(Frame, ".uno:ShowTrackedChanges", "", 0, oPropertyValue);
	oPropertyValue[0].Name = "URL";
	oPropertyValue[0].Value = sBaseDoc;
	dispatcher.executeDispatch(Frame, ".uno:CompareDocuments", "", 0, oPropertyValue);
	WScript.Quit(0);
}

if (parseInt(word.Version) >= vOffice2007)
{
	sTempDoc = sNewDoc;
	sNewDoc = sBaseDoc;
	sBaseDoc = sTempDoc;
}

objScript = null;

word.visible = true;

// Open the new document
try
{
    destination = word.Documents.Open(sNewDoc, true, true);
}
catch(e)
{
    WScript.Echo("Error opening " + sNewDoc);
    // Quit
    WScript.Quit(1);
}

// If the Type property returns either wdOutlineView or wdMasterView and the Count property returns zero, the current document is an outline.
if (((destination.ActiveWindow.View.Type == wdOutlineView) || (destination.ActiveWindow.View.Type == wdMasterView)) && (destination.Subdocuments.Count == 0))
{
    // Change the Type property of the current document to normal
    destination.ActiveWindow.View.Type = wdNormalView;
}

// Compare to the base document
if (parseInt(word.Version) <= vOffice2000)
{
    // Compare for Office 2000 and earlier
    try
    {
        destination.Compare(sBaseDoc);
    }
    catch(e)
    {
        WScript.Echo("Error comparing " + sBaseDoc + " and " + sNewDoc);
        // Quit
        WScript.Quit(1);
    }
}
else
{
    // Compare for Office XP (2002) and later
    try
    {
        destination.Compare(sBaseDoc, "Comparison", wdCompareTargetNew, true, true);
    }
    catch(e)
    {
        WScript.Echo("Error comparing " + sBaseDoc + " and " + sNewDoc);
        // Close the first document and quit
        destination.Close(wdDoNotSaveChanges);
        WScript.Quit(1);
    }
}
    
// Show the comparison result
if (parseInt(word.Version) < vOffice2007)
{
	word.ActiveDocument.Windows(1).Visible = 1;
}
    
// Mark the comparison document as saved to prevent the annoying
// "Save as" dialog from appearing.
word.ActiveDocument.Saved = 1;
    
// Close the first document
if (parseInt(word.Version) >= vOffice2002)
{
    destination.Close(wdDoNotSaveChanges);
}
