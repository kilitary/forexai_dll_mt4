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
        public static void FXNewDay()
        {
            SoundPlayer simpleSound = new SoundPlayer(Configuration.newDayWAV);
            simpleSound.PlaySync();
        }

        public static void FXProfit()
        {
            SoundPlayer simpleSound = new SoundPlayer(Configuration.profitWAV);
            simpleSound.PlaySync();
        }

        public static void FXBroken()
        {
            SoundPlayer simpleSound = new SoundPlayer(Configuration.brokenWAV);
            simpleSound.PlaySync();
        }
    }
}
