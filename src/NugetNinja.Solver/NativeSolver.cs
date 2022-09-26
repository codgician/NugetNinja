// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Z3;
using NugetNinja.Solver.Models;

namespace NugetNinja.Solver
{
    public class NativeSolver : IMaxSatSolver
    {
        public Model? Solve(Context context, Microsoft.Z3.Solver solver, BoolExpr[] hardConstraints, (BoolExpr, uint, string)[] softConstraints)
        {
            var optimize = context.MkOptimize();

            optimize.Assert(hardConstraints);

            foreach (var (constraint, weight, group) in softConstraints)
            {
                optimize.AssertSoft(constraint, weight, group);
            }

            if (optimize.Check() == Status.SATISFIABLE)
            {
                return optimize.Model;
            }

            return null;
        }
    }
}
