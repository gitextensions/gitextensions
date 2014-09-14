namespace NBug.Core.Submission.Tracker.DoctorDump
{
    using NBug.Core.Reporting.Info;
    using NBug.Core.Util.Serialization;
    using NBug.DoctorDump;
	using System;
	using System.Collections.Generic;
    using System.Globalization;
	using System.Net;
	using System.ServiceModel;
	using System.ServiceModel.Channels;

    internal class AnonymousData
    {
        public Report Report { get; set; }
        public SerializableException Exception { get; set; }
        public string ApplicationGUID { get; set; }
        public string ToEmail { get; set; }

        public AnonymousData(string applicationGUID, string email, SerializableException exception, Report report)
        {
            Report = report;
            Exception = exception;
            ApplicationGUID = applicationGUID;
            ToEmail = email;
        }

        private static string GetOsVersionDoctorDumpFormat()
        {
            var osVer = Environment.OSVersion;
            string os = string.Format("os={0};v={1};spname={2}", osVer.Platform.ToString(), osVer.Version.ToString(), osVer.ServicePack);
            return os;
        }

        static private System.Net.NetworkInformation.PhysicalAddress GetMacAddress()
        {
            var googleDns = new System.Net.Sockets.UdpClient("8.8.8.8", 53);
            IPAddress localAddr = ((IPEndPoint)googleDns.Client.LocalEndPoint).Address;

            foreach (var netInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (var addr in netInterface.GetIPProperties().UnicastAddresses)
                {
                    if (addr.Address.Equals(localAddr))
                        return netInterface.GetPhysicalAddress();
                }
            }
            return null;
        }

        private int GetAnonymousMachineId()
        {
            System.Net.NetworkInformation.PhysicalAddress mac = GetMacAddress();
            return mac != null ? BitConverter.ToInt32(System.Security.Cryptography.MD5.Create().ComputeHash(mac.GetAddressBytes()), 0) : 0;
        }

        private static int GetHResult(SerializableException e)
        {
            if (e.ExtendedInformation == null || !e.ExtendedInformation.ContainsKey("HResult"))
                return 0;
            object hresult = e.ExtendedInformation["HResult"];
            if (hresult is int) // before serialization it is int
                return (int)hresult;
            if (hresult is string) // after deserialization it is string
                return int.Parse((string)hresult);
            return 0;
        }

        static private ExceptionInfo GetExceptionInfo(SerializableException e, bool anonymous)
        {
            return e == null ? null : new ExceptionInfo
            {
                Type = e.Type,
                HResult = GetHResult(e),
                StackTrace = e.StackTrace ?? string.Empty,
                Source = e.Source,
                Message = anonymous ? null : e.Message,
                InnerException = GetExceptionInfo(e.InnerException ?? (e.InnerExceptions != null && e.InnerExceptions.Count > 0 ? e.InnerExceptions[0] : null), anonymous)
            };
        }

        public ExceptionDescription GetExceptionDescription(bool anonymous)
        {
            return new ExceptionDescription
            {
                ClrVersion = Report.GeneralInfo.CLRVersion,
                OS = GetOsVersionDoctorDumpFormat(),
                CrashDate = Report.GeneralInfo.DateTime,
                PCID = GetAnonymousMachineId(),
                Exception = GetExceptionInfo(Exception, anonymous)
            };
        }

        public Application GetApplication()
        {
            Guid? appGuid = null;
            if (!string.IsNullOrEmpty(ApplicationGUID))
                appGuid = new Guid(ApplicationGUID);

            var mainAssembly = System.Reflection.Assembly.GetEntryAssembly();

            var companyAttributes = mainAssembly.GetCustomAttributes(typeof(System.Reflection.AssemblyCompanyAttribute), true);
            var companyFromResource = (companyAttributes.Length > 0) ? ((System.Reflection.AssemblyCompanyAttribute)companyAttributes[0]).Company : null;
            string company = string.IsNullOrWhiteSpace(companyFromResource) ? ToEmail : companyFromResource;

            var titleAttributes = mainAssembly.GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), true);
            var titleFromResource = (titleAttributes.Length > 0) ? ((System.Reflection.AssemblyTitleAttribute)titleAttributes[0]).Title : null;
            string appTitle = string.IsNullOrWhiteSpace(titleFromResource) ? mainAssembly.GetName().Name : titleFromResource;

            var appVersion = new Version(Report.GeneralInfo.HostApplicationVersion);
            
            return new Application
            {
                ApplicationGUID = appGuid,
                AppName = appTitle,
                CompanyName = company,
                Email = ToEmail,
                V1 = (ushort)appVersion.Major,
                V2 = (ushort)appVersion.Minor,
                V3 = (ushort)appVersion.Build,
                V4 = (ushort)appVersion.Revision,
                MainModule = Report.GeneralInfo.HostApplication
            };
        }

        public ClientLib GetClientLib()
        {
            var version = new Version(Report.GeneralInfo.NBugVersion);
            return new ClientLib
            {
                V1 = (ushort)version.Major,
                V2 = (ushort)version.Minor,
                V3 = (ushort)version.Build,
                V4 = (ushort)version.Revision
            };
        }
    }

    internal class PrivateData : AnonymousData
    {
        byte[] ZipReport { get; set; }

        public PrivateData(string applicationGUID, string email, System.IO.Stream file, SerializableException exception, Report report)
            : base(applicationGUID, email, exception, report)
        {
            var buf = new byte[file.Length];
            file.Read(buf, 0, buf.Length);
            ZipReport = buf;
        }

        public DetailedExceptionDescription GetDetailedExceptionDescription()
        {
            return new DetailedExceptionDescription
            {
                Exception = GetExceptionDescription(false),
                UserDescription = Report.GeneralInfo.UserDescription,
                Report = ZipReport
            };
        }
    }

    internal class DoctorDumpService
    {
        public DoctorDumpService()
        {
            _uploader = CreateService();
        }

        private static IdolSoftwareDoctorDumpNBugGateINBugReportUploaderClient CreateService()
        {
            var serviceUrl = new System.UriBuilder("https://drdump.com/Service/NBugReportUploader.svc");

            string configOverride = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Idol Software\DumpUploader", "ServiceURL", null) as string;
            if (!string.IsNullOrEmpty(configOverride))
            {
                var config = new System.Uri(configOverride);
                serviceUrl.Scheme = config.Scheme;
                serviceUrl.Host = config.Host;
                serviceUrl.Port = config.Port;
            }

            var mtomSoap12 = new MtomMessageEncodingBindingElement(MessageVersion.Soap12, System.Text.Encoding.UTF8);
            mtomSoap12.ReaderQuotas.MaxArrayLength = 1024 * 1024 * 1024;
            BindingElement https = serviceUrl.Scheme != "http"
                ? new HttpsTransportBindingElement() { MaxReceivedMessageSize = 1024 * 1024 * 1024 }
                : new HttpTransportBindingElement() { MaxReceivedMessageSize = 1024 * 1024 * 1024 };
            var endpoint = new EndpointAddress(serviceUrl.ToString());
            var binding = new CustomBinding(mtomSoap12, https);

            return new IdolSoftwareDoctorDumpNBugGateINBugReportUploaderClient(binding, new EndpointAddress(serviceUrl.Uri));
        }

        public Response SendAnonymousReport(string applicationGUID, string email, SerializableException exception, Report report)
        {
            var anonymousData = new AnonymousData(applicationGUID, email, exception, report);
            return _uploader.SendAnonymousReport(
                anonymousData.GetClientLib(),
                anonymousData.GetApplication(),
                anonymousData.GetExceptionDescription(anonymous: true));
        }

        public Response SendRequest(byte[] context, string applicationGUID, string email, System.IO.Stream file, SerializableException exception, Report report)
        {
            var data = new PrivateData(applicationGUID, email, file, exception, report);
            return _uploader.SendAdditionalData(context, data.GetDetailedExceptionDescription());
        }

        private IdolSoftwareDoctorDumpNBugGateINBugReportUploaderClient _uploader;
    }
}
