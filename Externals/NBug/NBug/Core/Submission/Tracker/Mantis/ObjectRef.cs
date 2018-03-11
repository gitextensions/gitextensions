namespace NBug.Core.Submission.Tracker.Mantis
{
	[System.SerializableAttribute(), System.Xml.Serialization.SoapTypeAttribute(Namespace = "http://futureware.biz/mantisconnect")]
	public class ObjectRef
	{

		private string idField;

		private string nameField;
		[System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
		public string id {
			get { return this.idField; }
			set { this.idField = value; }
		}

		public string name {
			get { return this.nameField; }
			set { this.nameField = value; }
		}

	}

}
