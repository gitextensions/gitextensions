namespace GitUI
{
    partial class FormCheckout
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.RevisionGrid = new GitUI.RevisionGrid();
            this.Force = new System.Windows.Forms.CheckBox();
            this.Ok = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionGrid.Location = new System.Drawing.Point(4, 4);
            this.RevisionGrid.Margin = new System.Windows.Forms.Padding(4);
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.Size = new System.Drawing.Size(676, 367);
            this.RevisionGrid.TabIndex = 0;
            // 
            // Force
            // 
            this.Force.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Force.AutoSize = true;
            this.Force.Location = new System.Drawing.Point(3, 6);
            this.Force.Name = "Force";
            this.Force.Size = new System.Drawing.Size(55, 19);
            this.Force.TabIndex = 3;
            this.Force.Text = "Force";
            this.Force.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.Ok.Location = new System.Drawing.Point(589, 3);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(86, 25);
            this.Ok.TabIndex = 2;
            this.Ok.Text = "Checkout";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.RevisionGrid, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(684, 412);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.Ok, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.Force, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 378);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(678, 31);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // FormCheckout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(684, 412);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(400, 200);
            this.Name = "FormCheckout";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Checkout revision";
            this.Load += new System.EventHandler(this.FormCheckoutLoad);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Ok;
        private RevisionGrid RevisionGrid;
        private System.Windows.Forms.CheckBox Force;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}