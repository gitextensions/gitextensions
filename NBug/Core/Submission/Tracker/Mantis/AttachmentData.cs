namespace NBug.Core.Submission.Tracker.Mantis
{

	[System.SerializableAttribute(), System.Xml.Serialization.SoapTypeAttribute(Namespace = "http://futureware.biz/mantisconnect")]
	public class AttachmentData
	{


		private string idField;

		private string filenameField;

		private string sizeField;

		private string content_typeField;

		private System.DateTime date_submittedField;

		private bool date_submittedFieldSpecified;

		private string download_urlField;

		private string user_idField;
		[System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
		public string id {
			get { return this.idField; }
			set { this.idField = value; }
		}

		public string filename {
			get { return this.filenameField; }
			set { this.filenameField = value; }
		}

		[System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
		public string size {
			get { return this.sizeField; }
			set { this.sizeField = value; }
		}

		public string content_type {
			get { return this.content_typeField; }
			set { this.content_typeField = value; }
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

		[System.Xml.Serialization.SoapElementAttribute(DataType = "anyURI")]
		public string download_url {
			get { return this.download_urlField; }
			set { this.download_urlField = value; }
		}

		[System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
		public string user_id {
			get { return this.user_idField; }
			set { this.user_idField = value; }
		}
	}

}
