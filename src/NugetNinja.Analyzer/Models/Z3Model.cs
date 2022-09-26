// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.NugetNinja.Core;
using Microsoft.Z3;

namespace NugetNinja.Analyzer.Models
{
    public class Z3ModelBuilder : IDisposable
    {
        private readonly Context _ctx;
        private readonly Optimize _opt;

        public Z3ModelBuilder()
        {
            _ctx = new Context();
            _opt = _ctx.MkOptimize();
        }

        public void AddPackage(PackageDescription package)
        {
            foreach (var constraint in package.Conflicts)
            {
                AddConflict(package.Name, package.Version, constraint);
            }

            foreach (var constraint in package.Depends)
            {
                AddDepend(package.Name, package.Version, constraint);
            }
        }

        public void AddConflict(string packageName, NugetVersion packageVersion, VersionConstraint constraint)
        {

        }

        public void AddDepend(string packageName, NugetVersion packageVersion, VersionConstraint constraint)
        {


        }

        public void Dispose()
        {
            _opt.Dispose();
            _ctx.Dispose();
        }
    }
}
