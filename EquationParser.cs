using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace computorv1
{
    internal static class EquationParser
    {
        private static readonly Regex MultipleExesConcatedRegex = new Regex(@"(\d+(\.\d+)?)?\*?x(\d+(\.\d+)?)?x");
        private static readonly Regex ForbiddenSymbolsRegex = new Regex(@"[^xX\.\-\+\*\=\^\d\s]");
        private static readonly Regex PartWithoutExRegex = new Regex(@"^[\+\-]?\d+(\.\d+)?(\^[\+\-]?\d+)?");
        private static readonly Regex PartWithExRegex = new Regex(@"^[\+\-]?(\d+(\.\d+)?)?\*?[xX](\^[\+\-]?\d+)?");

        public static void Parse(string equation)
        {
            var trimmedEqu = Regex.Replace(equation, @"\s+", "").ToLower();

            trimmedEqu = RemoveMultipleZeroes(ref trimmedEqu);
            CheckForInvalidSymbols(ref trimmedEqu);
            CheckForMultipleConcatedExes(ref trimmedEqu);
            trimmedEqu = SimplifySigns(ref trimmedEqu);
            trimmedEqu = MoveRightPartsToLeft(ref trimmedEqu); //do not change order
            trimmedEqu = RemoveMultiplyBy0(ref trimmedEqu);
            //remove useless powers ^0 ^1
            trimmedEqu = CalculatePowers(ref trimmedEqu);
            //calculate nums
            //calculate exes
            
            Console.WriteLine(trimmedEqu);
        }

        // private static string ReduceReducible(ref string trimmedEqu)
        // {
        //     var parts = SplitEquationByParts(ref trimmedEqu);
        //
        //     foreach (var VARIABLE in Regex.Matches())
        //     {
        //         
        //     }
        //     var sumsParts = 
        // }

        private static string CalculatePowers(ref string trimmedEqu)
        {
            // var parts = SplitEquationByParts(ref trimmedEqu);
            // var result = "";
            //
            // for (var i = 0; i < parts.Count; i++)
            // {
            //     if (!PartWithoutExRegex.IsMatch(parts[i]))
            //         continue;         
            //     if (!parts[i].Contains('^'))
            //         continue;
            //     if (parts[i][parts[i].IndexOf('^') + 1] == '-')  
            //         continue;
            //     var pieces = parts[i].Split('^');
            //     int lPiece, rPiece;
            //     double ldPiece;
            //     
            //     if (!int.TryParse(pieces[1], out rPiece))
            //         throw new Exception($"can't parse power [{pieces[1]}] as int in [{parts[i]}].");
            //     if (int.TryParse(pieces[0], out lPiece))
            //         parts[i] = "" + shitPow(lPiece, rPiece);
            //     else if (Double.TryParse(pieces[0], out ldPiece))
            //         parts[i] = "" + Math.Pow(ldPiece, rPiece);
            //     else
            //         throw new Exception($"can't parse value [{pieces[0]}] as int or double in [{parts[i]}].");
            // }
            //
            // foreach (var part in parts) 
            //     result += part;
            //
            // return result + "=0";

            var str = trimmedEqu.Substring(0);
            var match = Regex.Match(str, @"[\+\-]?\d+(\.\d+)?\^\+?\d+");

            if (!match.Success)
                return trimmedEqu;
            do
            {
                var splitPow = match.Value.Split('^');
                var sign = splitPow[0][0] == '-' ? "-" : splitPow[0][0] == '+' ? "+" : ""; 
                string result;
                int lPiece, rPiece;
                double ldPiece;

                splitPow[0] = Regex.Replace(splitPow[0], @"^[\+\-]", "");
                if (!int.TryParse(splitPow[1], out rPiece))
                    throw new Exception($"can't parse power [{splitPow[1]}] as int in [{match.Value}].");
                if (int.TryParse(splitPow[0], out lPiece))
                    result = "" + shitPow(lPiece, rPiece);
                else if (Double.TryParse(splitPow[0], out ldPiece))
                    result = "" + Math.Pow(ldPiece, rPiece);
                else
                    throw new Exception($"can't parse value [{splitPow[0]}] as int or double in [{match.Value}].");
                str = str.Remove(match.Index, match.Length).Insert(match.Index, sign + result);
            } while ((match = Regex.Match(str, @"[\+\-]?\d+(\.\d+)?\^\+?\d+")).Success);

            return str;
        }

        private static int shitPow(long value, int power)
        {
            if (value > int.MaxValue)
                throw new Exception("can't work with numbers bigger than INT_MAX: " + value + "^" + power);
            
            long ret = 1;
            var x = value;
            while (power != 0)
            {
                if ((power & 1) == 1)
                    ret *= x;
                x *= x;
                if (x > int.MaxValue)
                    throw new Exception("can't work with numbers bigger than INT_MAX: " + value + "^" + power);
                power >>= 1;
            }
            return (int)ret;
        }

        private static string RemoveMultipleZeroes(ref string trimmedEqu)
        {
            var match = Regex.Match(trimmedEqu, @"[\+\-\*\.\^](0{2,})[\+\-\*\.\^]");
            var newEqu = trimmedEqu.Substring(0);

            if (!match.Success)
                return trimmedEqu;
            do
            {
                newEqu = newEqu.Remove(match.Index + 2, match.Length - 3);
            } while ((match = Regex.Match(newEqu, @"[\+\-\*\.\^](0{2,})[\+\-\*\.\^]")).Success);

            return newEqu;
        }

        private static string RemoveMultiplyBy0(ref string trimmedEqu)
        {
            var parts = SplitEquationByParts(ref trimmedEqu);
            var result = "";

            for (var i = 0; i < parts.Count; i++)
            {
                if (!Regex.Match(parts[i], @"([^\^]0\*)|(\*\-?0)|(0x)").Success)
                    continue;
                parts.RemoveAt(i);
                i--;
            }

            foreach (var part in parts)
                result += part;

            return result + "=0";
        }
        
        private static List<string> SplitEquationByParts(ref string trimmedEqu)
        {
            var parts = new List<string>();
            var str = trimmedEqu.Substring(0);

            while (str.Length != 2) // last 2 symbols should be "=0"
            {
                var regMatch = PartWithExRegex.Match(str);
                if (!regMatch.Success)
                    regMatch = PartWithoutExRegex.Match(str);
                if (!regMatch.Success && str.Length != 2)
                    throw new Exception("debug me man");
                str = str.Remove(0, regMatch.Length);
                parts.Add(regMatch.Value);
                if (str.Length > 2)
                {
                    while (str[0] == '*')
                    {
                        var tmpEqu = str.Substring(1);
                        var tmpMatch = PartWithExRegex.Match(tmpEqu);
                        if (!tmpMatch.Success)
                            tmpMatch = PartWithoutExRegex.Match(tmpEqu);
                        if (!tmpMatch.Success)
                            break;
                        parts[^1] += "*" + tmpMatch.Value;
                        str = str.Remove(0, tmpMatch.Length + 1); //+1 because of *
                    }
                }
            }

            return parts;
        }

        private static string SimplifySigns(ref string trimmedEqu)
        {
            var simplifiedString = trimmedEqu.Replace("--", "+");
            simplifiedString = simplifiedString.Replace("++", "+");
            simplifiedString = simplifiedString.Replace("-+", "-");
            simplifiedString = simplifiedString.Replace("+-", "-");
            if (simplifiedString[0] == '+')
                simplifiedString = simplifiedString.Remove(0, 1);
            return simplifiedString;
        }

        private static void CheckForInvalidSymbols(ref string str)
        {
            var match = ForbiddenSymbolsRegex.Match(str);
            if (!match.Success)
                return;
            throw new Exception("invalid symbol.\n" + str + "\n" + "^".PadLeft(match.Index + 1));
        }
        
        private static void CheckForMultipleConcatedExes(ref string str)
        {
            var match = MultipleExesConcatedRegex.Match(str);
            if (!match.Success)
                return;
            throw new Exception("badly concatenated exes.\n" + str + "\n" + "^".PadLeft(match.Index + 1));
        }
        
        private static string MoveRightPartsToLeft(ref string trimmedEquation)
        {
            var sides = trimmedEquation.Split('=');
                
            if (sides.Length != 2)
                throw new Exception("equation should have only one equals sign.\n" +
                                    trimmedEquation + "\n" +
                                    "^".PadLeft(trimmedEquation.Length - sides[1].Length + 1));
            if (sides[0].Length == 0 || sides[1].Length == 0)
                throw new Exception("left and right side of equation shouldn't be empty.");
            
            while (true)
            {
                var match = PartWithExRegex.Match(sides[1]);
                if (!match.Success)
                    match = PartWithoutExRegex.Match(sides[1]);
                if (match.Success)
                {
                    var part = SwapPartSigns(match.Value);
                    
                    sides[1] = sides[1].Remove(0, match.Length);
                    while (sides[1].Length > 0 && sides[1][0] == '*')
                    {
                        var tmpSide = sides[1].Substring(1);
                        var tmpMatch = PartWithExRegex.Match(tmpSide);
                        if (!tmpMatch.Success)
                            tmpMatch = PartWithoutExRegex.Match(tmpSide);
                        if (!tmpMatch.Success)
                            break;
                        part += "*" + tmpMatch.Value;
                        sides[1] = sides[1].Remove(0, tmpMatch.Length + 1); //+1 because of *
                    }
                    sides[0] += part;
                    continue;
                }

                if (sides[1].Length < 1)
                    break;

                throw new Exception("there's error in this equation:\n" +
                                    trimmedEquation + "\n" +
                                    "^".PadLeft(trimmedEquation.Length - sides[1].Length + 1));
            }

            return sides[0] + "=0";
        }

        private static string SwapPartSigns(string part)
        {
            var pieces = part.Split('^');
            part = pieces[0];
            
            if (part[0].Equals('-'))
                part = part.Replace('-', '+');
            else if (part[0].Equals('+'))
                part = part.Replace('+', '-');
            else
                part = part.Insert(0, "-");
            return pieces.Length == 1 ? part : part + "^" + pieces[1];
        }
    }
}