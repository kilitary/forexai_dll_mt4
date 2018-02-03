//class2999
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃コ ア に パ ー キ ロ ェ  ン ヨ ダ キ ェ  !
//┓┏┓┏┓┃＼○／ 
//┛┗┛┗┛┃ / /
//┓┏┓┏┓┃ノ
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃
//......................../´¯)........... 
//.....................,/..../............ 
//..................../..../ ............. 
//............./´¯/' .../´¯/ ¯/\...... 
//........../'/.../... ./... /..././¯\.... 
//........('(....(.... (....(.. /'...).... 
//.........\................. ..\/..../.... 
//..........\......................./´..... 
//............\................ ..(........
using static forexAI.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forexAI
{
    static public class Experimental
    {
        static Random random = new Random((int) (((double) DateTimeOffset.Now.ToUnixTimeMilliseconds())));

        static public void AlliedInstructions()
        {
            int e;
            do
            {
                e = random.Next(6);
            } while (e % 2 == 0 || random.Next(4) == 1);

            if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 21)
                console("пиздец");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 33)
                console("даладно нахуй");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 44)
                console("ахуеть");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 29)
                console("чо за хуйня");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 444)
                console("мудаки ёбаные");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 777)
                console("пошли нахуй");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 999)
                console("пидоры");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 52)
                console("да");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 54)
                console("ого");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 53)
                console("мда бля..");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 154)
                console("ахуенчик");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 48)
                console("бля ну заебись");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 4381)
                console("нед");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 4293)
                console("нет");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 4481)
                console("ноу");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 4811)
                console("найн");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 934)
                console("да и похуй");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 4219)
                console("√");

            if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == Configuration.ExperimentalAlliedRandomLimit)
                console("и чо");

            if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == random.Next(Configuration.ExperimentalAlliedRandomLimit))
            {
                console($"bLACK HATE cHECKPOINt +---{random.Next(Configuration.ExperimentalAlliedRandomLimit)} {random.Next(Configuration.ExperimentalAlliedRandomLimit)} {random.Next(Configuration.ExperimentalAlliedRandomLimit)} {random.Next(Configuration.ExperimentalAlliedRandomLimit)} {random.Next(Configuration.ExperimentalAlliedRandomLimit)} {random.Next(Configuration.ExperimentalAlliedRandomLimit)}---+");
                Audio.FX.TheBroken();
            }
        }
    }
}

