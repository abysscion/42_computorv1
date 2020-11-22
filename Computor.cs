using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace computorv1
{
    internal static class Computor
    {
        private static void Main(string[] args)
        {
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
                
                if (args.Length < 1)
                    PrintUsage();
                else
                    ProceedArguments(args);
            }
            catch (Exception e)
            {
                Console.WriteLine("[Error] " + (e.Message.Length != 0 ? e.Message : e.ToString()));
                if (e.StackTrace !=  null)
                    Console.WriteLine(e.StackTrace);
            }
        }

        private static void ProceedArguments(string[] args)
        {
            var optsParser = new OptionsParser();
            var opts = new string[args.Length - 1];
            string equ;
            int degree;
            
            Array.Copy(args, opts, args.Length - 1);
            optsParser.Parse(opts);
            if (optsParser.RndFlagSet)
                GenerateRandomEquations(optsParser.RndEquationsCount);
            Console.WriteLine($"Reduced form: {equ = EquationParser.Parse(args[^1], optsParser.RFlagSet)}");
            Console.WriteLine($"Polynomial degree: {degree = EquationParser.GetPolynomialDegree(equ)}");
            if (degree < 0 || degree > 2)
                throw new Exception("sorry, but polynomial degree should be in range of [0, 2].");
            if (degree == 0)
            {
                if (equ.Length != 3)
                    throw new Exception("seems like there's no actual EQUATION!");
                if (equ[0] != equ[2])
                    throw new Exception("seems like there's no actual EQUATION!");
                Console.WriteLine("The solutions are all numbers.");
            }
            else
            {
                var solver = new EquationSolver();
                solver.Solve(equ);
            }
        }

        private static void PrintUsage()
        { 
            Console.WriteLine("./computor.sh [options] \"equation\"\n\t(to solve equation)");
            Console.WriteLine("options:\n" +
                              "\t-rnd:<N>\t - generate N random equations, where's N - integer number in range (1, 100) inclusive.\n" +
                              "\t-s\t - print equation solving steps\n" +
                              "\t-r\t - print equation reducing steps");
        }
        
        private static void GenerateRandomEquations(int count = 20)
        {
            if (count <= 0)
                return;
            
            var signs = new [] {"", "+", "-", "*"};
            var exes = new [] {"x", "X"};
            var rnd = new Random();

            using (var writer = new StreamWriter(new FileStream("rndEquations.txt", FileMode.Create)))
            {
                for (var i = 0; i < count; i++)
                {
                    var wholeStr = "";
                    var partsCount = rnd.Next(2, 21);
                    var partsList = new List<string>();
                    
                    for (var j = 0; j < partsCount; j++)
                    {
                        var numPresented = false;  
                        var part = "";
                        
                        if (rnd.NextDouble() >= 0.5) // would number be presented?
                        {
                            part += signs[rnd.Next(0, 3)]; // would sign be presented?
                            if (rnd.NextDouble() >= 0.5) // would number be floating?
                                part += $"{rnd.NextDouble() * rnd.Next(0, 10):#0.0#}";
                            else
                                part += rnd.Next(0, 11);
                            numPresented = true;
                        }

                        if (!numPresented || rnd.NextDouble() >= 0.5) // would ex be presented?
                        {
                            if (numPresented && rnd.NextDouble() >= 0.5) // would * between number and X be presented?
                                part += "*";
                            part += exes[rnd.Next(0, 2)];
                        }

                        if (rnd.NextDouble() >= 0.5) // would power be presented?
                        {
                            part += "^";
                            part += signs[rnd.Next(0, 3)]; // would sign be presented?
                            part += rnd.Next(0, 10);
                        }

                        partsList.Add(part);
                    }

                    wholeStr += partsList[0];
                    partsList.RemoveAt(0);
                    var leftPartsCount = partsList.Count / 2;
                    var rightPartsCount = partsList.Count % 2;
                    for (var j = 0; j < leftPartsCount;  j++)
                        wholeStr += signs[rnd.Next(1, 4)] + partsList[j];
                    wholeStr += "=";
                    wholeStr += partsList[leftPartsCount];
                    for (var j = 1; j < rightPartsCount;  j++)
                        wholeStr += signs[rnd.Next(1, 4)] + partsList[j];

                    writer.WriteLine(wholeStr.Replace(',', '.'));
                }
            }
        }
    }
}