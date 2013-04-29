using System;

namespace NBug.Core.Submission.Web
{
    using System.IO;

    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;

    internal class AzureBlobStorage : Protocol
    {
        internal AzureBlobStorage(string connectionString) 
            : base(connectionString, Protocols.AzureBlobStorage)
        {
        }

        internal AzureBlobStorage(string connectionString, Stream reportFile) 
            : base(connectionString, reportFile, Protocols.AzureBlobStorage)
        {
        }

        internal AzureBlobStorage() 
            : base(Protocols.AzureBlobStorage)
        {
        }

        public string AccountName { get; set; }
        public string SharedAccessSignature { get; set; }
        public string ContainerName { get; set; }

        // Connection string format (single line)
        // Warning: There should be no semicolon (;) or equals sign (=) used in any field except for password.
        // Warning: No field value value should contain the phrase 'password='
        // Note: SharedAccessUrl must be Uri.EscapeDataString()'ed

        /* Type=AzureBlobStorage;
         * AccountName=youraccountname;
         * ContainerName=yourcontainername;
         * SharedAccessSignature=sr%3Dc%26si%3D16dacb22-asdf-asdf-asdf-e58fasdff3ed%26se%3D2098-12-31T23%253A00%253A00Z%26sig%3asdfIWtlasdfb98q0Kidp%252BasdffJcRm1ulFIjyks4E%253D
         */

        internal bool Send()
        {
            var cred = new StorageCredentials(Uri.UnescapeDataString(SharedAccessSignature));

            var blobUrl = new Uri(string.Format("https://{0}.blob.core.windows.net/{1}", AccountName, ContainerName));
            var container = new CloudBlobContainer(blobUrl, cred);
            var fileName = Path.GetFileName(((FileStream) this.ReportFile).Name);

            var blob = container.GetBlockBlobReference(fileName);

            this.ReportFile.Position = 0;
            blob.UploadFromStream(this.ReportFile);
            return true;
        }
    }
}
