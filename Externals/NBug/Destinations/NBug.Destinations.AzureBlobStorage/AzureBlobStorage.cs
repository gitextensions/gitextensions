// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AzureBlobStorage.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Destinations.AzureBlobStorage
{
	using System;
	using System.IO;

	using Microsoft.WindowsAzure.Storage.Auth;
	using Microsoft.WindowsAzure.Storage.Blob;

	using NBug.Core.Reporting.Info;
	using NBug.Core.Submission;
	using NBug.Core.Util.Serialization;

	public class AzureBlobStorage : ProtocolBase
	{
		public AzureBlobStorage(string connectionString)
			: base(connectionString)
		{
		}

		public string AccountName { get; set; }

		public string ContainerName { get; set; }

		public string SharedAccessSignature { get; set; }

		// Connection string format (single line)
		// Warning: There should be no semicolon (;) or equals sign (=) used in any field except for password.
		// Warning: No field value value should contain the phrase 'password='
		// Note: SharedAccessUrl must be Uri.EscapeDataString()'ed

		/* Type=AzureBlobStorage;
		 * AccountName=youraccountname;
		 * ContainerName=yourcontainername;
		 * SharedAccessSignature=sr%3Dc%26si%3D16dacb22-asdf-asdf-asdf-e58fasdff3ed%26se%3D2098-12-31T23%253A00%253A00Z%26sig%3asdfIWtlasdfb98q0Kidp%252BasdffJcRm1ulFIjyks4E%253D
		 */
		public override bool Send(string fileName, Stream file, Report report, SerializableException exception)
		{
			var cred = new StorageCredentials(Uri.UnescapeDataString(this.SharedAccessSignature));

			var blobUrl = new Uri(string.Format("https://{0}.blob.core.windows.net/{1}", this.AccountName, this.ContainerName));
			var container = new CloudBlobContainer(blobUrl, cred);

			var blob = container.GetBlockBlobReference(fileName);

			file.Position = 0;
			blob.UploadFromStream(file);
			return true;
		}
	}
}