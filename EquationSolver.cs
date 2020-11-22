using System.Collections.Generic;

namespace computorv1
{
    public class EquationSolver
    {
        public IReadOnlyList<string> solvingSteps { get; private set; }
        public double root0 { get; private set; }
        public double root1 { get; private set; }
        public int rootsCount { get; private set; }

        public void Solve(string equ, bool showSolvingSteps = false)
        {
            throw new System.NotImplementedException();
        }
    }
}