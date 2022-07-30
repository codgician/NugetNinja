// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace NugetNinja.DependencySolver.Models;

using Microsoft.Z3;

public class MaxSatModel : IDisposable
{
    private Context ctx;

    public Dictionary<string, PackageModel> Packages { get; set; }

    public MaxSatModel(List<PackageCudf> packages)
    {
        // Initialize Z3 context
        ctx = new Context();

        // Create package models
        Packages = packages
            .GroupBy(p => p.Name)
            .ToDictionary(p => p.Key, p => new PackageModel(ctx, p.Key, p.ToList()));

        // Apply package constraints
        var hardClauses = Packages.Values
            .SelectMany(p => p.GetHardClauses(ctx, Packages))
            .Aggregate((x, y) => ctx.MkAnd(x, y));

        // Assert hard clause
        var solver = ctx.MkSolver();
        solver.Assert(hardClauses);

        // Soft clauses
        // to be implemented...
    }
    public void Dispose()
    {
        ctx.Dispose();
    }
}

