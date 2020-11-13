using System;

namespace computorv1
{
    internal class OptionsParser
    {
        private bool sFlagSet;
        
        private OptionsParser() {}

        public static void Parse(string[] opts)
        {
            var self = new OptionsParser();

            foreach (var opt in opts)
                Console.Write(opt + " ");
            Console.WriteLine();
        }
    }
}