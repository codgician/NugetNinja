// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using NugetNinja.Solver.Solvers;

namespace NugetNinja.Solver.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var solver = new NativeSolver();
            solver.Play();
        }
    }
}
