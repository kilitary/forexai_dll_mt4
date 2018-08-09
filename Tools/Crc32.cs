using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forexAI
{
	public static class Crc32
	{
		static uint[] table;
		static bool bInitDone = false;

		public static uint Calc(string str)
		{
			if(!bInitDone)
				Init();

			byte[] originalBytes = Encoding.Default.GetBytes(str);
			return ComputeChecksum(originalBytes);
		}

		public static uint ComputeChecksum(byte[] bytes)
		{
			if(!bInitDone)
				Init();

			uint crc = 0xffffffff;
			for(int i = 0; i < bytes.Length; ++i)
			{
				byte index = (byte) (((crc) & 0xff) ^ bytes[i]);
				crc = (uint) ((crc >> 8) ^ table[index]);
			}
			return ~crc;
		}

		public static byte[] ComputeChecksumBytes(byte[] bytes)
		{
			if(!bInitDone)
				Init();

			return BitConverter.GetBytes(ComputeChecksum(bytes));
		}

		public static void Init()
		{
			uint poly = 0xedb88320;
			table = new uint[256];
			uint tempValue = 0;
			for(uint i = 0; i < table.Length; ++i)
			{
				tempValue = i;
				for(int j = 8; j > 0; --j)
				{
					if((tempValue & 1) == 1)
					{
						tempValue = (uint) ((tempValue >> 1) ^ poly);
					}
					else
					{
						tempValue >>= 1;
					}
				}
				table[i] = tempValue;
			}

			bInitDone = true;
		}
	}
}