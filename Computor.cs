using System;
using System.Collections.Generic;
using System.IO;

namespace computorv1
{
    internal static class Computor
    {
        private static void Main(string[] args)
        {
            try
            {
                if (args.Length < 1)
                    PrintUsage();
                else
                {
                    ParseInput(args);
                    EquationParser.Parse(args[^1]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[Error] " + (e.Message.Length != 0 ? e.Message : e.ToString()));
            }
        }

        private static void ParseInput(string[] args)
        {
            if (args.Length >= 2)
            {
                var opts = new string[args.Length - 1];
                Array.Copy(args, opts, args.Length - 1);
                OptionsParser.Parse(opts);
            }
        }
        
        private static void PrintUsage()
        { 
            Console.WriteLine("./computor.sh [options] \"equation\"\n\t(to get equation roots)");
            Console.WriteLine("options:\n" +
                              "\t-rnd:<N>\t - generate N random equations, where's N - integer number in range (1, 100) inclusive.\n" +
                              "\t-s\t - print equation solving steps\n" +
                              "\t-z\t - print something else idk");
        }
    }
}