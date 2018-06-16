using System;
using System.IO;
using System.Security.Cryptography;
using static forexAI.Logger;

namespace forexAI
{
	public static class YRandom
	{
		private static Random random = new Random((int) DateTimeOffset.Now.ToUnixTimeMilliseconds());
		private static int prevResult = -1;
		private static readonly RNGCryptoServiceProvider _generator = new RNGCryptoServiceProvider();

		public static int Next(int n)
		{
			return between(0, n);
		}

		public static int Next(int n, int m)
		{
			return between(n, m);
		}

		public static int between(int minimumValue, int maximumValue)
		{
			byte[] randomNumber = new byte[1];
			int result;

			do
			{
				_generator.GetBytes(randomNumber);

				double asciiValueOfRandomCharacter = Convert.ToDouble(randomNumber[0]);

				// We are using Math.Max, and substracting 0.00000000001, 
				// to ensure "multiplier" will always be between 0.0 and .99999999999
				// Otherwise, it's possible for it to be "1", which causes problems in our rounding.
				double multiplier = Math.Max(0, (asciiValueOfRandomCharacter / 255d) - 0.00000000001d);

				// We need to add one to the range, to allow for the rounding done with Math.Floor
				int range = maximumValue - minimumValue + 1;

				double randomValueInRange = Math.Floor(multiplier * range);

				result = (int) (minimumValue + randomValueInRange);
			} while (result == prevResult);

			prevResult = result;

			return result;
		}

		public static string randomString()
		{
			// create a stronger hash code using RNGCryptoServiceProvider
			byte[] randomLocal = new byte[64];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			// populate with random bytes
			rng.GetBytes(randomLocal);

			// convert random bytes to string
			string randomBase64 = Convert.ToBase64String(randomLocal);
			// display
			return randomBase64;
		}
	}
}