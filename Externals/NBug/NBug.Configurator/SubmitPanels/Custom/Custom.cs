using System.Windows.Forms;

namespace NBug.Configurator.SubmitPanels.Custom
{
    public partial class Custom : UserControl, ISubmitPanel
    {
        public Custom()
        {
            InitializeComponent();
        }


        public string ConnectionString
        {
            get
            {
                var custom = new Core.Submission.Custom.Custom();
                return custom.ConnectionString;
            }

            set
            {
                //additional connection string parameters are not supported for Custom submission.
            }
        }
    }
}
