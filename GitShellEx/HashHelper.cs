using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Reflection;
using System.ComponentModel;
namespace FileHashShell
{
    public class HashHelper
    {

        public enum HashType
        {
            [Description("SHA-1")]
            SHA1,
            [Description("SHA-256")]
            SHA256,
            [Description("SHA-384")]
            SHA384,
            [Description("SHA-512")]
            SHA512,
            [Description("MD5")]
            MD5,
            [Description("RIPEMD-160")]
            RIPEMD160
           
        }

        public static string GetFileHash(string filePath)
        {
           return  GetFileHash(filePath, HashType.SHA1);
        }
        public static string GetFileHash(string filePath, HashType type)
        {
            if (!File.Exists(filePath))
                return string.Empty;

            System.Security.Cryptography.HashAlgorithm hasher;
            switch(type)
            {
                case HashType.SHA1:
                default:
                    hasher = new SHA1CryptoServiceProvider();
                    break;
                case HashType.SHA256:
                    hasher = new SHA256Managed();
                    break;
                case HashType.SHA384:
                    hasher = new SHA384Managed();
                    break;
                case HashType.SHA512:
                    hasher = new SHA512Managed();
                    break;
                case HashType.MD5:
                    hasher = new MD5CryptoServiceProvider();
                    break;
                case HashType.RIPEMD160:
                    hasher = new RIPEMD160Managed();
                    break;
            }
            StringBuilder buff = new StringBuilder();
            try
            {
                using (FileStream f = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192))
                {
                    hasher.ComputeHash(f);
                    Byte[] hash = hasher.Hash;
                    foreach (Byte hashByte in hash)
                    {
                        buff.Append(string.Format("{0:x2}", hashByte));
                    }
                }
            }
            catch
            {
                return "Error reading file." + new System.Random(DateTime.Now.Second * DateTime.Now.Millisecond).Next().ToString();
            }
            return buff.ToString();

        }
        public static string GetDescription(Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return en.ToString();
        }


    }
}
