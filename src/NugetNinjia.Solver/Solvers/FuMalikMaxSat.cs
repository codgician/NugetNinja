// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Z3;
using NugetNinjia.Solver.Models;

namespace NugetNinjia.Solver.Solvers
{
    public class FuMalikMaxSat : IMaxSatSolver
    {
        public int Solve(Context context, Microsoft.Z3.Solver solver, BoolExpr[] hardConstraints, BoolExpr[] softConstraints)
        {
            AssertHardConstraints(context, solver, hardConstraints);

            var isSat = solver.Check();
            if (isSat == Status.UNSATISFIABLE)
            {
                // Not possible to make formula satisfiable even when ignoring all soft constraints.
                return -1;
            }

            if (softConstraints.Length == 0)
            {
                // Nothing to be done
                return 0;
            }

            var auxVars = AssertSoftConstraints(context, solver, softConstraints);

            int iteration = 0;
            while (true)
            {
                Console.WriteLine($"Iteration: {iteration}");
                if (Step(context, solver, softConstraints, auxVars))
                {
                    return softConstraints.Length - iteration;
                }
                iteration++;
            }
        }

        private void AssertHardConstraints(Context context, Microsoft.Z3.Solver solver, BoolExpr[] constraints)
        {
            solver.Assert(constraints);
        }
           
        private BoolExpr[] AssertSoftConstraints(Context context, Microsoft.Z3.Solver solver, BoolExpr[] constraints)
        {
#nullable disable
            return Enumerable.Repeat(context.MkFreshConst("k", context.MkBoolSort()) as BoolExpr, constraints.Length)
                             .Zip(constraints)
                             .Select(t => { solver.Assert(context.MkOr(t.First, t.Second)); return t.First; })
                             .ToArray();
#nullable enable
        }

        private bool Step(Context context, Microsoft.Z3.Solver solver, BoolExpr[] softConstraints, BoolExpr[] auxVars)
        {
            var assumptions = softConstraints
                .Zip(auxVars)
                .Select(x => context.MkNot(x.Second));

            Status isSat = solver.Check(assumptions);
            if (isSat != Status.UNSATISFIABLE)
            {
                return true;
            }

            var core = solver.UnsatCore;


        }
    }
}
