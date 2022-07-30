// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.NugetNinja.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NugetNinja.DependencySolver.Models;
using NugetNinja.DependencySolver.Services;

namespace NugetNinja.DependencySolver.Tests;

[TestClass]
public class SimpleTests
{
    [TestMethod]
    public void SimpleTest()
    {
        var a1 = new PackageCudf(
            "Package1",
            new NugetVersion("1.0.0"),
            installed: true,
            conflicts: new List<VersionConstraint>(),
            depends: new List<VersionConstraint>()
            {
                new VersionConstraint("Package2", RelationOperator.GreaterOrEqual, new NugetVersion("1.0.1")),
            });

        var a2 = new PackageCudf(
            "Package1",
            new NugetVersion("1.0.1"),
            installed: false,
            conflicts: new List<VersionConstraint>(),
            depends: new List<VersionConstraint>()
            {
                new VersionConstraint("Package2", RelationOperator.GreaterOrEqual, new NugetVersion("1.0.0")),
            });

        var b1 = new PackageCudf(
            "Package2",
            new NugetVersion("1.0.0"),
            installed: false,
            conflicts: new List<VersionConstraint>(),
            depends: new List<VersionConstraint>());

        var b2 = new PackageCudf(
            "Package2",
            new NugetVersion("1.0.1"),
            installed: false,
            conflicts: new List<VersionConstraint>(),
            depends: new List<VersionConstraint>());

        var solver = new MaxSatSolver(new List<PackageCudf>() { a1, a2, b1 });
        solver.Solve();
    }
}
