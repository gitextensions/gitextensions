namespace NBug.Core.Submission.Tracker.Mantis
{

	[System.SerializableAttribute(), System.Diagnostics.DebuggerStepThroughAttribute(), System.Xml.Serialization.SoapTypeAttribute(Namespace = "http://futureware.biz/mantisconnect")]
	public class AccountData
	{

		private string idField;
		private string nameField;
		private string real_nameField;

		private string emailField;
		[System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
		public string id {
			get { return this.idField; }
			set { this.idField = value; }
		}

		public string name {
			get { return this.nameField; }
			set { this.nameField = value; }
		}

		public string real_name {
			get { return this.real_nameField; }
			set { this.real_nameField = value; }
		}

		public string email {
			get { return this.emailField; }
			set { this.emailField = value; }
		}

	}

}
