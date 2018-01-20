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
            if (random.Next(Configuration.ExRandomDimension) == 21)
                console("пиздец");
            else if (random.Next(Configuration.ExRandomDimension) == 33)
                console("даладно нахуй");
            else if (random.Next(Configuration.ExRandomDimension) == 44)
                console("ахуеть");
            else if (random.Next(Configuration.ExRandomDimension) == 29)
                console("чо за хуйня");
            else if (random.Next(Configuration.ExRandomDimension) == 444)
                console("мудаки ёбаные");
            else if (random.Next(Configuration.ExRandomDimension) == 777)
                console("пошли нахуй");
            else if (random.Next(Configuration.ExRandomDimension) == 999)
                console("пидоры");
            else if (random.Next(Configuration.ExRandomDimension) == 52)
                console("да");
            else if (random.Next(Configuration.ExRandomDimension) == 54)
                console("ого");
            else if (random.Next(Configuration.ExRandomDimension) == 53)
                console("мда бля..");
            else if (random.Next(Configuration.ExRandomDimension) == 154)
                console("ахуенчик");
            else if (random.Next(Configuration.ExRandomDimension) == 48)
                console("бля ну заебись");
            else if (random.Next(Configuration.ExRandomDimension) == 9981)
                console("нед");
            else if (random.Next(Configuration.ExRandomDimension) == 9293)
                console("нет");
            else if (random.Next(Configuration.ExRandomDimension) == 9481)
                console("ноу");
            else if (random.Next(Configuration.ExRandomDimension) == 9811)
                console("найн");
            else if (random.Next(Configuration.ExRandomDimension) == 934)
                console("да и похуй");

            if (random.Next(Configuration.ExRandomDimension) == Configuration.ExRandomDimension)
                console("и чо");

            if (random.Next(Configuration.ExRandomDimension) == random.Next(Configuration.ExRandomDimension))
                console($"блэк хейт чекпоинт +---{random.Next(Configuration.ExRandomDimension)} {random.Next(Configuration.ExRandomDimension)} {random.Next(Configuration.ExRandomDimension)} {random.Next(Configuration.ExRandomDimension)} {random.Next(Configuration.ExRandomDimension)} {random.Next(Configuration.ExRandomDimension)}---+");
        }
    }
}

