// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using NugetNinja.DependencySolver.Models;

namespace NugetNinja.DependencySolver.Services;

using Microsoft.Z3;

public class UpgradeSolver : IDisposable
{
    private Context ctx;

    public Dictionary<string, PackageModel> Packages { get; set; }

    public UpgradeSolver(List<PackageCudf> packages)
    {
        // Initialize Z3 context
        ctx = new Context();

        // Create package models
        Packages = packages
            .GroupBy(p => p.Name)
            .ToDictionary(p => p.Key, p => new PackageModel(ctx, p.Key, p.ToList()));
    }

    public void Solve()
    {
        // Apply package constraints
        var hardClauses = Packages.Values
            .SelectMany(p => p.GetHardClauses(ctx, Packages))
            .Aggregate((x, y) => ctx.MkAnd(x, y));

        // Assert hard clause
        var solver = ctx.MkSolver();
        solver.Assert(hardClauses);

        // Soft clauses
        if (solver.Check() == Status.SATISFIABLE)
        {
            return;
        }
        else
        {
            throw new Exception("No feasilbe solution found");
        }
    } 

    public void Dispose()
    {
        ctx.Dispose();
    }
}

