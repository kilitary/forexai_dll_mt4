//コ  ー キ ロ ェ に パ ン ヨ ダ キ ェ アコ  ー キ ロ ェ に パ ン ヨ ダ キ ェ アコ  ー キ ロ ェ に パ ン ヨ ダ キ ェ アコ  ー キ ロ ェ に パ ン ヨ ダ キ ェ ア
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
            SoundPlayer sampleSound = new SoundPlayer(audioFileName);
            sampleSound.Play();
        }

        public static void TheNewDay()
        {
            Play(Configuration.newDayWAV);
        }

        public static void Profit()
        {
            Play(Configuration.profitWAV);
        }

        public static void TheBroken()
        {
            Play(Configuration.brokenWAV);
        }

        public static void TheGoodWork()
        {
            Play(Configuration.goodWorkWAV);
        }
        public static void LowBalance()
        {
            Play(Configuration.lowBalanceWAV);
        }

    }
}
