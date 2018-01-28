using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using forexAI;

namespace Audio
{
    public static class FX
    {
        public static void Play(string audioFileName)
        {
            SoundPlayer simpleSound = new SoundPlayer(audioFileName);
            simpleSound.Play();
        }

        public static void FXNewDay()
        {
            Play(Configuration.newDayWAV);
        }

        public static void FXProfit()
        {
            Play(Configuration.profitWAV);
        }

        public static void FXBroken()
        {
            Play(Configuration.brokenWAV);
        }

        public static void FXLowBalance()
        {
            Play(Configuration.lowBalanceWAV);
        }

        public static void FXGoodWork()
        {
            Play(Configuration.goodWorkWAV);
        }
    }
}
