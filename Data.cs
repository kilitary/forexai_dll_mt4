using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forexAI
{
    internal static class Data
    {
        public static Dictionary<string, Dictionary<string, string>> nnFunctions =
            new Dictionary<string, Dictionary<string, string>>();

        static public DB db = null;

        public static string DataDirectory = @"D:\temp\forexAI";
    }
}