// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExceptionDetails.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using BugReporter.Serialization;

namespace BugReporter;

internal partial class ExceptionDetails : UserControl
{
    private readonly Dictionary<TreeViewItem, SerializableException> _exceptionDetailsList = [];

    public ExceptionDetails()
    {
        InitializeComponent();
        exceptionTreeView.SelectionChanged += ExceptionTreeView_AfterSelect;
        exceptionDetailsListView.DoubleTapped += ExceptionDetailsListView_DoubleClick;
        exceptionDetailsListView.PointerMoved += ExceptionDetailsListView_ItemMouseHover;
        exceptionDetailsListView.ItemTemplate = new FuncDataTemplate<KeyValuePair<string, string>>((item, _) =>
        {
            Grid row = new() { ColumnDefinitions = new ColumnDefinitions($"{PropertyColumnWidth},{InformationColumnWidth}") };
            TextBlock property = new() { Text = item.Key };
            if (exceptionTreeView.SelectedItem is TreeViewItem node
                && _exceptionDetailsList[node].ExtendedInformation?.ContainsKey(item.Key) == true)
            {
                property.FontWeight = Avalonia.Media.FontWeight.Bold;
            }

            TextBlock information = new() { Text = item.Value };
            Grid.SetColumn(information, 1);
            ToolTip.SetTip(row, item.Value);
            row.Children.Add(property);
            row.Children.Add(information);
            return row;
        });
    }

    public int InformationColumnWidth
    {
        get
        {
            return (int)informationColumnHeader.Width;
        }

        set
        {
            informationColumnHeader.Width = value;
        }
    }

    public int PropertyColumnWidth
    {
        get
        {
            return (int)propertyColumnHeader.Width;
        }

        set
        {
            propertyColumnHeader.Width = value;
        }
    }

    internal void Initialize(SerializableException exception)
    {
        TreeViewItem node = new() { Header = exception.Type, IsExpanded = true };
        exceptionTreeView.Items.Add(node);
        _exceptionDetailsList.Add(node, exception);

        if (exception.InnerException is not null)
        {
            FillInnerExceptionTree(exception.InnerException, node);
        }

        if (exception.InnerExceptions is not null)
        {
            foreach (SerializableException innerException in exception.InnerExceptions)
            {
                FillInnerExceptionTree(innerException, node);
            }
        }

        exceptionTreeView.SelectedItem = node;
        DisplayExceptionDetails(node);
    }

    private void DisplayExceptionDetails(TreeViewItem node)
    {
        SerializableException exception = _exceptionDetailsList[node];
        exceptionDetailsListView.Items.Clear();

        if (exception.Type is not null)
        {
            exceptionDetailsListView.Items.Add(new KeyValuePair<string, string>("Exception", exception.Type ?? string.Empty));
        }

        if (exception.Message is not null)
        {
            exceptionDetailsListView.Items.Add(new KeyValuePair<string, string>("Message", exception.Message ?? string.Empty));
        }

        if (exception.TargetSite is not null)
        {
            exceptionDetailsListView.Items.Add(new KeyValuePair<string, string>("Target Site", exception.TargetSite ?? string.Empty));
        }

        if (exception.InnerException is not null)
        {
            exceptionDetailsListView.Items.Add(new KeyValuePair<string, string>("Inner Exception", exception.InnerException.Type ?? string.Empty));
        }

        if (exception.Source is not null)
        {
            exceptionDetailsListView.Items.Add(new KeyValuePair<string, string>("Source", exception.Source ?? string.Empty));
        }

        if (exception.HelpLink is not null)
        {
            exceptionDetailsListView.Items.Add(new KeyValuePair<string, string>("Help Link", exception.HelpLink ?? string.Empty));
        }

        if (exception.StackTrace is not null)
        {
            exceptionDetailsListView.Items.Add(new KeyValuePair<string, string>("Stack Trace", exception.StackTrace ?? string.Empty));
        }

        if (exception.Data is not null)
        {
            foreach (KeyValuePair<object, object> pair in exception.Data)
            {
                exceptionDetailsListView.Items.Add(new KeyValuePair<string, string>(string.Format("Data[\"{0}\"]", pair.Key), pair.Value.ToString() ?? string.Empty));
            }
        }

        if (exception.ExtendedInformation is not null)
        {
            foreach (KeyValuePair<string, object> info in exception.ExtendedInformation)
            {
                exceptionDetailsListView.Items.Add(new KeyValuePair<string, string>(info.Key, info.Value.ToString() ?? string.Empty));
            }
        }
    }

    private void ExceptionDetailsListView_DoubleClick(object sender, EventArgs e)
    {
        if (exceptionDetailsListView.SelectedItem is KeyValuePair<string, string> selected)
        {
            using ExceptionDetailView detailView = new();
            detailView.ShowDialog(selected.Key, selected.Value, TopLevel.GetTopLevel(this) as IWin32Window);
        }
    }

    private void ExceptionDetailsListView_ItemMouseHover(object sender, PointerEventArgs e)
    {
        // Avalonia item templates provide the hovered row's full value through ToolTip.Tip.
        if (e.Source is Control { DataContext: KeyValuePair<string, string> item })
        {
            ToolTip.SetTip(exceptionDetailsListView, item.Value);
        }
    }

    private void ExceptionTreeView_AfterSelect(object sender, SelectionChangedEventArgs e)
    {
        if (exceptionTreeView.SelectedItem is TreeViewItem selected)
        {
            DisplayExceptionDetails(selected);
        }
    }

    private void FillInnerExceptionTree(SerializableException innerException, TreeViewItem innerNode)
    {
        TreeViewItem node = new() { Header = innerException.Type, IsExpanded = true };
        innerNode.Items.Add(node);
        _exceptionDetailsList.Add(node, innerException);

        if (innerException.InnerException is not null)
        {
            FillInnerExceptionTree(innerException.InnerException, (TreeViewItem)innerNode.Items[0]!);
        }
    }

    // parity-scaffolding: retains the original detail-list test boundary.
    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor
    {
        private readonly ExceptionDetails _form;

        public TestAccessor(ExceptionDetails form)
        {
            _form = form;
        }

        public ListBox ExceptionDetailsListView => _form.exceptionDetailsListView;
    }
}
