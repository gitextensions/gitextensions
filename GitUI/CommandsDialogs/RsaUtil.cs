using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
	public static class RsaUtil
	{
		/// <summary>
		/// RSA Public key in RFC-4716 The Secure Shell (SSH) Public Key File Format (4-byte length prefixed in PEM).
		/// </summary>
		/// <param name="rsaparams"></param>
		/// <returns></returns>
		public static string ExportPublicKeyOpenSsh(RSAParameters rsaparams)
		{
			var output = new MemoryStream();

			/*
string    "ssh-rsa"
mpint     e
mpint     n
*/

			// Algorithm
			string algo = "ssh-rsa";
			WriteLengthPrefixedValue(Encoding.ASCII.GetBytes(algo), output);

			// Public Exponent
			WriteLengthPrefixedValue(EnsureNonnegativeInteger(rsaparams.Exponent /* public exponent */), output);

			// Modulus
			WriteLengthPrefixedValue(EnsureNonnegativeInteger(rsaparams.Modulus), output);

			// Header, PEM
			var sb = new StringBuilder();
			sb.Append("ssh-rsa");
			sb.Append(' ');
			sb.Append(Convert.ToBase64String(output.ToArray(), 0));

			return sb.ToString();
		}

		/// <summary>
		/// RSA Public key in RFC-3447 (PKCS#1) in ASN.1 in DER in PEM.
		/// </summary>
		/// <param name="rsaparams"></param>
		/// <returns></returns>
		public static string ExportPublicKeyPkcs1(RSAParameters rsaparams)
		{
			// Make ASN.1 DER bytes

			/*
  RSAPublicKey ::= SEQUENCE 
  {
      modulus           INTEGER,  -- n
      publicExponent    INTEGER   -- e
  }
*/
			var seqcontent = new MemoryStream();

			// Modulus
			WriteDerTagLengthValueInteger(rsaparams.Modulus, seqcontent);

			// Public Exponent
			WriteDerTagLengthValueInteger(rsaparams.Exponent /* public exponent */, seqcontent);

			// Sequence out of these
			var sequence = new MemoryStream();
			seqcontent.WriteByte(0x30 /* SEQUENCE */);
			WriteDerLength((int)seqcontent.Length, sequence);
			seqcontent.Position = 0;
			seqcontent.WriteTo(sequence);

			// Header, PEM, footer
			var sb = new StringBuilder();
			sb.AppendLine("-----BEGIN RSA PUBLIC KEY-----");
			WritePem(sequence, sb);
			sb.AppendLine("-----END RSA PUBLIC KEY-----");

			return sb.ToString();
		}

		/// <summary>
		/// RSA Public key in RFC-4716 The Secure Shell (SSH) Public Key File Format (4-byte length prefixed in PEM).
		/// </summary>
		/// <param name="rsaparams"></param>
		/// <returns></returns>
		public static string ExportPublicKeySsh(RSAParameters rsaparams)
		{
			var output = new MemoryStream();

			/*
string    "ssh-rsa"
mpint     e
mpint     n
*/

			// Algorithm
			string algo = "ssh-rsa";
			WriteLengthPrefixedValue(Encoding.ASCII.GetBytes(algo), output);

			// Public Exponent
			WriteLengthPrefixedValue(EnsureNonnegativeInteger(rsaparams.Exponent /* public exponent */), output);

			// Modulus
			WriteLengthPrefixedValue(EnsureNonnegativeInteger(rsaparams.Modulus), output);

			// Header, PEM, footer
			var sb = new StringBuilder();
			sb.AppendLine("---- BEGIN SSH2 PUBLIC KEY ----");
			WritePem(output, sb);
			sb.AppendLine("---- END SSH2 PUBLIC KEY ----");

			return sb.ToString();
		}

		public static void TestGenerateKey()
		{
			var rsa = new RSACryptoServiceProvider(2048); // Doesn't actually generate the key pair
			RSAParameters rsaparams = rsa.ExportParameters(true);

			//		    Console.WriteLine(rsa.ToXmlString(true));
			Console.WriteLine(rsa.ToXmlString(false));
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine(ExportPublicKeyPkcs1(rsaparams));
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine(ExportPublicKeySsh(rsaparams));
			Console.WriteLine();
			Console.WriteLine();
			string openssh = ExportPublicKeyOpenSsh(rsaparams);
			Console.WriteLine(openssh);
			Clipboard.SetText(openssh);

			Console.WriteLine("Done.");
		}

		/// <summary>
		/// Negative integers are two's complement; by convention, they're detected by the most significant bit of the first byte. To make sure the number is treated as positive, we must prefix it with a zero byte if so.
		/// </summary>
		[NotNull]
		private static byte[] EnsureNonnegativeInteger([NotNull] byte[] bigint)
		{
			if(bigint == null)
				throw new ArgumentNullException(nameof(bigint));
			if(bigint.Length == 0)
				return bigint;
			if((bigint[0] & 0x80) == 0)
				return bigint;
			var prefixed = new byte[bigint.Length + 1];
			Buffer.BlockCopy(bigint, 0, prefixed, 1, bigint.Length);
			return prefixed;
		}

		private static void WriteDerLength(int nDataLen, Stream output)
		{
			if(nDataLen < 0x80)
				output.WriteByte((byte)nDataLen);
			else
			{
				byte[] lenbytes = new BigInteger(nDataLen).ToByteArray();
				output.WriteByte((byte)(0x80 /* writing length of the length */| (byte)lenbytes.Length));
				for(int a = 0; a < lenbytes.Length; a++)
					output.WriteByte(lenbytes[a]);
			}
		}

		private static void WriteDerTagLengthValueInteger([NotNull] byte[] bytes, [NotNull] Stream output)
		{
			if(bytes == null)
				throw new ArgumentNullException(nameof(bytes));
			if(bytes.Length == 0)
				throw new ArgumentOutOfRangeException(nameof(bytes), bytes, "Shan't be empty.");
			if(output == null)
				throw new ArgumentNullException(nameof(output));

			// Tag
			output.WriteByte(0x02 /* INTEGER */);

			// Length
			bool isZeroPadding = (bytes[0] & 0x80) != 0;
			WriteDerLength(bytes.Length + (isZeroPadding ? 1 : 0), output);

			// Value
			if(isZeroPadding)
				output.WriteByte(0);
			output.Write(bytes, 0, bytes.Length);
		}

		private static void WriteLengthPrefixedValue([NotNull] byte[] bytes, [NotNull] Stream output)
		{
			if(bytes == null)
				throw new ArgumentNullException(nameof(bytes));
			if(output == null)
				throw new ArgumentNullException(nameof(output));

			int len = bytes.Length;
			output.WriteByte((byte)((len >> 24) & 0xFF));
			output.WriteByte((byte)((len >> 16) & 0xFF));
			output.WriteByte((byte)((len >> 08) & 0xFF));
			output.WriteByte((byte)((len >> 00) & 0xFF));

			output.Write(bytes, 0, len);
		}

		private static void WritePem([NotNull] Stream input, [NotNull] StringBuilder output)
		{
			if(input == null)
				throw new ArgumentNullException(nameof(input));
			if(output == null)
				throw new ArgumentNullException(nameof(output));
			input.Position = 0;
			var buffer = new byte[48]; // 48 octets => 384 bits => 64 sextets, the required PEM line length (Convert::ToBase64String makes them 76 somehow)
			int nRead;
			while((nRead = input.Read(buffer, 0, buffer.Length)) > 0)
				output.AppendLine(Convert.ToBase64String(buffer, 0, nRead, 0));
		}
	}
}