namespace NBug.Core.Submission.Tracker.Mantis
{

	[System.SerializableAttribute(), System.Xml.Serialization.SoapTypeAttribute(Namespace = "http://futureware.biz/mantisconnect")]
	public class CustomFieldValueForIssueData
	{

		private ObjectRef fieldField;

		private string valueField;
		public ObjectRef field {
			get { return this.fieldField; }
			set { this.fieldField = value; }
		}

		public string value {
			get { return this.valueField; }
			set { this.valueField = value; }
		}

	}
}
