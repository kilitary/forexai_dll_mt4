﻿//......................../´¯)........... 
//.....................,/..../............ 
//..................../..../ ............. 
//............./´¯/' .../´¯/ ¯/\...... 
//........../'/.../... ./... /..././¯\.... 
//........('(....(.... (....(.. /'...).... 
//.........\................. ..\/..../.... 
//..........\......................./´..... 
//............\................ ..(........

using System;

namespace NQuotes.DebugHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                NQuotes.DebugHost.Server.Start(args);
            }
            catch(Exception e)
            {
                Console.WriteLine($"EXCEPTION:\r\n{e.Message}");
            }
        }
    }
}
