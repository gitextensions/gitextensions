// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StreamUpload.cs" company="NBusy Project">
//   Originates from: http://www.andrescottwilson.com/posting-content-from-memory-stream-using-httpwebrequest-c/
//   Changed for NBug by Michal Turecki
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace NBug.Core.Util.Web
{
	public class StreamUpload
	{
		private readonly string boundary = "";
		private ICredentials credentials;
		private MemoryStream outputStream = new MemoryStream();
		private WebResponse response;

		public static StreamUpload Create()
		{
			return new StreamUpload();
		}

		public StreamUpload()
		{
			boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
		}

		public StreamUpload WithCredentials(ICredentials credentials)
		{
			this.credentials = credentials;
			return this;
		}

		public StreamUpload Upload(string url)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
			request.Method = "POST";

			if (credentials != null)
			{
				var user = credentials.GetCredential(request.RequestUri, "Basic");
				string auth = string.Format("{0}:{1}", user.UserName, user.Password);
				request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(auth)));
			}

			WriteToHttpStream(request, outputStream);
			response = request.GetResponse();
			return this;
		}

		public StreamUpload Add(string name, string value)
		{
			WriteBoundaryToStream(outputStream, Environment.NewLine);
			WriteToStream(outputStream, Encoding.UTF8, string.Format("Content-Disposition: form-data; name=\"{0}\"{1}{1}", name, Environment.NewLine));
			WriteToStream(outputStream, Encoding.UTF8, value + Environment.NewLine);
			return this;
		}

		public StreamUpload AddNameValues(NameValueCollection nameValues)
		{
			foreach (string name in nameValues.Keys)
			{
				Add(name, nameValues[name]);
			}
			return this;
		}

		public StreamUpload Add(Stream inputStream)
		{
			return Add(inputStream, "form", "file", "application/octet-stream");
		}

		public StreamUpload Add(Stream inputStream, string formName, string fileName, string contentType)
		{
			// write content boundary start
			WriteBoundaryToStream(outputStream, Environment.NewLine);

			WriteToStream(outputStream, System.Text.Encoding.UTF8, string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", formName, fileName, Environment.NewLine));
			WriteToStream(outputStream, System.Text.Encoding.UTF8, string.Format("Content-Type: {0}{1}{1}", contentType, Environment.NewLine));

			byte[] buffer = new byte[inputStream.Length];
			int bytesRead = 0;

			while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) != 0)
			{
				outputStream.Write(buffer, 0, bytesRead);
			}

			// must include a new line before writing the end boundary
			WriteToStream(outputStream, Encoding.ASCII, Environment.NewLine);

			// make sure we end boundary now as the content is finished
			WriteEndBoundaryToStream(outputStream);

			return this;
		}

		public string Response()
		{
			return (response == null) ? "" : new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
		}

		private void WriteEndBoundaryToStream(MemoryStream stream)
		{
			WriteBoundaryToStream(stream, "--");
		}

		private void WriteBoundaryToStream(MemoryStream stream, string endDeliminator)
		{
			WriteToStream(stream, Encoding.ASCII, string.Format("--{0}{1}", boundary, endDeliminator));
		}

		private void WriteNameValuesToStream(MemoryStream stream, NameValueCollection nameValues)
		{
			foreach (string name in nameValues.Keys)
			{
				WriteBoundaryToStream(stream, Environment.NewLine);

				WriteToStream(stream, Encoding.UTF8, string.Format("Content-Disposition: form-data; name=\"{0}\"{1}{1}", name, Environment.NewLine));
				WriteToStream(stream, Encoding.UTF8, nameValues[name] + Environment.NewLine);
			}
		}

		private void WriteToHttpStream(HttpWebRequest request, MemoryStream outputStream)
		{
			request.ContentLength = outputStream.Length;

			using (Stream requestStream = request.GetRequestStream())
			{
				outputStream.Position = 0;

				byte[] tempBuffer = new byte[outputStream.Length];
				outputStream.Read(tempBuffer, 0, tempBuffer.Length);
				outputStream.Close();

				requestStream.Write(tempBuffer, 0, tempBuffer.Length);
				requestStream.Close();
			}
		}

		private void WriteToStream(MemoryStream stream, Encoding encoding, string output)
		{
			byte[] headerbytes = encoding.GetBytes(output);
			stream.Write(headerbytes, 0, headerbytes.Length);
		}

		public StreamUpload Clear()
		{
			outputStream.Close();
			response = null;
			return this;
		}
	}
}