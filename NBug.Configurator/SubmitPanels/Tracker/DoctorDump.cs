namespace NBug.Configurator.SubmitPanels.Tracker
{
	using System.Windows.Forms;

	public partial class DoctorDump : UserControl, ISubmitPanel
	{
        public DoctorDump()
		{
			this.InitializeComponent();
            AppIdTextBox.Text = System.Guid.NewGuid().ToString("D");
        }

		public string ConnectionString
		{
			get
			{
				// Check the mandatory fields
                if (string.IsNullOrEmpty(this.EmailTextBox.Text) || this.EmailTextBox.Text.IndexOf('@') <= 0)
				{
					MessageBox.Show(
                        "Email is mandatory field, it cannot be left blank.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return null;
				}

                var doctorDump = new Core.Submission.Tracker.DoctorDump.DoctorDump
                {
                    ApplicationGUID = AppIdTextBox.Text,
                    Email = EmailTextBox.Text,
                    SendAnonymousReportSilently = SendAnonymousCheckBox.Checked,
                    OpenProblemSolutionPage = OpenSolutionCheckBox.Checked
                };

				return doctorDump.ConnectionString;
			}

			set
			{
                var doctorDump = new Core.Submission.Tracker.DoctorDump.DoctorDump(value);
                this.AppIdTextBox.Text = doctorDump.ApplicationGUID;
                this.EmailTextBox.Text = doctorDump.Email;
                this.SendAnonymousCheckBox.Checked = doctorDump.SendAnonymousReportSilently;
                this.OpenSolutionCheckBox.Checked = doctorDump.OpenProblemSolutionPage;
			}
		}

        private void NewIdButton_Click(object sender, System.EventArgs e)
        {
            AppIdTextBox.Text = System.Guid.NewGuid().ToString("D");
        }

        static System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"^[^@]+@[^@]+\.[\w\d]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

        private void EmailTextBox_TextChanged(object sender, System.EventArgs e)
        {
            if (r.IsMatch(EmailTextBox.Text))
                emailNotValid.SetError(EmailTextBox, string.Empty);
            else
                emailNotValid.SetError(EmailTextBox, "Valid e-mail address is required.");
        }
	}
}