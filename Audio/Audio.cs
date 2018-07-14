//コ  ー キ ロ ェ に パ ン ヨ ダ キ ェ アコ  ー キ ロ ェ に パ ン ヨ ダ キ ェ アコ  ー キ ロ ェ に パ ン ヨ ダ キ ェ アコ  ー キ ロ ェ に パ ン ヨ ダ キ ェ ア
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using forexAI;

namespace forexAI
{
	public static class AudioFX
	{
		public static void Play(string audioFileName)
		{
			if (!Configuration.audioEnabled)
				return;

			new SoundPlayer(audioFileName).Play();
		}

		public static void Wipe()
		{
			Play(Configuration.wipe);
		}

		public static void NewDay()
		{
			Play(Configuration.newDayWAV);
		}

		public static void Fail()
		{
			Play(Configuration.failWav);
		}

		public static void Profit()
		{
			Play(Configuration.profitWAV);
		}

		public static void TheBroken()
		{
			Play(Configuration.brokenWAV);
		}

		public static void GoodWork()
		{
			Play(Configuration.goodWorkWAV);
		}
		public static void LowBalance()
		{
			Play(Configuration.lowBalanceWAV);
		}

	}
}
