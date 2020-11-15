using System;
using System.Collections.Generic;
using System.IO;

namespace computorv1
{
    internal class OptionsParser
    {
        private bool _rndFlagSet;
        private bool _sFlagSet;
        private int _rndEquationsCount;
        
        private OptionsParser() {}

        public static void Parse(string[] opts)
        {
            var self = new OptionsParser();
            
            self.ParseOptions(opts);
            self.ProceedOptions();
        }

        private void ParseOptions(string[] opts)
        {
            // foreach (var opt in opts)
            //     Console.Write("[" + opt + "] "); //TODO: remove
            // Console.WriteLine();
            
            foreach (var opt in opts)
            {
                if (opt.StartsWith("-rnd:"))
                {
                    if (_rndFlagSet)
                        throw new Exception("-rnd flag is already set.");
                    TrySetRndFlag(opt);
                }
                else
                {
                    switch (opt)
                    {
                        case "-s":
                            throw new NotImplementedException();
                        default:
                            throw new Exception("unknown flag provided: " + opt);
                    }
                    
                }
            }
        }
        
        private void ProceedOptions()
        {
            if (_rndFlagSet)
                GenerateRandomEquations(_rndEquationsCount);
            
        }
        
        private void TrySetRndFlag(string opt)
        {
            try
            {
                var numericPart = opt.Substring(5);
                var numVal = int.Parse(numericPart);
                
                if (numVal <= 0 || numVal > 100)
                    throw new Exception("-rnd flag value should be integer number in range (1, 100) inclusive.");
                _rndFlagSet = true;
                _rndEquationsCount = numVal;
            }
            catch (ArgumentNullException)
            {
                throw new Exception("no value provided for -rnd flag.");
            }
            catch (FormatException)
            {
                throw new Exception("incorrect number format for -rnd flag.");
            }
            catch (OverflowException)
            {
                throw new Exception("value of -rnd flag should be in range of integer.");
            }
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
                    var equationPresented = false;
                    var partsCount = rnd.Next(2, 8);
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