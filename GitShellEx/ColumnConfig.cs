using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
namespace FileHashShell
{
   
    public class ColumnConfig
    {

        public ColumnConfig()
        {
        }
        public ColumnConfig(string columnName, int sortOrder, bool isVisible, int columnWidth)
        {
            this.columnName = columnName;
            this.sortOrder = sortOrder;
            this.isVisible = isVisible;
            this.columnWidth = columnWidth;
        }
        string columnName;

        public string ColumnName
        {
            get { return columnName; }
            set { columnName = value; }
        }
        bool isVisible;

        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value; }
        }
        int sortOrder;

        public int SortOrder
        {
            get { return sortOrder; }
            set { sortOrder = value; }
        }
        int columnWidth;

        public int ColumnWidth
        {
            get { return columnWidth; }
            set { columnWidth = value; }
        } 
    }
}
