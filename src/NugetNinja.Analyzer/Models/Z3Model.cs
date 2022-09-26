// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.NugetNinja.Core;
using Microsoft.Z3;

namespace NugetNinja.Analyzer.Models
{
    public class Z3ModelBuilder : IDisposable
    {
        private readonly PackageUniverse _packages;

        private readonly Context _ctx;
        private readonly Optimize _opt;

        // Interval variables
        private readonly PackageDictionary<BoolExpr> _xs;
        private readonly PackageDictionary<BoolExpr> _uups;
        private readonly PackageDictionary<BoolExpr> _udowns;
        private readonly PackageDictionary<BoolExpr> _iups;
        private readonly PackageDictionary<BoolExpr> _idowns;

        // For not-up-to-date rules
        private readonly Dictionary<string, BoolExpr> _ts;

        public Z3ModelBuilder(PackageUniverse packages)
        {
            _packages = packages;
            _ctx = new Context();
            _opt = _ctx.MkOptimize();

            _xs = new PackageDictionary<BoolExpr>(
                packages.Select(p => (p.Name, p.Version, _ctx.MkBoolConst($"x__{p.Name}__{p.Version}"))));
            _uups = new PackageDictionary<BoolExpr>(
                packages.Select(p => (p.Name, p.Version, _ctx.MkBoolConst($"uup__{p.Name}__{p.Version}"))));
            _udowns = new PackageDictionary<BoolExpr>(
                packages.Select(p => (p.Name, p.Version, _ctx.MkBoolConst($"udown__{p.Name}__{p.Version}"))));
            _iups = new PackageDictionary<BoolExpr>(
                packages.Select(p => (p.Name, p.Version, _ctx.MkBoolConst($"iup__{p.Name}__{p.Version}"))));
            _idowns = new PackageDictionary<BoolExpr>(
                packages.Select(p => (p.Name, p.Version, _ctx.MkBoolConst($"idown__{p.Name}__{p.Version}"))));

            _ts = packages.Names.ToDictionary(x => x, x => _ctx.MkBoolConst($"ts__{x}"));
        }

        public void AddPackage(PackageDescription package)
        {
            AddBaseRules(package.Name, package.Version);
            AddNotUpToDateRules(package.Name, package.Version, 1);

            foreach (var constraint in package.Conflicts)
            {
                AddConflictRules(package.Name, package.Version, constraint);
            }

            foreach (var constraint in package.Depends)
            {
                AddDependRules(package.Name, package.Version, constraint);
            }
        }

        private void AddConflictRules(string packageName, NugetVersion packageVersion, VersionConstraint constraint)
        {
            switch (constraint.RelOp)
            {
                case RelationOperator.Equal:
                    _opt.Assert(_ctx.MkOr(_ctx.MkNot(_xs[(packageName, packageVersion)]), _xs[(constraint.PackageName, constraint.Version)]));
                    break;
                case RelationOperator.NotEqual:
                    _opt.Assert(_ctx.MkAnd(
                        _ctx.MkOr(_ctx.MkNot(_xs[(packageName, packageVersion)]), _udowns[(constraint.PackageName, constraint.Version)]),
                        _ctx.MkOr(_ctx.MkNot(_xs[(packageName, packageVersion)]), _uups[(constraint.PackageName, constraint.Version)])));
                    break;
                case RelationOperator.LessOrEqual:
                    _opt.Assert(_ctx.MkOr(_ctx.MkNot(_xs[(packageName, packageVersion)]), _udowns[(constraint.PackageName, constraint.Version)]));
                    break;
                case RelationOperator.GreaterOrEqual:
                    _opt.Assert(_ctx.MkOr(_ctx.MkNot(_xs[(packageName, packageVersion)]), _uups[(constraint.PackageName, constraint.Version)]));
                    break;
            }
        }

        private void AddDependRules(string packageName, NugetVersion packageVersion, VersionConstraint constraint)
        {
            switch (constraint.RelOp)
            {
                case RelationOperator.Equal:
                    _opt.Assert(_ctx.MkOr(_ctx.MkNot(_xs[(packageName, packageVersion)])), _xs[(constraint.PackageName, constraint.Version)]);
                    break;
                case RelationOperator.NotEqual:
                    _opt.Assert(_ctx.MkOr(
                        _ctx.MkNot(_xs[(packageName, packageVersion)])),
                        _idowns[(constraint.PackageName, constraint.Version)],
                        _iups[(constraint.PackageName, constraint.Version)]);
                    break;
                case RelationOperator.LessOrEqual:
                    _opt.Assert(_ctx.MkOr(_ctx.MkNot(_xs[(packageName, packageVersion)])), _idowns[(constraint.PackageName, constraint.Version)]);
                    break;
                case RelationOperator.GreaterOrEqual:
                    _opt.Assert(_ctx.MkOr(_ctx.MkNot(_xs[(packageName, packageVersion)])), _iups[(constraint.PackageName, constraint.Version)]);
                    break;
            }
        }

        private void AddBaseRules(string packageName, NugetVersion packageVersion)
        {
            _opt.Assert(_ctx.MkOr(_ctx.MkNot(_uups[(packageName, packageVersion)]), _ctx.MkNot(_xs[(packageName, packageVersion)])));
            _opt.Assert(_ctx.MkOr(_ctx.MkNot(_uups[(packageName, packageVersion)]), _uups[(packageName, _packages.GetNextPackageVersion(packageName, packageVersion))]));

            _opt.Assert(_ctx.MkOr(_ctx.MkNot(_udowns[(packageName, packageVersion)]), _ctx.MkNot(_xs[(packageName, packageVersion)])));
            _opt.Assert(_ctx.MkOr(_ctx.MkNot(_udowns[(packageName, packageVersion)]), _udowns[(packageName, _packages.GetPrevPackageVersion(packageName, packageVersion))]));

            _opt.Assert(_ctx.MkOr(
                _ctx.MkNot(_iups[(packageName, packageVersion)]),
                _xs[(packageName, packageVersion)],
                _iups[(packageName, _packages.GetNextPackageVersion(packageName, packageVersion))]));
            _opt.Assert(_ctx.MkOr(
                 _ctx.MkNot(_idowns[(packageName, packageVersion)]),
                 _xs[(packageName, packageVersion)],
                 _idowns[(packageName, _packages.GetPrevPackageVersion(packageName, packageVersion))]));
        }

        private void AddNotUpToDateRules(string packageName, NugetVersion packageVersion, uint weight = 1)
        {
            _opt.Assert(_ctx.MkOr(_ctx.MkNot(_xs[(packageName, packageVersion)]), _ts[packageName]));
            _opt.AssertSoft(_ctx.MkOr(_ctx.MkNot(_ts[packageName]), _xs[(packageName, _packages.GetPackageMaxVerison(packageName)!)]), weight, "notuptodate");
        }

        public void Dispose()
        {
            _opt.Dispose();
            _ctx.Dispose();
        }
    }
}
