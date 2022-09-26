// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Z3;

namespace NugetNinjia.Solver.Models
{
    public interface IMaxSatSolver
    {
        public int Solve(Context context, Microsoft.Z3.Solver solver, BoolExpr[] hardConstraints, BoolExpr[] softConstraints);
    }
}
