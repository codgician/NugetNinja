// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Z3;
using NugetNinjia.Solver.Models;

namespace NugetNinjia.Solver.Solvers
{
    public class Native : IMaxSatSolver
    {
        public int Solve(Context context, Microsoft.Z3.Solver solver, BoolExpr[] hardConstraints, BoolExpr[] softConstraints)
        {
            var optimize = context.MkOptimize();
            
            foreach (var constraint in hardConstraints)
            {
                optimize.Assert(constraint);
            }

            foreach (var constraint in softConstraints)
            {
                optimize.AssertSoft(constraint, 1, "soft");
            }

            if (optimize.Check() == Status.SATISFIABLE)
            {
                Console.WriteLine(optimize.Model);
                return 1;
            }

            return -1;
        }
    }
}
