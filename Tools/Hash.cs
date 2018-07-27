//......................../´¯)........... 
//.....................,/..../............ 
//..................../..../ ............. 
//............./´¯/' .../´¯/ ¯/\...... 
//........../'/.../... ./... /..././¯\.... 
//........('(....(.... (....(.. /'...).... 
//.........\................. ..\/..../.... 
//..........\......................./´..... 
//............\................ ..(........

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace forexAI
{
	public static class Hash
	{
		public static string md5(string input)
		{
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

			byte[] originalBytes = ASCIIEncoding.Default.GetBytes(input);
			byte[] encodedBytes = md5.ComputeHash(originalBytes);

			return BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
		}

		public static string sha256(string input)
		{
			SHA256 mySHA256 = SHA256.Create();

			byte[] originalBytes = ASCIIEncoding.Default.GetBytes(input);
			byte[] encodedBytes = mySHA256.ComputeHash(originalBytes);

			return BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
		}
	}
}
