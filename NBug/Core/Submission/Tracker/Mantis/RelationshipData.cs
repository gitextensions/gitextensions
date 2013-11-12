namespace NBug.Core.Submission.Tracker.Mantis
{
	[System.SerializableAttribute(), System.Xml.Serialization.SoapTypeAttribute(Namespace = "http://futureware.biz/mantisconnect")]
	public class RelationshipData
	{
		private string idField;

		private ObjectRef typeField;

		private string target_idField;
		[System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
		public string id {
			get { return this.idField; }
			set { this.idField = value; }
		}

		public ObjectRef type {
			get { return this.typeField; }
			set { this.typeField = value; }
		}

		[System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
		public string target_id {
			get { return this.target_idField; }
			set { this.target_idField = value; }
		}
	}

}
