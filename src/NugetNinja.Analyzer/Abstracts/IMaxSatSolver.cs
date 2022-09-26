// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Z3;

namespace NugetNinja.Analyzer.Models
{
    public interface IMaxSatSolver
    {
        public Model? Solve(Context context, Microsoft.Z3.Solver solver, BoolExpr[] hardConstraints, (BoolExpr, uint, string)[] softConstraints);
    }
}
