namespace NBug.Core.Submission.Tracker.Mantis
{
	[System.SerializableAttribute(), System.Xml.Serialization.SoapTypeAttribute(Namespace = "http://futureware.biz/mantisconnect")]
	public class UserData
	{

		private AccountData account_dataField;
		private string access_levelField;

		private string timezoneField;
		public AccountData account_data {
			get { return this.account_dataField; }
			set { this.account_dataField = value; }
		}

		[System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
		public string access_level {
			get { return this.access_levelField; }
			set { this.access_levelField = value; }
		}

		public string timezone {
			get { return this.timezoneField; }
			set { this.timezoneField = value; }
		}

	}

}
