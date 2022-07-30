namespace NugetNinja.DependencySolver.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NugetNinja.DependencySolver.Services;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        var solver = new TestExample();
        solver.test();
    }
}
