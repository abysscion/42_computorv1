using System;
using System.Text.RegularExpressions;

namespace computorv1
{
    internal static class EquationParser
    {
        private static readonly Regex PermittedSymbolsRegex = new Regex(@"[xX\.\-\+\*\=\^\d\s]");
        private static readonly Regex PartWithoutExRegex = new Regex(@"^[\+\-]?\d+(\.\d+)?(\^[\+\-]?\d+)?");
        private static readonly Regex PartWithExRegex = new Regex(@"^[\+\-]?(\d+(\.\d+)?)?\*?[xX](\^[\+\-]?\d+)?");

        public static void Parse(string equation)
        {
            var trimmedEqu = Regex.Replace(equation, @"\s+", "");
            
            if (!PermittedSymbolsRegex.IsMatch(trimmedEqu))
                throw new Exception("equation has invalid symbol.");

            var sides = trimmedEqu.ToLower().Split('=');
            if (sides.Length != 2)
                throw new Exception("equation should have only one equals sign");
            if (sides[0].Length == 0 || sides[1].Length == 0)
                throw new Exception("left and right side of equation shouldn't be empty");

            while (true)
            {
                var match = PartWithExRegex.Match(sides[1]);
                if (!match.Success)
                    match = PartWithoutExRegex.Match(sides[1]);
                if (match.Success)
                {
                    var str = match.Value;
                    
                    sides[1] = sides[1].Remove(0, match.Length);
                    if (str.Contains('^'))
                    {
                        var pieces = str.Split('^');
                        str = SwapPieceSign(pieces[0]) + "^" + SwapPieceSign(pieces[1]);
                    }
                    else
                        str = SwapPieceSign(str);

                    sides[0] += str;
                    continue;
                }

                if (sides[1].Length < 1)
                    break;

                throw new Exception("there's error in this equation:\n" +
                                    trimmedEqu + "\n" +
                                    "^".PadLeft(trimmedEqu.Length - sides[1].Length + 1));
            }
            
            //PartWithExRegex
            //PartWithoutExRegex

            Console.WriteLine(equation);
        }

        
        
        private static string SwapPieceSign(string piece)
        {
            if (piece[0].Equals('-'))
                piece = piece.Replace('-', '+');
            else if (piece[0].Equals('+'))
                piece = piece.Replace('+', '-');
            else
                piece = piece.Insert(0, "-");
            return piece;
        }
    }
}