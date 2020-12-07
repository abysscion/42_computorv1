using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace computorv1
{
    public class EquationSolver
    {
        private readonly bool _shouldNotReduceFraction;
        private string _equation;

        public enum SolutionTypes
        {
            Any,
            None,
        }

        public SolutionTypes SolutionType { get; private set; }
        public List<string> Steps { get; }
        public List<string> Roots { get; }
        public double Discriminant { get; private set; }
        public int Degree { get; private set; }

        public EquationSolver(string equ, bool doNotNotReduceFraction = false)
        {
            Steps = new List<string>();
            Roots = new List<string>();
            _equation = equ;
            _shouldNotReduceFraction = doNotNotReduceFraction;
        }

        public void Solve()
        {
            double a, b, c;

            if (!DegreeCheck())
                return;
            if (Degree == 0)
            {
                SolutionType = SolutionTypes.Any;
                return;
            }

            EquationParser.SetCoefficients(out a, out b, out c, ref _equation);
            Steps.Add($"[Reading coefficients]\t\ta = {a}, b = {b}, c = {c}");

            if (a == 0)
            {
                if (c == 0)
                    Roots.Add("0");
                else
                    SolveLinear(b, c);

                return;
            }

            Discriminant = b * b - 4 * a * c;
            Steps.Add($"[Calculating discriminant]\tD = b^2 - 4ac = {b}^2 - 4 * {a} * {c} = {Discriminant}");
            if (Discriminant >= 0)
                SolveQuadratic(a, b);
            else
                SolveComplex(a, b);
        }

        private void SolveLinear(double b, double c)
        {
            var rootVal = -c / b;
            var rootStr = _shouldNotReduceFraction ? -c + "/" + b : "" + rootVal;

            if (Regex.Match(rootStr, @"\-.+\/\-.+").Success)
                rootStr = rootStr.Replace("-", "");
            Steps.Add($"[Calculating root]\t\tx = -c/b = {-c}/{b} = {rootStr}");
            Roots.Add(rootStr);
        }

        private void SolveComplex(double a, double b)
        {
            var sqrD = ShitMath.Sqrt(-Discriminant);
            var real = -b / (2 * a);
            var imaginary = ShitMath.Abs(sqrD / (2 * a));
            var rStr = _shouldNotReduceFraction ? -b + "/" + 2 * a : "" + real;
            var iStr = _shouldNotReduceFraction ? sqrD + "/" + 2 * a : "" + imaginary;

            Steps.Add($"[Real part of roots]\t\tr = -b / 2a = {-b} / {2 * a} = {rStr}");
            Steps.Add($"[Imaginary part of roots]\ti = sqrt(D) / 2a = {sqrD} / {2 * a} = {iStr}");
            Steps.Add($"[Calculating first root]\tx0 = r - i = {rStr} - {iStr}i");
            Steps.Add($"[Calculating second root]\tx1 = r + i = {rStr} + {iStr}i");
            if (Regex.Match(rStr, @"\-.+\/\-.+").Success)
                rStr = rStr.Replace("-", "");
            if (Regex.Match(iStr, @"\-.+\/\-.+").Success)
                iStr = iStr.Replace("-", "");
            Roots.Add(rStr + " - " + iStr + "i");
            Roots.Add(rStr + " + " + iStr + "i");
        }

        private void SolveQuadratic(double a, double b)
        {
            var sqrD = ShitMath.Sqrt(Discriminant);
            var rootVal = (-b + sqrD) / (2 * a);
            var rootStr = _shouldNotReduceFraction ? "" + (-b + sqrD + "/" + 2 * a) : "" + rootVal;

            if (Regex.Match(rootStr, @"\-.+\/\-.+").Success)
                rootStr = rootStr.Replace("-", "");
            Steps.Add(
                $"[Calculating first root]\tx0 = (-b + sqrt(D)) / 2a = ({-b} + {sqrD}) / {2 * a} = {rootStr}");
            Roots.Add(rootStr);

            if (Discriminant > 0)
            {
                rootVal = (-b - sqrD) / (2 * a);
                rootStr = _shouldNotReduceFraction ? "" + (-b - sqrD + "/" + 2 * a) : "" + rootVal;
                if (Regex.Match(rootStr, @"\-.+\/\-.+").Success)
                    rootStr = rootStr.Replace("-", "");
                Steps.Add(
                    $"[Calculating second root]\tx0 = (-b - sqrt(D)) / 2a = ({-b} - {sqrD}) / {2 * a} = {rootStr}");
                Roots.Add(rootStr);
            }
        }

        private bool DegreeCheck()
        {
            var match = Regex.Match(_equation, @"x\^\-");
            if (match.Success)
            {
                throw new Exception("reduced form should not contain negative powers of x.\n" + _equation + "\n" +
                                    "^".PadLeft(match.Index + 1));
            }

            Degree = EquationParser.GetPolynomialDegree(_equation);

            if (Degree < 0 || Degree > 2)
                throw new Exception("polynomial degree should be in range of [0, 2].");
            if (Degree == 0 && (_equation.Length != 3 || _equation[0] != _equation[2]))
            {
                SolutionType = SolutionTypes.None;
                return false;
            }

            return true;
        }
    }
}
