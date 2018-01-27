//class2999
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃ ЕБАНЫЙ РОООООТ!
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
        static Random random = new Random((int) (((double) DateTimeOffset.Now.ToUnixTimeMilliseconds()) / 3.9));

        static public void AlliedInstructions()
        {
            int e;
            do
            {
                e = random.Next(3);
            } while (e % 2 == 0 || YRandom.Next(4) > 1);

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
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 9981)
                console("нед");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 9293)
                console("нет");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 9481)
                console("ноу");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 9811)
                console("найн");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 934)
                console("да и похуй");
            else if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == 99219)
                console("√");

            if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == Configuration.ExperimentalAlliedRandomLimit)
                console("и чо");

            if (random.Next(Configuration.ExperimentalAlliedRandomLimit) == random.Next(Configuration.ExperimentalAlliedRandomLimit))
            {
                console($"блэк хейт чекпоинт +---{random.Next(Configuration.ExperimentalAlliedRandomLimit)} {random.Next(Configuration.ExperimentalAlliedRandomLimit)} {random.Next(Configuration.ExperimentalAlliedRandomLimit)} {random.Next(Configuration.ExperimentalAlliedRandomLimit)} {random.Next(Configuration.ExperimentalAlliedRandomLimit)} {random.Next(Configuration.ExperimentalAlliedRandomLimit)}---+");
                Audio.FX.FXBroken();
            }
        }
    }
}

