// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Z3;

namespace Microsoft.NugetNinja.Analyzer.Models
{
    public interface IMaxSatSolver
    {
        public Model? Solve(Context context, BoolExpr[] hardConstraints, (BoolExpr, uint, string)[] softConstraints);
    }
}
