// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExceptionDetails.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.UI.WinForms.Panels
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Windows.Forms;

	using NBug.Core.Util.Serialization;

	internal partial class ExceptionDetails : UserControl
	{
		private readonly Dictionary<TreeNode, SerializableException> exceptionDetailsList = new Dictionary<TreeNode, SerializableException>();

		public ExceptionDetails()
		{
			this.InitializeComponent();
		}

		public int InformationColumnWidth
		{
			get
			{
				return this.exceptionDetailsListView.Columns[1].Width;
			}

			set
			{
				this.exceptionDetailsListView.Columns[1].Width = value;
			}
		}

		public int PropertyColumnWidth
		{
			get
			{
				return this.exceptionDetailsListView.Columns[0].Width;
			}

			set
			{
				this.exceptionDetailsListView.Columns[0].Width = value;
			}
		}

		internal void Initialize(SerializableException exception)
		{
			this.exceptionDetailsList.Add(this.exceptionTreeView.Nodes.Add(exception.Type), exception);

			if (exception.InnerException != null)
			{
				this.FillInnerExceptionTree(exception.InnerException, this.exceptionTreeView.Nodes[0]);
			}

			if (exception.InnerExceptions != null)
			{
				foreach (var innerException in exception.InnerExceptions)
				{
					this.FillInnerExceptionTree(innerException, this.exceptionTreeView.Nodes[0]);
				}
			}

			this.exceptionTreeView.ExpandAll();
			this.DisplayExceptionDetails(this.exceptionTreeView.Nodes[0]);
		}

		private void DisplayExceptionDetails(TreeNode node)
		{
			var exception = this.exceptionDetailsList[node];
			this.exceptionDetailsListView.SuspendLayout();
			this.exceptionDetailsListView.Items.Clear();

			if (exception.Type != null)
			{
				this.exceptionDetailsListView.Items.Add("Exception").SubItems.Add(exception.Type);
			}

			if (exception.Message != null)
			{
				this.exceptionDetailsListView.Items.Add("Message").SubItems.Add(exception.Message);
			}

			if (exception.TargetSite != null)
			{
				this.exceptionDetailsListView.Items.Add("Target Site").SubItems.Add(exception.TargetSite);
			}

			if (exception.InnerException != null)
			{
				this.exceptionDetailsListView.Items.Add("Inner Exception").SubItems.Add(exception.InnerException.Type);
			}

			if (exception.Source != null)
			{
				this.exceptionDetailsListView.Items.Add("Source").SubItems.Add(exception.Source);
			}

			if (exception.HelpLink != null)
			{
				this.exceptionDetailsListView.Items.Add("Help Link").SubItems.Add(exception.HelpLink);
			}

			if (exception.StackTrace != null)
			{
				this.exceptionDetailsListView.Items.Add("Stack Trace").SubItems.Add(exception.StackTrace);
			}

			if (exception.Data != null)
			{
				foreach (var pair in exception.Data)
				{
					this.exceptionDetailsListView.Items.Add(string.Format("Data[\"{0}\"]", pair.Key)).SubItems.Add(pair.Value.ToString());
				}
			}

			if (exception.ExtendedInformation != null)
			{
				foreach (var info in exception.ExtendedInformation)
				{
					var item = this.exceptionDetailsListView.Items.Add(info.Key);
					item.UseItemStyleForSubItems = false;
					item.Font = new Font(this.Font, FontStyle.Bold);
					item.SubItems.Add(info.Value.ToString());
				}
			}

			this.exceptionDetailsListView.ResumeLayout();
		}

		private void ExceptionDetailsListView_DoubleClick(object sender, EventArgs e)
		{
			using (var detailView = new ExceptionDetailView())
			{
				detailView.ShowDialog(this.exceptionDetailsListView.SelectedItems[0].Text, this.exceptionDetailsListView.SelectedItems[0].SubItems[1].Text);
			}
		}

		private void ExceptionDetailsListView_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
		{
			this.toolTip.RemoveAll();
			this.toolTip.Show(e.Item.SubItems[1].Text, this.exceptionDetailsListView);
		}

		private void ExceptionTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			this.DisplayExceptionDetails(e.Node);
		}

		private void FillInnerExceptionTree(SerializableException innerException, TreeNode innerNode)
		{
			this.exceptionDetailsList.Add(innerNode.Nodes.Add(innerException.Type), innerException);

			if (innerException.InnerException != null)
			{
				this.FillInnerExceptionTree(innerException.InnerException, innerNode.Nodes[0]);
			}
		}
	}
}