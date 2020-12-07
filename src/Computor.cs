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
            }
        }

        private static void ProceedArguments(string[] args)
        {
            EquationSolver solver;
            string equation;
            var optsParser = new OptionsParser();
            var opts = new string[args.Length - 1];

            Array.Copy(args, opts, args.Length - 1);
            optsParser.Parse(opts);
            equation = EquationParser.Parse(args[^1]);
            solver = new EquationSolver(equation, optsParser.FFlagSet);
            solver.Solve();
            
            PrintInfo(solver, optsParser, equation);
            if (optsParser.RndFlagSet)
                GenerateRandomEquations(optsParser.RndEquationsCount);
        }

        private static void PrintInfo(EquationSolver solver, OptionsParser optsParser, string equation)
        {
            Console.WriteLine($"Reduced form: {equation}");
            Console.WriteLine($"Polynomial degree: {solver.Degree}");
            
            if (solver.Degree == 0)
            {
                Console.WriteLine(solver.SolutionType == EquationSolver.SolutionTypes.Any
                    ? "Any number is solution."
                    : "There is no solution.");
            }
            else if (solver.Degree == 1)
                Console.WriteLine(solver.Roots[0]);
            else
            {
                if (solver.Discriminant > 0)
                    Console.WriteLine("Discriminant is strictly positive. There are two solutions:");
                else if (solver.Discriminant == 0)
                    Console.WriteLine("Discriminant is 0. There is one solution:");
                else
                    Console.WriteLine("Discriminant is strictly negative. There are two complex solutions:");
                foreach (var root in solver.Roots)
                    Console.WriteLine(root);
            }

            if (optsParser.RFlagSet)
            {
                Console.WriteLine("\nREDUCTION STEPS:");
                EquationParser.PrintReductionSteps();
            }
            
            if (optsParser.SFlagSet)
            {
                Console.WriteLine("\nSOLVING STEPS:");
                foreach (var step in solver.Steps)
                    Console.WriteLine(step);
            }
        }

        private static void PrintUsage()
        { 
            Console.WriteLine("./computor.sh [options] \"equation\"\n\t(to solve equation)");
            Console.WriteLine("options:\n" +
                              "\t-rnd:<N>\t- generate N random equations, where's N - integer number in range (1, 100) inclusive.\n" + 
							  "\t\t\t  generated equations will be stored at file \"rndEquations.txt\"\n" +
                              "\t-f\t\t- show roots as fractions\n" +
                              "\t-s\t\t- print equation solving steps\n" +
                              "\t-r\t\t- print equation reducing steps");
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
                            
                            if (rnd.NextDouble() >= 0.5) // would power be presented?
                            {
                                part += "^";
                                part += signs[rnd.Next(0, 3)]; // would sign be presented?
                                part += rnd.Next(0, 5);
                            }
                        }

                        if (!numPresented || rnd.NextDouble() >= 0.5) // would ex be presented?
                        {
                            if (numPresented && rnd.NextDouble() >= 0.5) // would * between number and X be presented?
                                part += "*";
                            part += exes[rnd.Next(0, 2)];
                            
                            if (!numPresented && rnd.NextDouble() >= 0.5) // would power be presented?
                            {
                                part += "^";
                                part += signs[rnd.Next(0, 3)]; // would sign be presented?
                                part += rnd.Next(0, 4);
                            }
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
