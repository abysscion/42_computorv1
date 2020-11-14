using System;
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
                var numericPart = opt.Substring(3);
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
            // var partsList = new List<string>(); //TODO: remove
            
            using (var writer = new StreamWriter(new FileStream("rndEquations.txt", FileMode.Create)))
            {
                for (var i = 0; i < count; i++)
                {
                    var wholeStr = "";
                    var equationPresented = false;
                    var partsCount = rnd.Next(2, 6);
                    for (var j = 0; j < partsCount; j++)
                    {
                        var numPresented = false;  
                        var part = "";
                        
                        if (rnd.NextDouble() >= 0.5) // would number be presented?
                        {
                            part += signs[rnd.Next(j == 0 ? 0 : 1, j == 0 ? 3 : 4)]; // would sign be presented?
                            if (rnd.NextDouble() >= 0.5) // would number be floating?
                                part += $"{rnd.NextDouble() * rnd.Next(0, 10):#0.0#}";
                            else
                                part += rnd.Next(0, 11);
                            numPresented = true;
                        }

                        if (!numPresented) // check if we get no number, so we won't get case [2x][x^2] -> 2xx^2
                            part += signs[rnd.Next(j == 0 ? 0 : 1, j == 0 ? 3 : 4)]; //which sign presented?
                        
                        if (!numPresented || rnd.NextDouble() >= 0.5) // would ex be presented?
                        {
                            if (numPresented && rnd.NextDouble() >= 0.5) // would * between number and X be presented?
                                part += "*";
                            part += exes[rnd.Next(0, 2)];
                        }

                        if (rnd.NextDouble() >= 0.5) // would power be presented?
                        {
                            part += "^";
                            part += signs[rnd.Next(0, 3)]; // does sign presented?
                            part += rnd.Next(0, 10);
                        }
                        
                        if (!equationPresented) // '=' sign input
                        {
                            if (j >= partsCount / 2)
                            {
                                part += "=";
                                if (j == partsCount - 1) // add 0 to the end if last part looks like [+2x=]
                                    part += "0";
                                equationPresented = true;
                            }
                        }

                        if (wholeStr.EndsWith('=') && part.StartsWith('*')) // trim * on first part after '='
                            part = part.Remove(0, 1);
                        
                        wholeStr += part;
                        //partsList.Add(part); //TODO: remove
                    }

                    writer.WriteLine(wholeStr.Replace(',', '.'));
                    //TODO: remove
                    // writer.Write(wholeStr.Replace(',', '.'));
                    // writer.Write("\t\t\t\t\t\t\t");
                    // foreach (var part in partsList)
                    //     writer.Write("[" + part + "]");
                    // writer.WriteLine();
                    // partsList.Clear();
                }
            }
        }
    }
}