namespace NBug.Core.Submission.Tracker.Mantis
{

	[System.SerializableAttribute(), System.Xml.Serialization.SoapTypeAttribute(Namespace = "http://futureware.biz/mantisconnect")]
	public class IssueData
	{


		private string idField;

		private ObjectRef view_stateField;

		private System.DateTime last_updatedField;

		private bool last_updatedFieldSpecified;

		private ObjectRef projectField;

		private string categoryField;

		private ObjectRef priorityField;

		private ObjectRef severityField;

		private ObjectRef statusField;

		private AccountData reporterField;

		private string summaryField;

		private string versionField;

		private string buildField;

		private string platformField;

		private string osField;

		private string os_buildField;

		private ObjectRef reproducibilityField;

		private System.DateTime date_submittedField;

		private bool date_submittedFieldSpecified;

		private string sponsorship_totalField;

		private AccountData handlerField;

		private ObjectRef projectionField;

		private ObjectRef etaField;

		private ObjectRef resolutionField;

		private string fixed_in_versionField;

		private string target_versionField;

		private string descriptionField;

		private string steps_to_reproduceField;

		private string additional_informationField;

		private AttachmentData[] attachmentsField;

		private RelationshipData[] relationshipsField;

		private IssueNoteData[] notesField;

		private CustomFieldValueForIssueData[] custom_fieldsField;

		private System.DateTime due_dateField;

		private bool due_dateFieldSpecified;

		private AccountData[] monitorsField;

		private bool stickyField;

		private bool stickyFieldSpecified;

		private ObjectRef[] tagsField;

		[System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
		public string id {
			get { return this.idField; }
			set { this.idField = value; }
		}


		public ObjectRef view_state {
			get { return this.view_stateField; }
			set { this.view_stateField = value; }
		}


		public System.DateTime last_updated {
			get { return this.last_updatedField; }
			set { this.last_updatedField = value; }
		}


		[System.Xml.Serialization.SoapIgnoreAttribute()]
		public bool last_updatedSpecified {
			get { return this.last_updatedFieldSpecified; }
			set { this.last_updatedFieldSpecified = value; }
		}


		public ObjectRef project {
			get { return this.projectField; }
			set { this.projectField = value; }
		}


		public string category {
			get { return this.categoryField; }
			set { this.categoryField = value; }
		}


		public ObjectRef priority {
			get { return this.priorityField; }
			set { this.priorityField = value; }
		}


		public ObjectRef severity {
			get { return this.severityField; }
			set { this.severityField = value; }
		}


		public ObjectRef status {
			get { return this.statusField; }
			set { this.statusField = value; }
		}


		public AccountData reporter {
			get { return this.reporterField; }
			set { this.reporterField = value; }
		}


		public string summary {
			get { return this.summaryField; }
			set { this.summaryField = value; }
		}


		public string version {
			get { return this.versionField; }
			set { this.versionField = value; }
		}


		public string build {
			get { return this.buildField; }
			set { this.buildField = value; }
		}


		public string platform {
			get { return this.platformField; }
			set { this.platformField = value; }
		}


		public string os {
			get { return this.osField; }
			set { this.osField = value; }
		}


		public string os_build {
			get { return this.os_buildField; }
			set { this.os_buildField = value; }
		}


		public ObjectRef reproducibility {
			get { return this.reproducibilityField; }
			set { this.reproducibilityField = value; }
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


		[System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
		public string sponsorship_total {
			get { return this.sponsorship_totalField; }
			set { this.sponsorship_totalField = value; }
		}


		public AccountData handler {
			get { return this.handlerField; }
			set { this.handlerField = value; }
		}


		public ObjectRef projection {
			get { return this.projectionField; }
			set { this.projectionField = value; }
		}


		public ObjectRef eta {
			get { return this.etaField; }
			set { this.etaField = value; }
		}


		public ObjectRef resolution {
			get { return this.resolutionField; }
			set { this.resolutionField = value; }
		}


		public string fixed_in_version {
			get { return this.fixed_in_versionField; }
			set { this.fixed_in_versionField = value; }
		}


		public string target_version {
			get { return this.target_versionField; }
			set { this.target_versionField = value; }
		}


		public string description {
			get { return this.descriptionField; }
			set { this.descriptionField = value; }
		}


		public string steps_to_reproduce {
			get { return this.steps_to_reproduceField; }
			set { this.steps_to_reproduceField = value; }
		}


		public string additional_information {
			get { return this.additional_informationField; }
			set { this.additional_informationField = value; }
		}


		public AttachmentData[] attachments {
			get { return this.attachmentsField; }
			set { this.attachmentsField = value; }
		}


		public RelationshipData[] relationships {
			get { return this.relationshipsField; }
			set { this.relationshipsField = value; }
		}


		public IssueNoteData[] notes {
			get { return this.notesField; }
			set { this.notesField = value; }
		}


		public CustomFieldValueForIssueData[] custom_fields {
			get { return this.custom_fieldsField; }
			set { this.custom_fieldsField = value; }
		}


		public System.DateTime due_date {
			get { return this.due_dateField; }
			set { this.due_dateField = value; }
		}


		[System.Xml.Serialization.SoapIgnoreAttribute()]
		public bool due_dateSpecified {
			get { return this.due_dateFieldSpecified; }
			set { this.due_dateFieldSpecified = value; }
		}


		public AccountData[] monitors {
			get { return this.monitorsField; }
			set { this.monitorsField = value; }
		}


		public bool sticky {
			get { return this.stickyField; }
			set { this.stickyField = value; }
		}


		[System.Xml.Serialization.SoapIgnoreAttribute()]
		public bool stickySpecified {
			get { return this.stickyFieldSpecified; }
			set { this.stickyFieldSpecified = value; }
		}


		public ObjectRef[] tags {
			get { return this.tagsField; }
			set { this.tagsField = value; }
		}
	}

}
