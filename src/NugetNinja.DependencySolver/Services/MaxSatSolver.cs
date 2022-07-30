// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Z3;
using NugetNinja.DependencySolver.Models;

namespace NugetNinja.DependencySolver.Services;

public class MaxSatSolver : IDisposable
{
    private readonly Context ctx;

    public Dictionary<string, PackageModel> Packages { get; set; }

    public MaxSatSolver(List<PackageCudf> packages)
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
            var model = solver.Model;

            // Parse model

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

