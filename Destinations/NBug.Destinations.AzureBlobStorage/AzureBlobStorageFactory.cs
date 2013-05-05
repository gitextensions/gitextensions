namespace NBug.Destinations.AzureBlobStorage
{
	using NBug.Core.Submission;

	public class AzureBlobStorageFactory : IProtocolFactory
	{
		public IProtocol FromConnectionString(string connectionString)
		{
			return new AzureBlobStorage(connectionString);
		}

		public string SupportedType
		{
			get { return "AzureBlobStorage"; }
		}
	}
}