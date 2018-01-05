using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forexAI
{
    internal static class Logger
    {
        public static void log(String lines)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"d:\temp\forexAI\mt4.log", true);
            file.WriteLine(lines);
            file.Close();
        }

        public static void info(String lines)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"d:\temp\forexAI\mt4.log", true);
            file.WriteLine("info: " + lines);
            file.Close();
        }

        public static void warning(String lines)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"d:\temp\forexAI\mt4.log", true);
            file.WriteLine("warning: " + lines);
            file.Close();
        }

        public static void error(String lines)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"d:\temp\forexAI\mt4.log", true);
            file.WriteLine("error: " + lines);
            file.Close();
        }
    }
}