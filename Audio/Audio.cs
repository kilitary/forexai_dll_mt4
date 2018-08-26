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
using static forexAI.Logger;

namespace forexAI
{
	public static class AudioFX
	{
		public static bool currentlyPriceComingPlaying = false;

		public static void Play(string audioFileName)
		{
			if(!Configuration.audioEnabled)
				return;

			new SoundPlayer(audioFileName).Play();
		}

		public static void Wipe()
		{
			Play(Configuration.wipeWav);
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

		public static void PriceComing(double freq)
		{
			if(currentlyPriceComingPlaying || !App.config.IsEnabled("priceApproachingSound"))
				return;

			currentlyPriceComingPlaying = true;
			//log($"freq bef={freq} poiunt={App.mqlApi.Point}", "dev");
			freq = (freq) / App.MQLApi.Point / 10;
			//log($"freq aft={freq}", "dev");
			Task.Factory.StartNew(() =>
			{
				for(var i = 0; i < freq % 2; i++)
				{
					Thread.Sleep((int) freq);
					Console.Beep(776 + i * 100, 50 - i * 3);
				}
				currentlyPriceComingPlaying = false;
			});
		}
	}
}
