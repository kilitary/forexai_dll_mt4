//cl4$$4399...34343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃E x p e r i m e n t a l c o d e i s n o t w r i t t e n f o r m o m e N T S W h e n s o m e o n e a s k s " f o r w h a t ? "
//┓┏┓┏┓┃                                             !
//┛┗┛┗┛┃に パ に ェ キ  コ ア... に ダ ヨ パ ェ, ェm  ンンンン ダ-ダ-ダ                                     [ ロー○ーロ ]            [ ,..m=n=m..' ]
//┓┏┓┏┓┃＼○／ 
//┛┗┛┗┛┃ / /
//┓┏┓┏┓┃ノ
//┛┗┛┗┛┃! 
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃パ
//................../´¯)........... 
//...............,/..../............ 
//............../..../ ............. 
//......./´¯/' .../´¯/ ¯/\...... 
//..../'/.../... ./... /..././¯\.... 
//..('(....(.... (....(.. /'...).... 
//...\................. ..\/..../.... 
//....\......................./´..... 
//......\................ ..(........
using static forexAI.Logger;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

namespace forexAI
{
    static public class Experimental
    {
        static Random random = new Random((int) (((double) DateTimeOffset.Now.ToUnixTimeMilliseconds() + 48)));

        static public void AlliedInstructions()
        {
            int e;
            do
            {
                e = random.Next(6);
            } while (e % 2 == 0 || random.Next(4) == 1);

            if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 21)
                console("пиздец", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 33)
                console("даладно нахуй", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 44)
                console("ахуеть", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 29)
                console("чо за хуйня", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 444)
                console("мудаки ёбаные", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 777)
                console("пошли нахуй", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 999)
                console("пидоры", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 52)
                console("да", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 54)
                console("ого", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 53)
                console("мда бля..", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 154)
                console("ахуенчик", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 48)
                console("бля ну заебись", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 4381)
                console("нед", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 4293)
                console("нет", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 4481)
                console("ноу", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 4811)
                console("найн", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 934)
                console("да и похуй", ConsoleColor.White, ConsoleColor.Green);
            else if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == 4219)
                console("√", ConsoleColor.White, ConsoleColor.Green);

            if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == Configuration.experimentalAlliedRandomUpperBound)
                console("и чо", ConsoleColor.White, ConsoleColor.Green);

            IsBlackHateFocused();
        }

        static public bool IsBlackHateFocused() // B l ack @te.. F oc us ed
        {
            if (random.Next(Configuration.experimentalAlliedRandomUpperBound) == random.Next(Configuration.experimentalAlliedRandomUpperBound))
            {
                console($"bLACK HATE CHECKPOINt +---{random.Next(Configuration.experimentalAlliedRandomUpperBound)} {random.Next(Configuration.experimentalAlliedRandomUpperBound)} {random.Next(Configuration.experimentalAlliedRandomUpperBound)} {random.Next(Configuration.experimentalAlliedRandomUpperBound)} {random.Next(Configuration.experimentalAlliedRandomUpperBound)} {random.Next(Configuration.experimentalAlliedRandomUpperBound)}---+", ConsoleColor.Red, ConsoleColor.DarkBlue);
                Audio.FX.TheBroken();
                return true;
            }
            return false;
        }

        static public bool IsHardwareForcesConnected()
        {
            int cValue = random.Next(Configuration.experimentalAlliedRandomUpperBound);
            if (cValue <= 41 || cValue >= 44 || cValue == 11)
            {
                return random.Next(0, 5) < 3;
            }
            return random.Next(0, 2) == 0;
        }
    }
}

