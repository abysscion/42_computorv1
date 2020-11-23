using System;
using System.Collections.Generic;

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
            OneRoot,
            TwoRoots
        }

        public SolutionTypes SolutionType { get; private set; }
        public List<string> SolvingSteps { get; }
        public List<string> Roots { get; }
        public double Discriminant { get; private set; }
        public int Degree { get; private set; }

        public EquationSolver(string equ, bool doNotNotReduceFraction = false)
        {
            SolvingSteps = new List<string>();
            Roots = new List<string>();
            _equation = equ;
            _shouldNotReduceFraction = doNotNotReduceFraction;
        }

        public void Solve()
        {
            double a, b, c, sqrD;

            if (!DegreeCheck())
                return;
            if (Degree == 0)
            {
                SolutionType = SolutionTypes.Any;
                return;
            }

            EquationParser.SetCoefficients(out a, out b, out c, ref _equation);
            SolvingSteps.Add($"[Reading coefficients]\t\ta = {a}, b = {b}, c = {c}");
            if (a == 0)
            {
                SolvingSteps.Add(
                    "\"a\" coefficient is 0, so the Computorv1 stops to prevent universe collapsing because of division by zero...");
                return;
            }

            Discriminant = b * b - 4 * a * c;
            SolvingSteps.Add($"[Calculating discriminant]\tD = b^2 - 4ac = {b}^2 - 4 * {a} * {c} = {Discriminant}");
            if (Discriminant >= 0)
            {
                sqrD = ShitMath.Sqrt(Discriminant);
                Roots.Add(_shouldNotReduceFraction ? "" + (-b + sqrD + "/" + 2 * a) : "" + (-b + sqrD) / (2 * a));
                SolvingSteps.Add(
                    $"[Calculating first root]\tx0 = (-b + sqrt(D)) / 2a = ({-b} + {sqrD}) / {2 * a} = {Roots[0]}");
                if (Discriminant > 0)
                {
                    Roots.Add(_shouldNotReduceFraction ? "" + (-b - sqrD + "/" + 2 * a) : "" + (-b - sqrD) / (2 * a));
                    SolvingSteps.Add(
                        $"[Calculating second root]\tx0 = (-b - sqrt(D)) / 2a = ({-b} - {sqrD}) / {2 * a} = {Roots[1]}");
                }
            }
            else
            {
                Discriminant = -Discriminant;
                sqrD = ShitMath.Sqrt(Discriminant);
                var real = -b / (2 * a);
                var imaginary = ShitMath.Abs(sqrD / (2 * a));
                var rStr = _shouldNotReduceFraction ? -b + "/" + 2 * a : "" + real;
                var iStr = _shouldNotReduceFraction ? sqrD + "/" + 2 * a : "" + imaginary;

                SolvingSteps.Add($"[Real part of roots]\t\tr = -b / 2a = {-b} / {2 * a} = {rStr}");
                SolvingSteps.Add($"[Imaginary part of roots]\ti = sqrt(D) / 2a = {sqrD} / {2 * a} = {iStr}");
                SolvingSteps.Add($"[Calculating first root]\tx0 = r - i = {rStr} - {iStr}i");
                SolvingSteps.Add($"[Calculating second root]\tx1 = r + i = {rStr} + {iStr}i");
                Roots.Add(rStr + " - " + iStr + "i");
                Roots.Add(rStr + " + " + iStr + "i");
            }
        }

        private bool DegreeCheck()
        {
            Degree = EquationParser.GetPolynomialDegree(_equation);
            
            if (Degree < 0 || Degree > 2)
                throw new Exception("sorry, but polynomial degree should be in range of [0, 2].");
            if (Degree == 0 && (_equation.Length != 3 || _equation[0] != _equation[2]))
            {
                SolutionType = SolutionTypes.None;
                return false;
            }

            return true;
        }
    }
}