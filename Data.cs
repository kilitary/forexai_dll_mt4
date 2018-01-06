using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forexAI
{
    static class Data
    {
        public static Dictionary <string, Dictionary <string, string>> nnFunctions =
            new Dictionary <string, Dictionary <string, string>>();

        public static DB db = null;
    }
}