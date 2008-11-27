using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
namespace FileHashShell
{
    public partial class ShellForm : Form
    {
        List<string> fileNames = new List<string>();
        HashHelper.HashType hashType;
        int startingWidth = -1;
        bool saveWidth = false;
        public ShellForm()
        {
            InitializeComponent();
            this.startingWidth = this.Width;
        }
        public ShellForm(List<string> fileNames, HashHelper.HashType type)
      : this()
        {
            this.hashType = type;
            this.fileNames = fileNames;
            KeyValuePair<HashHelper.HashType, List<string>> pair = new KeyValuePair<HashHelper.HashType, List<string>>(type, fileNames);
           
            bgWorker.RunWorkerAsync(pair);
        }
        public ShellForm(string fileName) :this(new List<string>(new string[]{fileName}),HashHelper.HashType.SHA1)
        {
        }
        public ShellForm(string[] fileNames)
            : this(new List<string>(fileNames), HashHelper.HashType.SHA256)
        {
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BindingList<FileInformation> data = new BindingList<FileInformation>();
            if (e.Argument is KeyValuePair<HashHelper.HashType, List<string>>)
            {
                List<string> files = ((KeyValuePair<HashHelper.HashType, List<string>>)e.Argument).Value;
                HashHelper.HashType type = ((KeyValuePair<HashHelper.HashType, List<string>>)e.Argument).Key;
                foreach (string file in files)
                {
                    FileInfo fileInf = new FileInfo(file);
                    FileInformation inf = new FileInformation();
                    inf.Name = fileInf.Name;
                    bgWorker.ReportProgress(0, fileInf.Name);
                    inf.Path = fileInf.FullName;
                    inf.Hash = HashHelper.GetFileHash(file, type);
                    inf.FileDate = fileInf.LastWriteTime;
                    inf.FileSize = (Int64)fileInf.Length/1000;
                    
                    data.Add(inf);
                }
            }
            e.Result = data;
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblFileName.Text = e.UserState.ToString();
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BindingList<FileInformation> fileList = (BindingList<FileInformation>)e.Result;
            if (!Properties.Settings.Default.IncludeDate)
                this.fileDateDataGridViewTextBoxColumn.Visible = false;
            else
                this.fileDateDataGridViewTextBoxColumn.Visible = true;

            if (!Properties.Settings.Default.IncludeFileSize)
                this.fileSizeDataGridViewTextBoxColumn.Visible = false;
            else
                this.fileSizeDataGridViewTextBoxColumn.Visible = true;

            dataGridView1.DataSource = fileList;

            //expand height to show up to 10 rows
            this.Height = 67 + (dataGridView1.Rows[0].Height *  ((fileList.Count > 10)? 10 : fileList.Count));
            

            //expand width to fit the columns up to 600 if it hasn't been modified by the user
            if (Properties.Settings.Default.FormWidth > 0)
            {
                this.Width = Properties.Settings.Default.FormWidth;
            }else
            {
                int colWidth = 0;
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    colWidth += dataGridView1.Columns[i].Width;

                if (colWidth > 730) colWidth = 730;

                if (dataGridView1.Rows.Count < 10)
                {
                    this.Width = 12 + colWidth;
                }
                else
                {
                    this.Width = 30 + colWidth;
                }
            }
            //See if we're going to hang over the right side of the screen.. if so, shift to the left...
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            if ((this.Location.X + this.Width) > screenWidth)
            {
                //see how much we have to shift
                int overhang = this.Location.X + this.Width - screenWidth;
                if ((this.Location.X - overhang) > 0)
                    this.Location = new Point(this.Location.X - overhang, this.Location.Y);
                else
                    this.Location = new Point(0, this.Location.Y);
            }
            panel1.SendToBack();

            this.dataGridView1.ColumnDisplayIndexChanged += new System.Windows.Forms.DataGridViewColumnEventHandler(this.dataGridView1_ColumnDisplayIndexChanged);
            this.dataGridView1.ColumnWidthChanged += new System.Windows.Forms.DataGridViewColumnEventHandler(this.dataGridView1_ColumnWidthChanged);
            this.SizeChanged += new System.EventHandler(this.ShellForm_SizeChanged);

         
        }

        private void ShellForm_Load(object sender, EventArgs e)
        {
            string desc = HashHelper.GetDescription(hashType);
            this.hashDataGridViewTextBoxColumn.HeaderText = desc + " File Hash";
            this.Text = desc  + " File Hash";

            this.pathDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            this.pathDataGridViewTextBoxColumn.Width = 200;

            Point point = System.Windows.Forms.Cursor.Position;
            Point screenPoint = this.PointToScreen(point);
            this.Location = screenPoint;

            if (Properties.Settings.Default.CustomColumnConfig != null)
            {
                List<ColumnConfig> cfgs = Properties.Settings.Default.CustomColumnConfig;
                foreach (DataGridViewColumn col in this.dataGridView1.Columns)
                {
                    for (int i = 0; i < cfgs.Count; i++)
                    {
                        if (cfgs[i].ColumnName == col.Name)
                        {
                            col.Width = cfgs[i].ColumnWidth;
                            col.DisplayIndex = cfgs[i].SortOrder;
                        }
                    }
                }
            }
            else
            {
                this.nameDataGridViewTextBoxColumn.DisplayIndex = 0;
                this.hashDataGridViewTextBoxColumn.DisplayIndex = 1;
                this.fileSizeDataGridViewTextBoxColumn.DisplayIndex = 2;
                this.fileDateDataGridViewTextBoxColumn.DisplayIndex = 3;
                this.pathDataGridViewTextBoxColumn.DisplayIndex = 4;
            }
           

        }

        private void ShellForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                Application.Exit();
            }
        }

        private void dataGridView1_ColumnDisplayIndexChanged(object sender, DataGridViewColumnEventArgs e)
        {
            SaveColumnDefaults();
        }

        private void dataGridView1_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            SaveColumnDefaults();
        }
        private void SaveColumnDefaults()
        {
            List<ColumnConfig> cfgs = new List<ColumnConfig>();
            foreach (DataGridViewColumn col in this.dataGridView1.Columns)
            {
                ColumnConfig cfg = new ColumnConfig(col.Name, col.DisplayIndex, col.Displayed, col.Width);
                cfgs.Add(cfg);
            }
            Properties.Settings.Default.CustomColumnConfig = cfgs;
            Properties.Settings.Default.Save();
        }

        private void ShellForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.Width != this.startingWidth)
            {
                this.saveWidth = true;
            }
        }

        private void ShellForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.saveWidth)
            {
                Properties.Settings.Default.FormWidth = this.Width;
                Properties.Settings.Default.Save();
            }
        }
      
    }
}