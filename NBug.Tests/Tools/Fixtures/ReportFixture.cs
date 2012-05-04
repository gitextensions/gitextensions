// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportFixture.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Tests.Tools.Fixtures
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml.Linq;
	using System.Xml.Serialization;

	using NBug.Core.Reporting.Info;
	using NBug.Core.Util.Serialization;
	using NBug.Core.Util.Storage;
	using NBug.Enums;
	using NBug.Tests.Tools.Stubs;

	using Xunit;

	public class ReportFixture : IDisposable
	{
		public ReportFixture()
		{
			this.DeleteGarbageReportFile();
			this.SerializableException = new SerializableException(new DummyArgumentException());
			this.Report = new Report(this.SerializableException);
		}

		public Report Report { get; set; }

		public SerializableException SerializableException { get; set; }

		public void Dispose()
		{
			this.DeleteGarbageReportFile();
		}

		internal void DeleteGarbageReportFile()
		{
			using (var storer = new Storer())
			using (var stream = storer.GetFirstReportFile())
			{
				if (stream != null)
				{
					storer.DeleteCurrentReportFile();
				}
			}
		}

		internal void VerifyAndDeleteCompressedReportFile(bool verifyCustomReport)
		{
			using (var storer = new Storer())
			using (var stream = storer.GetFirstReportFile())
			{
				Assert.NotNull(stream);
				this.VerifyIndividualReportItems(stream, verifyCustomReport);
				storer.DeleteCurrentReportFile();
			}
		}

		internal void VerifyAndDeleteCompressedReportFile()
		{
			this.VerifyAndDeleteCompressedReportFile(false);
		}

		// This is not static for future performance improvements (like totally eliminating calls to Storer object)
		private void VerifyIndividualReportItems(Stream reportFile, bool verifyCustomReport)
		{
			var zipStorer = ZipStorer.Open(reportFile, FileAccess.Read);
			using (var zipItemStream = new MemoryStream())
			{
				SerializableException serializableException = null;
				Report report = null;
				uint fileSize = 0;
				var zipDirectory = zipStorer.ReadCentralDir();

				foreach (var entry in zipDirectory)
				{
					if (Path.GetFileName(entry.FilenameInZip) == StoredItemFile.Exception)
					{
						zipItemStream.SetLength(0);
						zipStorer.ExtractFile(entry, zipItemStream);
						zipItemStream.Position = 0;
						var deserializer = new XmlSerializer(typeof(SerializableException));
						serializableException = (SerializableException)deserializer.Deserialize(zipItemStream);
						zipItemStream.Position = 0;
						Assert.NotNull(XElement.Load(zipItemStream));
					}
					else if (Path.GetFileName(entry.FilenameInZip) == StoredItemFile.Report)
					{
						zipItemStream.SetLength(0);
						zipStorer.ExtractFile(entry, zipItemStream);
						zipItemStream.Position = 0;
						var deserializer = new XmlSerializer(typeof(Report));
						report = (Report)deserializer.Deserialize(zipItemStream);
						zipItemStream.Position = 0;
						Assert.NotNull(XElement.Load(zipItemStream));
						if (verifyCustomReport)
						{
							Assert.NotNull(report.CustomInfo);
						}
					}
					else if (Path.GetFileName(entry.FilenameInZip) == StoredItemFile.MiniDump)
					{
						fileSize = entry.FileSize;
					}
					else
					{
						Assert.True(false, "A new file type has been added to the compressed report file. A new validation check for the file should be added.");
					}
				}

				Assert.NotNull(serializableException);
				Assert.NotNull(report);

				if (Settings.MiniDumpType != MiniDumpType.None)
				{
					Assert.True(fileSize != 0);
				}
			}
		}
	}
}
