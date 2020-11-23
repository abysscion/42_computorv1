using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace computorv1
{
    internal static class EquationParser
    {
        private static readonly List<string> ReductionSteps = new List<string>();
        private static readonly Regex MultipleExesConcatedRegex = new Regex(@"(\d+(\.\d+)?)?\*?x(\d+(\.\d+)?)?x");
        private static readonly Regex ForbiddenSymbolsRegex = new Regex(@"[^xX\.\-\+\*\=\^\d\s]");
        private static readonly Regex PartWithoutExRegex = new Regex(@"^[\+\-]?\d+(\.\d+)?(\^[\+\-]?\d+)?");
        private static readonly Regex PartWithExRegex = new Regex(@"^[\+\-]?(\d+(\.\d+)?)?\*?[xX](\^[\+\-]?\d+)?");
        
        public static string Parse(string equation)
        {
            var trimmedEqu = Regex.Replace(equation, @"\s+", "").ToLower();

            CheckForInvalidEquation(ref trimmedEqu);
            CheckForMultipleConcatedExes(ref trimmedEqu);
            trimmedEqu = SimplifySigns(ref trimmedEqu);
            ReductionSteps.Add(trimmedEqu);
            trimmedEqu = RemoveMultipleZeroes(ref trimmedEqu);
            ReductionSteps.Add(trimmedEqu);
            trimmedEqu = MoveRightPartsToLeft(ref trimmedEqu);
            ReductionSteps.Add(trimmedEqu);
            trimmedEqu = RemoveMultiplyBy0(ref trimmedEqu);
            ReductionSteps.Add(trimmedEqu);
            trimmedEqu = CalculatePowers(ref trimmedEqu);
            ReductionSteps.Add(trimmedEqu);
            trimmedEqu = CalculateMultiplies(ref trimmedEqu);
            ReductionSteps.Add(trimmedEqu);
            trimmedEqu = RemoveUselessPowers(ref trimmedEqu);
            ReductionSteps.Add(trimmedEqu);
            trimmedEqu = CalculateSums(ref trimmedEqu);
            ReductionSteps.Add(trimmedEqu);

            return trimmedEqu;
        }
        
        public static int GetPolynomialDegree(string equ)
        {
            if (!equ.Contains('x'))
                return 0;
            
            var parts = SplitEquationByParts(ref equ);
            var degree = int.MinValue;

            for (var i = 0; i < parts.Count; i++)
            {
                if (!parts[i].Contains('x'))
                    continue;

                var part = parts[i];
                var tmp = GetExPower(ref part);

                if (tmp > degree)
                    degree = tmp;
            }

            return degree;
        }

        public static void SetCoefficients(out double a, out double b, out double c, ref string equation)
        {
            var equParts = SplitEquationByParts(ref equation);
            
            a = 0;
            b = 0;
            c = 0;
            foreach (var part in equParts)
            {
                if (part.Contains("x^2"))
                {
                    if (part.Length != 3)
                    {
                        if (!double.TryParse(part.Substring(0, part.IndexOf('x')), out a))
                            throw new Exception($"can't parse value of [{part}] as number.");
                    }
                    else
                        a = 1;
                }
                else if (part.Contains('x'))
                {
                    if (part.Length != 1)
                    {
                        if (!double.TryParse(part.Substring(0, part.IndexOf('x')), out b))
                            throw new Exception($"can't parse value of [{part}] as number.");
                    }
                    else
                        b = 1;
                }
                else
                {
                    if (!double.TryParse(part, out c))
                        throw new Exception($"can't parse value of [{part}] as number.");
                }
            }
        }

        public static void PrintReductionSteps()
        {
            if (ReductionSteps.Count == 0)
                return;
            
            Console.WriteLine($"[Simplifying signs]\t\t {ReductionSteps[0]}");
            Console.WriteLine($"[Removing multiple zeroes]\t {ReductionSteps[1]}");
            Console.WriteLine($"[Moving right part to left]\t {ReductionSteps[2]}");
            Console.WriteLine($"[Removing multiplies by zero]\t {ReductionSteps[3]}");
            Console.WriteLine($"[Calculating powers]\t\t {ReductionSteps[4]}");
            Console.WriteLine($"[Calculating multiplyings]\t {ReductionSteps[5]}");
            Console.WriteLine($"[Replacing ^0 and ^1]\t\t {ReductionSteps[6]}");
            Console.WriteLine($"[Calculating summaries]\t\t {ReductionSteps[7]}");
        }
        
        private static string CalculateSums(ref string trimmedEqu)
        {
            var result = "";
            var parts = SplitEquationByParts(ref trimmedEqu);
            var exlessSum = 0.0;
            
            for (var curI = 0; curI < parts.Count - 1; curI++)
            {
                if (!parts[curI].Contains('x'))
                    continue;
                var currentEx = parts[curI];
                var currentPower = GetExPower(ref currentEx);

                for (var nextI = curI + 1; nextI < parts.Count; nextI++)
                {
                    if (!parts[nextI].Contains('x'))
                        continue;
                    var nextEx = parts[nextI];

                    if (currentPower != GetExPower(ref nextEx))
                        continue;

                    parts[curI] = SumExes(parts[curI], nextEx);
                    parts.RemoveAt(nextI);
                    if (parts[curI] == "+0")
                        break;
                    nextI--;
                }
            }
            
            for (var i = parts.Count - 1; i >= 0; i--)
            {
                if (parts[i].Contains('x'))
                    continue;
                if (!double.TryParse(parts[i], out var part))
                    throw new Exception($"unable to parse {parts[i]} as number.");
                exlessSum += part;
                if (ShitMath.Abs(exlessSum) > int.MaxValue)
                    throw new Exception($"unable to work with numbers larger than INT_MAX: {parts[i]}.");
                parts.RemoveAt(i);
            }

            var newParts = parts.ToArray();
            Array.Sort(newParts, (s, s1) => GetExPower(ref s1) - GetExPower(ref s));
            foreach (var part in newParts)
                result += part;
            if (result.Length > 0)
            {
                result = result[0] == '+' ? result.Remove(0, 1) : result;
                if (exlessSum != 0)
                {
                    result += (exlessSum > 0 ? "+" : "") + exlessSum;
                }
            }
            else
                result += exlessSum;

            return result + "=0";
        }

        private static string SumExes(string lX, string rX)
        {
            double lVal, rVal;
            var power = GetExPower(ref lX);

            if (power != GetExPower(ref rX))
                return "";

            if (lX[0] != '-' && lX[0] != '+')
                lX = lX.Insert(0, "+");
            if (lX.IndexOf('x') <= 1)
            {
                lVal = 1.0;
                lVal = lX[0] == '-' ? lVal * -1.0 : 1.0;
            }
            else
            {
                if (!double.TryParse(lX.Substring(0, lX.IndexOf('x')), out lVal))
                    throw new Exception($"can't parse value of [{lX}] as number.");
            }
            
            if (rX.IndexOf('x') <= 1)
            {
                rVal = 1.0;
                rVal = rX[0] == '-' ? rVal * -1.0 : 1.0;
            }
            else
            {
                if (!double.TryParse(rX.Substring(0, rX.IndexOf('x')), out rVal))
                    throw new Exception($"can't parse value of [{rX}] as number.");
            }

            var sum = lVal + rVal;
            if (sum == 0.0)
                return "+0";
            if (ShitMath.Abs(sum) > int.MaxValue)
                throw new Exception($"unable to work with numbers larger than INT_MAX: {lVal} + {rVal} = {sum}.");
            
            var result = sum > 0.0 ? "+" : "-";
            result += ShitMath.Abs(sum - 1.0) < 0.000000001 ? "" : "" + ShitMath.Abs(sum);

            return  result + 'x' + (power == 1 ? "" : "^" + power);
        }
        
        private static int GetExPower(ref string exString)
        {
            if (!exString.Contains('^'))
                return 1;

            var powerStr = exString.Substring(exString.IndexOf('^') + 1);
            return !int.TryParse(powerStr, out var power) ? 1 : power;
        }
        
        private static string CalculateMultiplies(ref string trimmedEqu)
        {
            var parts = SplitEquationByParts(ref trimmedEqu);
            var result = "";
            
            for (var i = 0; i < parts.Count; i++)
            {
                if (!parts[i].Contains('*'))
                    continue;
                var factors = parts[i].Split('*');

                for (var j = factors.Length - 2; j >= 0; j--)
                {
                    var lxHolder = "";
                    var rxHolder = "";
                    double lFactor = 1.0, rFactor = 1.0;

                    if (factors[j].Contains('x'))
                    {
                        lxHolder = factors[j].Substring(factors[j].IndexOf('x'));
                        factors[j] = factors[j].Remove(factors[j].IndexOf('x'));
                        if (factors[j].Length == 0)
                            factors[j] = "1";
                        else if (factors[j].Length == 1 && (factors[j][0] == '+' || factors[j][0] == '-'))
                            factors[j] += "1"; 
                    }
                    
                    if (factors[j + 1].Contains('x'))
                    {
                        rxHolder = factors[j + 1].Substring(factors[j + 1].IndexOf('x'));
                        factors[j + 1] = factors[j + 1].Remove(factors[j + 1].IndexOf('x'));
                        if (factors[j + 1].Length == 0)
                            factors[j + 1] = "1";
                        else if (factors[j + 1].Length == 1 && (factors[j + 1][0] == '+' || factors[j + 1][0] == '-'))
                            factors[j + 1] += "1"; 
                    }

                    if (factors[j].Length > 0)
                        if (!double.TryParse(factors[j], out lFactor))
                            throw new Exception($"can't parse factor [{factors[j]}] as number or x container.");

                    if (factors[j + 1].Length > 0)
                        if (!double.TryParse(factors[j + 1], out rFactor))
                            throw new Exception($"can't parse factor [{factors[j + 1]}] as number or x container.");
                    
                    var multiRes = lFactor * rFactor;
                    factors[j] = (multiRes >= 0.0 ? "+" : "") + multiRes + MultiplyExes(lxHolder, rxHolder);
                }

                parts[i] = factors[0];
            }

            foreach (var part in parts)
                result += part;

            return result + "=0";
        }

        private static string MultiplyExes(string lX, string rX)
        {
            var lP = 1;
            var rP = 1;

            if (lX.Length == 0 || rX.Length == 0)
                return lX + rX;
            if (lX.Contains('^'))
                if (!int.TryParse(lX.Substring(lX.IndexOf('^') + 1), out lP))
                    throw new Exception($"can't parse power of [{lX}] as integer.");
            if (rX.Contains('^'))
                if (!int.TryParse(rX.Substring(rX.IndexOf('^') + 1), out rP))
                    throw new Exception($"can't parse power of [{rX}] as integer.");
            return "x^" + (lP + rP);
        }
        
        private static string RemoveUselessPowers(ref string trimmedEqu)
        {
            var str = trimmedEqu.Substring(0);
            var match = Regex.Match(str, @"[\+\-]?(((\d+(\.\d+)?)?\*?x)|(\d+(\.\d+)?))\^[\+\-]?\d+");

            if (!match.Success)
                return trimmedEqu;
            do
            {
                var splitPow = match.Value.Split('^');
                var sign = splitPow[0][0] == '-' ? "-" : splitPow[0][0] == '+' ? "+" : ""; 
                string result;

                splitPow[0] = Regex.Replace(splitPow[0], @"^[\+\-]", "");
                if (!int.TryParse(splitPow[1], out var rPiece))
                    throw new Exception($"can't parse power [{splitPow[1]}] as int in [{match.Value}].");
                if (rPiece == 0)
                    result = "1";
                else if (rPiece == 1)
                    result = splitPow[0];
                else
                    result = splitPow[0] + '@' + splitPow[1];
                str = str.Remove(match.Index, match.Length).Insert(match.Index, sign + result);
            } while ((match = Regex.Match(str, @"[\+\-]?(((\d+(\.\d+)?)?\*?x)|(\d+(\.\d+)?))\^[\+\-]?\d+")).Success);

            str = str.Replace('@', '^');
            return str;
        }
        
        private static string CalculatePowers(ref string trimmedEqu)
        {
            var str = trimmedEqu.Substring(0);
            var match = Regex.Match(str, @"[\+\-]?\d+(\.\d+)?\^[\+\-]?\d+");

            if (!match.Success)
                return trimmedEqu;
            do
            {
                var splitPow = match.Value.Split('^');
                var sign = splitPow[0][0] == '-' ? "-" : splitPow[0][0] == '+' ? "+" : ""; 
                string result;

                splitPow[0] = Regex.Replace(splitPow[0], @"^[\+\-]", "");
                if (!int.TryParse(splitPow[1], out var rPiece))
                    throw new Exception($"can't parse power [{splitPow[1]}] as integer in [{match.Value}].");
                if (double.TryParse(splitPow[0], out var ldPiece))
                {
                    var value = ShitMath.Pow(ldPiece, rPiece);
                    result = "" + value;
                    if (value != 0 && (!double.IsNormal(value) ||
                                       result.Contains('e', StringComparison.OrdinalIgnoreCase) ||
                                       ShitMath.Abs(value) > int.MaxValue))
                        throw new Exception($"value {ldPiece} powered by {rPiece} is too big or too small: {value}");
                }
                else
                    throw new Exception($"can't parse value [{splitPow[0]}] as number in [{match.Value}].");
                str = str.Remove(match.Index, match.Length).Insert(match.Index, sign + result);
            } while ((match = Regex.Match(str, @"[\+\-]?\d+(\.\d+)?\^[\+\-]?\d+")).Success);

            return str;
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

            if (parts.Count > 0)
            {
                if (parts[0][0] != '-' && parts[0][0] != '+')
                    parts[0] = parts[0].Insert(0, "+");
            }
            for (var i = 0; i < parts.Count; i++)
            {
                if (!Regex.Match(parts[i], @"([\+\-]?\*((0\.0)|(0))\*?)|([\+\-\*]((0\.0)|(0))x)").Success)
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
                    throw new Exception($"invalid part presented in \"{str}\".");
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
            if (trimmedEqu[0] != '+' || trimmedEqu[0] != '-')
                trimmedEqu = trimmedEqu.Insert(0, "+");
            return simplifiedString;
        }

        private static void CheckForInvalidEquation(ref string str)
        {
            var match = ForbiddenSymbolsRegex.Match(str);
            if (match.Success)
                throw new Exception("invalid symbol.\n" + str + "\n" + "^".PadLeft(match.Index + 1));
            match = Regex.Match(str, @"x\d+");
            if (match.Success)
                throw new Exception("invalid symbol.\n" + str + "\n" + "^".PadLeft(match.Index + 1));
            match = Regex.Match(str, @"[\+\-\*\^\.]=");
            if (match.Success)
                throw new Exception("invalid symbol.\n" + str + "\n" + "^".PadLeft(match.Index + 1));
            match = Regex.Match(str, @"[\+\-]?\d+(\.\d+)?(\^[\+\-]?\d+[xX])");
            if (match.Success)
                throw new Exception("invalid part.\n" + str + "\n" + "^".PadLeft(match.Index + 1));
            if (!str.Contains('='))
                throw new Exception("not an equation.");
            if (str.IndexOf('=') == 0 || str.IndexOf('=') == str.Length - 1)
                throw new Exception("not an equation.");
            if (str.LastIndexOf('=') != str.IndexOf('='))
                throw new Exception("too many equals signs.\n" + str + "\n" + "^".PadLeft(str.LastIndexOf('=')));
            if (!str.Contains('x'))
            {
                match = Regex.Match(str, @"\d");
                if (!match.Success)
                    throw new Exception("there are no exes and there are no numbers. Am I joke to you?");
            }
            var leftPart = str.Substring(0, str.IndexOf('='));
            var rightPart = str.Substring(str.IndexOf('=') + 1, str.Length - 1 - leftPart.Length);
            if (!PartWithExRegex.Match(leftPart).Success && !PartWithoutExRegex.Match(leftPart).Success)
                throw new Exception("left half of equation does not contains any valid part.");
            if (!PartWithExRegex.Match(rightPart).Success && !PartWithoutExRegex.Match(rightPart).Success)
                throw new Exception("right half of equation does not contains any valid part.");
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