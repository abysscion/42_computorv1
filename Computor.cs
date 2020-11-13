using System;

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
            string osDependentStr;
            if (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX)
                osDependentStr = "./computor";
            else
                osDependentStr = "computor.exe";
            
            Console.WriteLine(osDependentStr + " [options] \"equation\"\n\t(to get equation roots)");
            Console.WriteLine("options:\n" +
                              "\t-s\t - print equation solving steps\n" +
                              "\t-z\t - print something else idk");
        }
    }
}