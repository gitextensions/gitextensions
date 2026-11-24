// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExceptionDetailView.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using GitUI.Compat;
using ResourceManager;

namespace BugReporter;

// Framework constraint: the original plain Form has no XLF translation surface.
[Untranslated]
internal partial class ExceptionDetailView : GitExtensionsFormBase
{
    public ExceptionDetailView()
    {
        InitializeComponent();
        InitializeComplete();
    }

    internal void ShowDialog(string property, string info, IWin32Window? owner = null)
    {
        propertyTextBox.Text = property;
        propertyInformationTextBox.Text = info;

        // Avalonia requires the implicit WinForms active modal owner to be supplied explicitly.
        ShowDialog(owner);
    }
}
