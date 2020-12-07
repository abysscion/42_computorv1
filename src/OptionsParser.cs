using System;

namespace computorv1
{
    internal class OptionsParser
    {
        public bool RndFlagSet { get; private set; }
        public bool SFlagSet { get; private set; }
        public bool RFlagSet { get; private set; }
        public bool FFlagSet { get; private set; }
        public int RndEquationsCount { get; private set; }

        public void Parse(string[] opts)
        {
            SFlagSet = false;
            RFlagSet = false;
            RndFlagSet = false;
            RndEquationsCount = 0;
            
            foreach (var opt in opts)
            {
                if (opt.StartsWith("-rnd:"))
                {
                    if (RndFlagSet)
                        throw new Exception("-rnd flag is already set.");
                    SetRndFlag(opt);
                }
                else
                {
                    switch (opt)
                    {
                        case "-s":
                            if (SFlagSet)
                                throw new Exception("-s flag is already set.");
                            SFlagSet = true;
                            break;
                        case "-f":
                            if (FFlagSet)
                                throw new Exception("-f flag is already set.");
                            FFlagSet = true;
                            break;
                        case "-r":
                            if (RFlagSet)
                                throw new Exception("-r flag is already set.");
                            RFlagSet = true;
                            break;
                        default:
                            throw new Exception("unknown flag provided: " + opt);
                    }
                }
            }
        }

        private void SetRndFlag(string opt)
        {
            try
            {
                var numericPart = opt.Substring(5);
                var numVal = int.Parse(numericPart);
                
                if (numVal <= 0 || numVal > 100)
                    throw new Exception("-rnd flag value should be integer number in range (1, 100) inclusive.");
                RndFlagSet = true;
                RndEquationsCount = numVal;
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
    }
}
