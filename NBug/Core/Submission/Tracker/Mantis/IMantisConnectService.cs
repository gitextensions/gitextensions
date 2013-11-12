namespace NBug.Core.Submission.Tracker.Mantis
{

    [System.ServiceModel.ServiceContractAttribute(Namespace = "http://futureware.biz/mantisconnect", ConfigurationName = "MantisConnectPortType")]
    public interface IMantisConnectService
    {

        [System.ServiceModel.OperationContractAttribute(Action = "*", ReplyAction = "*"), System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
        string mc_version();

        [System.ServiceModel.OperationContractAttribute(Action = "*", ReplyAction = "*"), System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
        UserData mc_login(string username, string password);

        [System.ServiceModel.OperationContractAttribute(Action = "*", ReplyAction = "*"), System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "return"), System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
        string mc_issue_add(string username, string password, IssueData issue);

        [System.ServiceModel.OperationContractAttribute(Action = "*", ReplyAction = "*"), System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "return"), System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
        string mc_project_get_id_from_name(string username, string password, string project_name);

        [System.ServiceModel.OperationContractAttribute(Action = "*", ReplyAction = "*"), System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
        string[] mc_project_get_categories(
            string username, 
            string password,
            [System.Xml.Serialization.SoapElementAttribute(DataType = "integer")] string project_id
        );

        [System.ServiceModel.OperationContractAttribute(Action = "*", ReplyAction = "*"), System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "return"), System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
        string mc_issue_attachment_add(
            string username, 
            string password, 
            [System.Xml.Serialization.SoapElementAttribute(DataType = "integer")] string issue_id, 
            string name, 
            string file_type, 
            [System.Xml.Serialization.SoapElementAttribute(DataType = "base64Binary")]byte[] content
        );
        
        [System.ServiceModel.OperationContractAttribute(Action = "*", ReplyAction = "*"), System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
        ProjectVersionData[] mc_project_get_versions(
                    string username, 
                    string password, 
                    [System.Xml.Serialization.SoapElementAttribute(DataType = "integer")] string project_id);

        [System.ServiceModel.OperationContractAttribute(Action = "*", ReplyAction = "*"), System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "return"), System.Xml.Serialization.SoapElementAttribute(DataType = "integer")]
        string mc_project_version_add(string username, string password, ProjectVersionData version);

    }

}


