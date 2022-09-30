// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using NugetNinja.Analyzer.Solvers;

namespace NugetNinja.Analyzer.Tests
{
    [TestClass]
    public class UpgradeModelTests
    {
        /* A simple test for the upgrability model:
         *  - Package A:
         *      - version 1: installed, depend (B, >=, 2)
         *      - version 2: depend (B, >=, 3)
         *  - Package B:
         *      - version 2: installed
         *      - version 3
         *      
         *  Expected solution: (A, 2), (B, 3)
         **/
        [TestMethod]
        public void SimpleTest()
        {
            var packageDescriptions = new[]
            {
                new PackageDescription(
                    "A",
                    new NugetVersion("1.0.0.0"),
                    true,
                    Array.Empty<VersionConstraint>(),
                    new [] { new VersionConstraint("B", RelationOperator.GreaterOrEqual, new NugetVersion("2.0.0.0")) }
                ),
                new PackageDescription(
                    "A",
                    new NugetVersion("2.0.0.0"),
                    false,
                    Array.Empty<VersionConstraint>(),
                    new [] { new VersionConstraint("B", RelationOperator.GreaterOrEqual, new NugetVersion("3.0.0.0")) }
                ),
                new PackageDescription(
                    "B",
                    new NugetVersion("2.0.0.0"),
                    true,
                    Array.Empty<VersionConstraint>(),
                    Array.Empty<VersionConstraint>()
                ),
                new PackageDescription(
                    "B",
                    new NugetVersion("3.0.0.0"),
                    false,
                    Array.Empty<VersionConstraint>(),
                    Array.Empty<VersionConstraint>()
                )
            };

            var packages = new PackageUniverse(packageDescriptions);
            var model = new UpgradeModel(packages);
            var result = model.Solve(new BuiltinSolver());
            Assert.IsNotNull(result);

            Console.WriteLine(result);
        }
    }
}
