// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.NugetNinja.Core;
using Microsoft.Z3;

namespace Microsoft.NugetNinja.Analyzer.Models;

public class UpgradeModel : IDisposable
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

    public UpgradeModel(PackageUniverse packages)
    {
        _packages = packages;
        _ctx = new Context();
        _opt = _ctx.MkOptimize();

        // Declare boolean constants
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

        // Add rules for each package
        foreach (var package in packages)
        {
            AddRulesForPackage(package);
        }
    }

    public Microsoft.Z3.Model? Solve(IMaxSatSolver solver)
    {
        if (_opt.Check() == Status.SATISFIABLE)
        {
            return _opt.Model;
        }

        return null;
    }

    private void AddRulesForPackage(PackageDescription package)
    {
        AddBaseRules(package.Name, package.Version);
        // AddNotUpToDateRules(package.Name, package.Version, 1);

        // Conflict with all other versions of itself
        // AddConflictRules(
        //    package.Name, package.Version, 
        //    new VersionConstraint(package.Name, RelationOperator.NotEqual, package.Version));

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
        Console.WriteLine($"AddConflictRule: {(packageName, packageVersion)} conflicts with {constraint}");

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
        Console.WriteLine($"AddDependRule: {(packageName, packageVersion)} depends on {constraint}");

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

    private void AddBaseRules(string packageName, NugetVersion version)
    { 
        var prevVersion = _packages.GetPrevPackageVersion(packageName, version);
        var nextVersion = _packages.GetNextPackageVersion(packageName, version);
        Console.WriteLine($"AddBaseRule: package {packageName}: {version}, prev = {prevVersion}, next = {nextVersion}");

        // u_up(p,v) -> !x(p, v): either !(all versions >= v of p are uninstalled), or !(v of p is installed)
        _opt.Assert(_ctx.MkOr(_ctx.MkNot(_uups[(packageName, version)]), _ctx.MkNot(_xs[(packageName, version)])));
        Console.WriteLine($"BaseRule: u_up({packageName}, {version}) -> !x({packageName}, {version})");

        // u_down(p, v) -> !x(p, v): either !(all versions <= v of p are uninstalled), or !(v of p is installed)
        _opt.Assert(_ctx.MkOr(_ctx.MkNot(_udowns[(packageName, version)]), _ctx.MkNot(_xs[(packageName, version)])));
        Console.WriteLine($"BaseRule: u_down({packageName}, {version}) -> !x({packageName}, {version})");

        if (prevVersion == null)
        {
            // i_down(p, v) -> x(p, v)
            // either !(exists <= v of p installed) `or` (v of p is installed)
            _opt.Assert(_ctx.MkOr(
                _ctx.MkNot(_idowns[(packageName, version)]),
                 _xs[(packageName, version)]));
            Console.WriteLine($"BaseRule: i_down({packageName}, {version}) -> x({packageName}, {version})");
        }
        else
        {
            // u_downs(p, v) -> u_downs(p, v - 1): either !(<= v of p are uninstalled), or <= (v - 1) of p are uninstalled)
            _opt.Assert(_ctx.MkOr(_ctx.MkNot(_udowns[(packageName, version)]), _udowns[(packageName, prevVersion)]));
            Console.WriteLine($"BaseRule: u_down({packageName}, {version}) -> u_down({packageName}, {prevVersion})");

            // i_down(p, v) -> x(p, v) `or` i_down(p, v - 1):
            // either !(exists <= v of p installed) `or` (v of p is installed) `or` (exists <= (v - 1) of p installed) 
            _opt.Assert(_ctx.MkOr(
                _ctx.MkNot(_idowns[(packageName, version)]),
                 _xs[(packageName, version)],
                 _idowns[(packageName, prevVersion)]));
            Console.WriteLine($"BaseRule: i_down({packageName}, {version}) -> x({packageName}, {version}) or i_down({packageName}, {prevVersion})");
        }

        if (nextVersion == null)
        {
            // i_up(p, v) -> x(p, v)
            // either !(exists >= v of p installed) `or` (v of p is installed)
            _opt.Assert(_ctx.MkOr(
                _ctx.MkNot(_iups[(packageName, version)]),
                _xs[(packageName, version)]));
            Console.WriteLine($"BaseRule: i_up({packageName}, {version}) -> x({packageName}, {version})");
        }
        else
        {
            // u_up(p, v) -> u_up(p, v + 1): either !(>= v of p are uninstalled), or <= (v + 1) of p are uninstalled)
            _opt.Assert(_ctx.MkOr(_ctx.MkNot(_uups[(packageName, version)]), _uups[(packageName, nextVersion)]));
            Console.WriteLine($"BaseRule: u_up[({packageName}, {version}) -> u_up({packageName}, {nextVersion})");

            // i_up(p, v) -> x(p, v) `or` up(p, v + 1):
            // either !(exists >= v of p installed) `or` (v of p is installed) `or` (exists >= (v + 1) of p installed) 
            _opt.Assert(_ctx.MkOr(
                _ctx.MkNot(_iups[(packageName, version)]),
                _xs[(packageName, version)],
                _iups[(packageName, nextVersion)]));
            Console.WriteLine($"BaseRule: i_up({packageName}, {version}) -> x({packageName}, {version}) or i_up({packageName}, {nextVersion})");
        }
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
