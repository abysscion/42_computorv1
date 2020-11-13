using System;

namespace computorv1
{
    internal class EquationParser
    {
        private EquationParser() {}

        public static void Parse(string equation)
        {
            var self = new EquationParser();
            
            Console.WriteLine(equation);
        }
    }
}