namespace NBug.Core.Submission.Tracker.Mantis
{

    [System.SerializableAttribute(), 
    System.Xml.Serialization.SoapTypeAttribute(Namespace = "http://futureware.biz/mantisconnect")]
    public class ProjectVersionData
    {

        private string idField;

        private string nameField;

        private string project_idField;

        private System.DateTime date_orderField;

        private bool date_orderFieldSpecified;

        private string descriptionField;

        private bool releasedField;

        private bool releasedFieldSpecified;

        private bool obsoleteField;

        private bool obsoleteFieldSpecified;

        [System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
        public string id
        {
            get { return this.idField; }
            set { this.idField = value; }
        }

        public string name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }

        [System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
        public string project_id
        {
            get { return this.project_idField; }
            set { this.project_idField = value; }
        }

        public System.DateTime date_order
        {
            get { return this.date_orderField; }
            set { this.date_orderField = value; }
        }

        [System.Xml.Serialization.SoapIgnoreAttribute()]
        public bool date_orderSpecified
        {
            get { return this.date_orderFieldSpecified; }
            set { this.date_orderFieldSpecified = value; }
        }

        public string description
        {
            get { return this.descriptionField; }
            set { this.descriptionField = value; }
        }

        public bool released
        {
            get { return this.releasedField; }
            set { this.releasedField = value; }
        }

        [System.Xml.Serialization.SoapIgnoreAttribute()]
        public bool releasedSpecified
        {
            get { return this.releasedFieldSpecified; }
            set { this.releasedFieldSpecified = value; }
        }

        public bool obsolete
        {
            get { return this.obsoleteField; }
            set { this.obsoleteField = value; }
        }

        [System.Xml.Serialization.SoapIgnoreAttribute()]
        public bool obsoleteSpecified
        {
            get { return this.obsoleteFieldSpecified; }
            set { this.obsoleteFieldSpecified = value; }
        }
    }
}


