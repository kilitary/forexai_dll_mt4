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
        static Random random = new Random((int) (int) (((double) DateTimeOffset.Now.ToUnixTimeMilliseconds()) / 3.2));

        static public void AlliedInstructions()
        {
            if (random.Next(Configuration.ExperimentalRandomLimit) == 21)
                console("пиздец");
            else if (random.Next(Configuration.ExperimentalRandomLimit) == 33)
                console("даладно нахуй");
            else if (random.Next(Configuration.ExperimentalRandomLimit) == 44)
                console("ахуеть");
            else if (random.Next(Configuration.ExperimentalRandomLimit) == 29)
                console("чо за хуйня");
            else if (random.Next(Configuration.ExperimentalRandomLimit) == 444)
                console("мудаки ёбаные");
            else if (random.Next(Configuration.ExperimentalRandomLimit) == 777)
                console("пошли нахуй");
            else if (random.Next(Configuration.ExperimentalRandomLimit) == 999)
                console("пидоры");
            else if (random.Next(Configuration.ExperimentalRandomLimit) == 52)
                console("да");
            else if (random.Next(Configuration.ExperimentalRandomLimit) == 54)
                console("ого");
            else if (random.Next(Configuration.ExperimentalRandomLimit) == 53)
                console("мда бля..");
            else if (random.Next(Configuration.ExperimentalRandomLimit) == 154)
                console("ахуенчик");
            else if (random.Next(Configuration.ExperimentalRandomLimit) == 48)
                console("бля ну заебись");
            else if (random.Next(Configuration.ExperimentalRandomLimit) == 9981)
                console("нед");
            else if (random.Next(Configuration.ExperimentalRandomLimit) == 9293)
                console("нет");
            else if (random.Next(Configuration.ExperimentalRandomLimit) == 9481)
                console("ноу");
            else if (random.Next(Configuration.ExperimentalRandomLimit) == 9811)
                console("найн");
            else if (random.Next(Configuration.ExperimentalRandomLimit) == 934)
                console("да и похуй");

            if (random.Next(Configuration.ExperimentalRandomLimit) == Configuration.ExperimentalRandomLimit)
                console("и чо");

            if (random.Next(Configuration.ExperimentalRandomLimit) == random.Next(Configuration.ExperimentalRandomLimit))
                console($"блэк хейт чекпоинт +---{random.Next(Configuration.ExperimentalRandomLimit)} {random.Next(Configuration.ExperimentalRandomLimit)} {random.Next(Configuration.ExperimentalRandomLimit)} {random.Next(Configuration.ExperimentalRandomLimit)} {random.Next(Configuration.ExperimentalRandomLimit)} {random.Next(Configuration.ExperimentalRandomLimit)}---+");
        }
    }
}

