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
        static Random random = new Random((int) (int) (((double) DateTimeOffset.Now.ToUnixTimeMilliseconds()) / 3.1));

        static public void AlliedInstructions()
        {
            if (YRandom.next(Configuration.ExperimentalRandomDimension) == 21)
                console("пиздец");
            else if (YRandom.next(Configuration.ExperimentalRandomDimension) == 33)
                console("даладно нахуй");
            else if (YRandom.next(Configuration.ExperimentalRandomDimension) == 44)
                console("ахуеть");
            else if (YRandom.next(Configuration.ExperimentalRandomDimension) == 29)
                console("чо за хуйня");
            else if (YRandom.next(Configuration.ExperimentalRandomDimension) == 444)
                console("мудаки ёбаные");
            else if (YRandom.next(Configuration.ExperimentalRandomDimension) == 777)
                console("пошли нахуй");
            else if (YRandom.next(Configuration.ExperimentalRandomDimension) == 999)
                console("пидоры");
            else if (YRandom.next(Configuration.ExperimentalRandomDimension) == 52)
                console("да");
            else if (YRandom.next(Configuration.ExperimentalRandomDimension) == 54)
                console("ого");
            else if (YRandom.next(Configuration.ExperimentalRandomDimension) == 53)
                console("мда бля..");
            else if (YRandom.next(Configuration.ExperimentalRandomDimension) == 154)
                console("ахуенчик");
            else if (YRandom.next(Configuration.ExperimentalRandomDimension) == 48)
                console("бля ну заебись");
            else if (YRandom.next(Configuration.ExperimentalRandomDimension) == 9981)
                console("нед");
            else if (YRandom.next(Configuration.ExperimentalRandomDimension) == 929)
                console("нет");
            else if (YRandom.next(Configuration.ExperimentalRandomDimension) == 9481)
                console("ноу");
            else if (YRandom.next(Configuration.ExperimentalRandomDimension) == 9811)
                console("найн");
            else if (YRandom.next(Configuration.ExperimentalRandomDimension) == 934)
                console("да и похуй");

            if (YRandom.next(Configuration.ExperimentalRandomDimension) == Configuration.ExperimentalRandomDimension - 1)
                console("и чо");

            if (YRandom.next(Configuration.ExperimentalRandomDimension) == YRandom.next(Configuration.ExperimentalRandomDimension))
                console($"блэк хейт чекпоинт +---{YRandom.next(Configuration.ExperimentalRandomDimension)} {YRandom.next(Configuration.ExperimentalRandomDimension)} {YRandom.next(Configuration.ExperimentalRandomDimension)} {YRandom.next(Configuration.ExperimentalRandomDimension)} {YRandom.next(Configuration.ExperimentalRandomDimension)} {YRandom.next(Configuration.ExperimentalRandomDimension)}---+");
        }
    }
}

