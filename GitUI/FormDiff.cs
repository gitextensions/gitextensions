using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using System.Threading;

namespace GitUI
{
    public partial class FormDiff : Form
    {
        public FormDiff()
        {
            InitializeComponent();
            EditorOptions.SetSyntax(OutPut, "output.cs");
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }




        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ShortDiffDto shortdto = new ShortDiffDto(From.Text, To.Text);
                ShortDiff shortdiff = new ShortDiff(shortdto);
                shortdiff.Execute();

                string message = "There are " + shortdto.Result + ".\nWhen the difference report is big, showing the diff can take a while.";

                if (MessageBox.Show(message, "Diff", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {

                    DiffDto dto = new DiffDto(From.Text, To.Text);
                    Diff diff = new Diff(dto);
                    diff.Execute();

                    OutPut.Text = dto.Result;
                }
            }
            catch
            {
            }
        }

        private void From_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void FormDiff_Load(object sender, EventArgs e)
        {
            From.DisplayMember = "Name";
            From.DataSource = GitCommands.GitCommands.GetHeads();
            
            To.DisplayMember = "Name";
            To.DataSource = GitCommands.GitCommands.GetHeads();
        }
    }
}
