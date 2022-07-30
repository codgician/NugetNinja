// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.NugetNinja.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NugetNinja.DependencySolver.Models;
using NugetNinja.DependencySolver.Services;

namespace NugetNinja.DependencySolver.Tests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void SimpleTest()
    {
        var pkg1 = new PackageCudf(
            "Package1",
            new NugetVersion("1.0.0"),
            installed: true,
            conflicts: new List<VersionConstraint>(),
            depends: new List<VersionConstraint>());

        var pkg2 = new PackageCudf(
            "Package1",
            new NugetVersion("1.0.1"),
            installed: false,
            conflicts: new List<VersionConstraint>(),
            depends: new List<VersionConstraint>());

        var solver = new UpgradeSolver(new List<PackageCudf>() { pkg1, pkg2 });
        solver.Solve();
    }
}
