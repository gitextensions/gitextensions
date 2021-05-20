// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExceptionDetails.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BugReporter.Serialization;

namespace BugReporter
{
    internal partial class ExceptionDetails : UserControl
    {
        private readonly Dictionary<TreeNode, SerializableException> _exceptionDetailsList = new();

        public ExceptionDetails()
        {
            InitializeComponent();
        }

        public int InformationColumnWidth
        {
            get
            {
                return exceptionDetailsListView.Columns[1].Width;
            }

            set
            {
                exceptionDetailsListView.Columns[1].Width = value;
            }
        }

        public int PropertyColumnWidth
        {
            get
            {
                return exceptionDetailsListView.Columns[0].Width;
            }

            set
            {
                exceptionDetailsListView.Columns[0].Width = value;
            }
        }

        internal void Initialize(SerializableException exception)
        {
            _exceptionDetailsList.Add(exceptionTreeView.Nodes.Add(exception.Type), exception);

            if (exception.InnerException is not null)
            {
                FillInnerExceptionTree(exception.InnerException, exceptionTreeView.Nodes[0]);
            }

            if (exception.InnerExceptions is not null)
            {
                foreach (var innerException in exception.InnerExceptions)
                {
                    FillInnerExceptionTree(innerException, exceptionTreeView.Nodes[0]);
                }
            }

            exceptionTreeView.ExpandAll();
            DisplayExceptionDetails(exceptionTreeView.Nodes[0]);
        }

        private void DisplayExceptionDetails(TreeNode node)
        {
            var exception = _exceptionDetailsList[node];
            exceptionDetailsListView.SuspendLayout();
            exceptionDetailsListView.Items.Clear();

            if (exception.Type is not null)
            {
                exceptionDetailsListView.Items.Add("Exception").SubItems.Add(exception.Type);
            }

            if (exception.Message is not null)
            {
                exceptionDetailsListView.Items.Add("Message").SubItems.Add(exception.Message);
            }

            if (exception.TargetSite is not null)
            {
                exceptionDetailsListView.Items.Add("Target Site").SubItems.Add(exception.TargetSite);
            }

            if (exception.InnerException is not null)
            {
                exceptionDetailsListView.Items.Add("Inner Exception").SubItems.Add(exception.InnerException.Type);
            }

            if (exception.Source is not null)
            {
                exceptionDetailsListView.Items.Add("Source").SubItems.Add(exception.Source);
            }

            if (exception.HelpLink is not null)
            {
                exceptionDetailsListView.Items.Add("Help Link").SubItems.Add(exception.HelpLink);
            }

            if (exception.StackTrace is not null)
            {
                exceptionDetailsListView.Items.Add("Stack Trace").SubItems.Add(exception.StackTrace);
            }

            if (exception.Data is not null)
            {
                foreach (var pair in exception.Data)
                {
                    exceptionDetailsListView.Items.Add(string.Format("Data[\"{0}\"]", pair.Key)).SubItems.Add(pair.Value.ToString());
                }
            }

            if (exception.ExtendedInformation is not null)
            {
                foreach (var info in exception.ExtendedInformation)
                {
                    var item = exceptionDetailsListView.Items.Add(info.Key);
                    item.UseItemStyleForSubItems = false;
                    item.Font = new Font(Font, FontStyle.Bold);
                    item.SubItems.Add(info.Value.ToString());
                }
            }

            exceptionDetailsListView.ResumeLayout();
        }

        private void ExceptionDetailsListView_DoubleClick(object sender, EventArgs e)
        {
            using ExceptionDetailView detailView = new();
            detailView.ShowDialog(exceptionDetailsListView.SelectedItems[0].Text, exceptionDetailsListView.SelectedItems[0].SubItems[1].Text);
        }

        private void ExceptionDetailsListView_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            toolTip.RemoveAll();
            toolTip.Show(e.Item.SubItems[1].Text, exceptionDetailsListView);
        }

        private void ExceptionTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DisplayExceptionDetails(e.Node);
        }

        private void FillInnerExceptionTree(SerializableException innerException, TreeNode innerNode)
        {
            _exceptionDetailsList.Add(innerNode.Nodes.Add(innerException.Type), innerException);

            if (innerException.InnerException is not null)
            {
                FillInnerExceptionTree(innerException.InnerException, innerNode.Nodes[0]);
            }
        }

        internal TestAccessor GetTestAccessor()
            => new(this);

        internal readonly struct TestAccessor
        {
            private readonly ExceptionDetails _form;

            public TestAccessor(ExceptionDetails form)
            {
                _form = form;
            }

            public ListView ExceptionDetailsListView => _form.exceptionDetailsListView;
        }
    }
}
