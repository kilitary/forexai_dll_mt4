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
            if (random.Next(Configuration.ExperimentalRandomDimension) == 21)
                console("пиздец");
            else if (random.Next(Configuration.ExperimentalRandomDimension) == 33)
                console("даладно нахуй");
            else if (random.Next(Configuration.ExperimentalRandomDimension) == 44)
                console("ахуеть");
            else if (random.Next(Configuration.ExperimentalRandomDimension) == 29)
                console("чо за хуйня");
            else if (random.Next(Configuration.ExperimentalRandomDimension) == 444)
                console("мудаки ёбаные");
            else if (random.Next(Configuration.ExperimentalRandomDimension) == 666)
                console("пошли нахуй");
            else if (random.Next(Configuration.ExperimentalRandomDimension) == 999)
                console("пидоры");
            else if (random.Next(Configuration.ExperimentalRandomDimension) == 52)
                console("да");
            else if (random.Next(Configuration.ExperimentalRandomDimension) == 54)
                console("ого");
            else if (random.Next(Configuration.ExperimentalRandomDimension) == 53)
                console("мда бля..");
            else if (random.Next(Configuration.ExperimentalRandomDimension) == 99)
                console("нет");
            else if (random.Next(Configuration.ExperimentalRandomDimension) == 154)
                console("ахуенчик");
            else if (random.Next(Configuration.ExperimentalRandomDimension) == 48)
                console("бля ну заебись");
            else if (random.Next(Configuration.ExperimentalRandomDimension) == Configuration.ExperimentalRandomDimension)
                console("и чо");

            if (random.Next(Configuration.ExperimentalRandomDimension) == random.Next(Configuration.ExperimentalRandomDimension))
                console($"блэк хэйт чекпоинт -> {random.Next(Configuration.ExperimentalRandomDimension)} {random.Next(Configuration.ExperimentalRandomDimension)} {random.Next(Configuration.ExperimentalRandomDimension)}");
        }
    }
}

