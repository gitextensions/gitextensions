using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
	/// <summary>
	/// RSA key formats manip.
	/// </summary>
	public static class RsaUtil
	{
		/// <summary>
		///  RSA Private Key in RFC-3447 RSAPrivateKey format.
		/// </summary>
		/// <remarks>
		///     <code>
		///       RSAPrivateKey ::= SEQUENCE {
		///           version           Version,
		///           modulus           INTEGER,  -- n
		///           publicExponent    INTEGER,  -- e
		///           privateExponent   INTEGER,  -- d
		///           prime1            INTEGER,  -- p
		///           prime2            INTEGER,  -- q
		///           exponent1         INTEGER,  -- d mod (p-1)
		///           exponent2         INTEGER,  -- d mod (q-1)
		///           coefficient       INTEGER,  -- (inverse of q) mod p
		///           otherPrimeInfos   OtherPrimeInfos OPTIONAL
		///       }
		/// 	
		///          OtherPrimeInfos ::= SEQUENCE SIZE(1..MAX) OF OtherPrimeInfo
		/// 
		///          OtherPrimeInfo ::= SEQUENCE {
		///              prime             INTEGER,  -- ri
		///              exponent          INTEGER,  -- di
		///              coefficient       INTEGER   -- ti
		///          }
		/// 
		///    The fields of type RSAPrivateKey have the following meanings:
		/// 
		///     * version is the version number, for compatibility with future
		///       revisions of this document.  It shall be 0 for this version of the
		///       document, unless multi-prime is used, in which case it shall be 1.
		/// 
		///             Version ::= INTEGER { two-prime(0), multi(1) }
		///                (CONSTRAINED BY
		///                {-- version must be multi if otherPrimeInfos present --})
		/// 
		///     * modulus is the RSA modulus n.
		/// 
		///     * publicExponent is the RSA public exponent e.
		/// 
		///     * privateExponent is the RSA private exponent d.
		/// 
		///     * prime1 is the prime factor p of n.
		/// 
		///     * prime2 is the prime factor q of n.
		/// 
		///     * exponent1 is d mod (p - 1).
		/// 
		///     * exponent2 is d mod (q - 1).
		/// 
		///     * coefficient is the CRT coefficient q^(-1) mod p.
		/// 
		///     * otherPrimeInfos contains the information for the additional primes
		///       r_3, ..., r_u, in order.  It shall be omitted if version is 0 and
		///       shall contain at least one instance of OtherPrimeInfo if version
		///       is 1.
		/// 
		///    The fields of type OtherPrimeInfo have the following meanings:
		/// 
		///     * prime is a prime factor r_i of n, where i >= 3.
		/// 
		///     * exponent is d_i = d mod (r_i - 1).
		/// 
		///     * coefficient is the CRT coefficient t_i = (r_1 * r_2 * ... * r_(i-
		///       1))^(-1) mod r_i.
		/// 	 
		/// 		
		///  </code>
		/// </remarks>
		public static string ExportPrivateKeyRSAPrivateKey(RSAParameters rsaparams)
		{
			// Write the ASN.1 structure in DER byte encoding

			// First, sequence members to know their length
			var derParams = new MemoryStream();

			// Version: v0 for only two primes in the modulus
			WriteDerIntegerTlvTriplet(0, derParams);

			// Modulus
			WriteDerIntegerTlvTriplet(rsaparams.Modulus, derParams);

			// Public exponent
			WriteDerIntegerTlvTriplet(rsaparams.Exponent /* yep that's public */, derParams);

			// Private exponent
			WriteDerIntegerTlvTriplet(rsaparams.D, derParams);

			// Prime #1
			WriteDerIntegerTlvTriplet(rsaparams.P, derParams);

			// Prime #2
			WriteDerIntegerTlvTriplet(rsaparams.Q, derParams);

			// Exponent #1
			WriteDerIntegerTlvTriplet(rsaparams.DP, derParams);

			// Exponent #2
			WriteDerIntegerTlvTriplet(rsaparams.DQ, derParams);

			// Coefficient
			WriteDerIntegerTlvTriplet(rsaparams.InverseQ, derParams);

			// Now wrap into a sequence
			var derRoot = new MemoryStream();
			derRoot.WriteByte(0x30 /* sequence */);
			WriteDerLength((int)derParams.Length, derRoot);
			derParams.Position = 0;
			derParams.WriteTo(derRoot);

			// TODO: remove
			File.WriteAllBytes("T:\\rsapk.der", derRoot.ToArray());
			// TODO: remove

			// Header, PEM, footer
			var sb = new StringBuilder();
			sb.AppendLine("-----BEGIN RSA PRIVATE KEY-----");
			WritePem(derRoot, sb);
			sb.AppendLine("-----END RSA PRIVATE KEY-----");

			return sb.ToString();
		}

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
			var derParams = new MemoryStream();

			// Modulus
			WriteDerIntegerTlvTriplet(rsaparams.Modulus, derParams);

			// Public Exponent
			WriteDerIntegerTlvTriplet(rsaparams.Exponent /* public exponent */, derParams);

			// Sequence out of these
			var derRoot = new MemoryStream();
			derParams.WriteByte(0x30 /* SEQUENCE */);
			WriteDerLength((int)derParams.Length, derRoot);
			derParams.Position = 0;
			derParams.WriteTo(derRoot);

			// Header, PEM, footer
			var sb = new StringBuilder();
			sb.AppendLine("-----BEGIN RSA PUBLIC KEY-----");
			WritePem(derRoot, sb);
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

		public static void TestExportKey()
		{
			var rsa = new RSACryptoServiceProvider(2048);
			rsa.FromXmlString("<RSAKeyValue><Modulus>u0Tq1PpgaGUJOwcquucXQdKnkvuEAPRANk8NqkUy9LgqLRnSIJ7XL7xOY0Z2rAbynf+8v6ZugynReZRHuZnxpH1R9lZEn4lu4PZUwI74YZ0OrM0V1I940Tihr9cNn6lAhShy09Bq9MwB5fuT8wUIQj23gnJ/5QSED4jJiYS68vYFDpN+Tq02Vu3KCYohIpaww23XlWDRogWoKw6DvQQDybL22s908/R28/WLQtMrBJz89yh+LYiKlCrepV5HG/utvCdU/W0zKxOtu2wVS/7Xe43qDhDyqPB0c/bQyebhghNBVwLST8AwUV7opQBPgiOxs/tlwkmdXglgpyYph554Bw==</Modulus><Exponent>AQAB</Exponent><P>zq1O+O193Rtuny5UEfIOfVxGmtdQIrBeFCVVjtlqae7XyNul1A47MWVohRf9p4TCZDelccYFp5GEDrSEZZWL16pt5N1SCoG3/7HdH/07PZ+vNRYXC2nMMEipfh56EbSxkqm7zlLhT10wLydC5Ycnte31sJwjst32sTgdxMJLmA0=</P><Q>5/Xpvs4JzXblyzTRQ4jHSrfgTpTthaX8tWEbaLwrnUbOyGMi50kVS32D1m4vf2IQTcER//mVw9SFhVB+A1uQCzPfNhpSWsXEkzjjJ0lEfwsWEANRt165kIPF487T5G4DPo7WcgwzO5zMqj/lUwb5pEIZ8/XS8+O3+b4Lw8y/l2M=</Q><DP>Bh8B6MN359WJKDPCD6oAndvex3U7DVv13VjhuNJdoXeOcod22Nn3DNZ5CwAT5iM7cqZnQwBfaIAZzimwhiWwt5l3mcCoTmFbJrqI1wqz93ZERSk7U341qvRr+K1EZkPYwoQ+aLTxsV0MMUlTZOLk3TGIBkwJjqGeTc9gqlQq0v0=</DP><DQ>uenIaeybwLkuHrt7igRjrwhHWNRWt0q9i+aD4GMJFep9q/CBNhzGW5YDB+eSXCeN57P6KE6zcZyxU5ObHDfT8JanraZVIXnnPxRWQ9OeebC6AyWV5kGeuyxSfG72N9zQ7t/vEsOigIIBK+dAd1Zc2gE86eK6LE300881gL5nRP8=</DQ><InverseQ>E2qRpWgso6kyOJzI7jE2BC6cJ372VPJ+7tAyfy+UscN2qpBKnzAdyXhYc1eST2bs2aN7mynjaFXTgbizL2Nv2yX4iecaNjxkHMtS9wrSM3+xYzRtrH/bj4pyDNzt0gTEqBRHL5kjPF3OV4l4qXqHACD7ILgz8Eq/NbOBifYWbMY=</InverseQ><D>At5csIcZuFx2QAkgkbz3eyK+Cv8/HhqfuMhzBgnJ7LvwQ9rMGeoE+gfq6K3MJJKorCkeNY6LM9QnD5mcLaiN7iSc5eabzEvSvuLXGOoEipsMZGY+o6mc9d6ygo4k32GJPPqJ8I4WDVdQq7qz7fpeQYS1GmwkyTFnN0VNPQXwLS33EFrs65Uc9BW0967XyPz1M4Lx8Mxg/HYK5tk8K+BziLsJFWyrv2c3OrdgaIXftN5QuF9Evvkgy9gztSTv6f03aNJhpo2h/1uYGNxnRM58VSvPQKldrmGqcqVqJwcd1+Sk8aDTNq1FoTYkOjpkqgRp92BCbqCR6YKhAau+m+aTRQ==</D></RSAKeyValue>");
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
			Console.WriteLine(ExportPublicKeyOpenSsh(rsaparams));
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine(ExportPrivateKeyRSAPrivateKey(rsaparams));

			Clipboard.SetText(ExportPrivateKeyRSAPrivateKey(rsaparams));

			Console.WriteLine("Done.");
		}

		public static void TestGenNewKey()
		{
			var rsa = new RSACryptoServiceProvider(2048);
			Clipboard.SetText(rsa.ToXmlString(true));
		}

		/// <summary>
		/// Negative integers are two's complement; by convention, they're detected by the most significant bit of the first byte. To make sure the number is treated as positive, we must prefix it with a zero byte if so.
		/// </summary>
		[NotNull]
		private static byte[] EnsureNonnegativeInteger([NotNull] byte[] bigint)
		{
			if(bigint == null)
				throw new ArgumentNullException("bigint");
			if(bigint.Length == 0)
				return bigint;
			if((bigint[0] & 0x80) == 0)
				return bigint;
			var prefixed = new byte[bigint.Length + 1];
			Buffer.BlockCopy(bigint, 0, prefixed, 1, bigint.Length);
			return prefixed;
		}

		private static void WriteDerIntegerTlvTriplet(uint integer, [NotNull] Stream output)
		{
			byte[] intbytes = new BigInteger(integer).ToByteArray();
			Array.Reverse(intbytes); // Make big-endian
			WriteDerIntegerTlvTriplet(intbytes, output);
		}

		private static void WriteDerIntegerTlvTriplet([NotNull] byte[] bytes, [NotNull] Stream output)
		{
			if(bytes == null)
				throw new ArgumentNullException("bytes");
			if(bytes.Length == 0)
				throw new ArgumentOutOfRangeException("bytes", bytes, "Shan't be empty.");
			if(output == null)
				throw new ArgumentNullException("output");

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

		private static void WriteDerLength(int nDataLen, Stream output)
		{
			if(nDataLen < 0x80)
				output.WriteByte((byte)nDataLen);
			else
			{
				byte[] lenbytes = new BigInteger(nDataLen).ToByteArray();
				Array.Reverse(lenbytes); // Make big-endian
				output.WriteByte((byte)(0x80 /* writing length of the length */| (byte)lenbytes.Length));
				for(int a = 0; a < lenbytes.Length; a++)
					output.WriteByte(lenbytes[a]);
			}
		}

		private static void WriteLengthPrefixedValue([NotNull] byte[] bytes, [NotNull] Stream output)
		{
			if(bytes == null)
				throw new ArgumentNullException("bytes");
			if(output == null)
				throw new ArgumentNullException("output");

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
				throw new ArgumentNullException("input");
			if(output == null)
				throw new ArgumentNullException("output");
			input.Position = 0;
			var buffer = new byte[48]; // 48 octets => 384 bits => 64 sextets, the required PEM line length (Convert::ToBase64String makes them 76 somehow)
			int nRead;
			while((nRead = input.Read(buffer, 0, buffer.Length)) > 0)
				output.AppendLine(Convert.ToBase64String(buffer, 0, nRead, 0));
		}
	}
}