// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AzureBlobStorageFactory.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Destinations.AzureBlobStorage
{
	using NBug.Core.Submission;

	public class AzureBlobStorageFactory : IProtocolFactory
	{
		public string SupportedType
		{
			get
			{
				return "AzureBlobStorage";
			}
		}

		public IProtocol FromConnectionString(string connectionString)
		{
			return new AzureBlobStorage(connectionString);
		}
	}
}