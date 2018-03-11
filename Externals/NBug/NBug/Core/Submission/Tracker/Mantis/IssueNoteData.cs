namespace NBug.Core.Submission.Tracker.Mantis
{

	[System.SerializableAttribute(), System.Xml.Serialization.SoapTypeAttribute(Namespace = "http://futureware.biz/mantisconnect")]
	public class IssueNoteData
	{


		private string idField;

		private AccountData reporterField;

		private string textField;

		private ObjectRef view_stateField;

		private System.DateTime date_submittedField;

		private bool date_submittedFieldSpecified;

		private System.DateTime last_modifiedField;

		private bool last_modifiedFieldSpecified;

		private string time_trackingField;

		private string note_typeField;

		private string note_attrField;
		[System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
		public string id {
			get { return this.idField; }
			set { this.idField = value; }
		}

		public AccountData reporter {
			get { return this.reporterField; }
			set { this.reporterField = value; }
		}

		public string text {
			get { return this.textField; }
			set { this.textField = value; }
		}

		public ObjectRef view_state {
			get { return this.view_stateField; }
			set { this.view_stateField = value; }
		}

		public System.DateTime date_submitted {
			get { return this.date_submittedField; }
			set { this.date_submittedField = value; }
		}

		[System.Xml.Serialization.SoapIgnoreAttribute()]
		public bool date_submittedSpecified {
			get { return this.date_submittedFieldSpecified; }
			set { this.date_submittedFieldSpecified = value; }
		}

		public System.DateTime last_modified {
			get { return this.last_modifiedField; }
			set { this.last_modifiedField = value; }
		}

		[System.Xml.Serialization.SoapIgnoreAttribute()]
		public bool last_modifiedSpecified {
			get { return this.last_modifiedFieldSpecified; }
			set { this.last_modifiedFieldSpecified = value; }
		}

		[System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
		public string time_tracking {
			get { return this.time_trackingField; }
			set { this.time_trackingField = value; }
		}

		[System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
		public string note_type {
			get { return this.note_typeField; }
			set { this.note_typeField = value; }
		}

		public string note_attr {
			get { return this.note_attrField; }
			set { this.note_attrField = value; }
		}
	}

}
