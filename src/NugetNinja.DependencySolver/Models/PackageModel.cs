// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.NugetNinja.Core;
using Microsoft.Z3;

namespace NugetNinja.DependencySolver.Models;

public class PackageModel
{
    public string Name { get; private set; }

    public SortedDictionary<NugetVersion, PackageCudf> Versions { get; private set; }
    public Dictionary<NugetVersion, int> VersionsIndex { get; private set; }

    public BoolExprs inst;
    public BoolExprs ige;
    public BoolExprs ile;
    public BoolExprs uge;
    public BoolExprs ule;

    public PackageModel(Context ctx, string name, List<PackageCudf> versions)
    {
        Name = name;
        Versions = new SortedDictionary<NugetVersion, PackageCudf>(
            versions.Where(p => p.Name == name).ToDictionary(p => p.Version, p => p));
        VersionsIndex = Versions.Keys
            .Select((v, idx) => (v, idx))
            .ToDictionary(x => x.v, x => x.idx);
            
        // Initialize version control constants
        var indexEnumerator = Enumerable.Range(1, Versions.Count);
        var initBoolExpr = (string prefix, int index) => ctx.MkBoolConst($"{prefix}-{name}-{index}");
        inst = new BoolExprs(indexEnumerator.Select(idx => initBoolExpr("inst", idx)), ctx.MkFalse());
        ige = new BoolExprs(indexEnumerator.Select(idx => initBoolExpr("ige", idx)), ctx.MkFalse());
        ile = new BoolExprs(indexEnumerator.Select(idx => initBoolExpr("ile", idx)), ctx.MkFalse());
        uge = new BoolExprs(indexEnumerator.Select(idx => initBoolExpr("uge", idx)), ctx.MkFalse());
        ule = new BoolExprs(indexEnumerator.Select(idx => initBoolExpr("ule", idx)), ctx.MkFalse());
    }

    public int? GetVersionId(NugetVersion version)
    {
        VersionsIndex.TryGetValue(version, out var index);
        return index;
    }

    private BoolExpr GetVersionClause(Context ctx)
    {
        var clause = ctx.MkTrue();

        for (int i = 0; i < Versions.Count; i++)
        {
            var igeConstraint = ctx.MkOr(ctx.MkNot(ige[i]), inst[i], ige[i + 1]);
            var ugeConstraint = ctx.MkAnd(
                ctx.MkOr(ctx.MkNot(uge[i]), ctx.MkNot(inst[i])),
                ctx.MkOr(ctx.MkNot(uge[i]), uge[i + 1]));

            var ileConstraint = ctx.MkOr(ctx.MkNot(ile[i]), inst[i], ile[i - 1]);
            var uleConstraint = ctx.MkAnd(
                ctx.MkOr(ctx.MkNot(ule[i]), ctx.MkNot(inst[i])),
                ctx.MkOr(ctx.MkNot(ule[i]), ule[i - 1]));

            clause = ctx.MkAnd(clause, ctx.MkAnd(igeConstraint, ugeConstraint, ileConstraint, uleConstraint));
        }

        return clause;
    }

    private BoolExpr GetConflictClause(Context ctx, Dictionary<string, PackageModel> packages)
    {
        return Versions.Values
            .SelectMany((p, i) => p.Conflicts.Select(c => 
            {
                var conflictPackage = packages[c.PackageName];
                var conflictPackageVersionId = conflictPackage.GetVersionId(c.Version) 
                    ?? throw new InvalidDataException($"No version id found for package: {c.PackageName} and version {c.Version}");

                return c.RelOp switch
                {
                    RelationOperator.Equal => ctx.MkOr(ctx.MkNot(inst[i]), ctx.MkNot(conflictPackage.inst[conflictPackageVersionId])),
                    RelationOperator.NotEqual => ctx.MkAnd(
                        ctx.MkOr(ctx.MkNot(inst[i]), conflictPackage.ule[conflictPackageVersionId - 1]),
                        ctx.MkOr(ctx.MkNot(inst[i]), conflictPackage.uge[conflictPackageVersionId + 1])),
                    RelationOperator.GreaterOrEqual => ctx.MkOr(ctx.MkNot(inst[i]), conflictPackage.uge[conflictPackageVersionId]),
                    RelationOperator.LessOrEqual => ctx.MkOr(ctx.MkNot(inst[i]), conflictPackage.ule[conflictPackageVersionId]),
                    _ => throw new NotImplementedException()
                };
            }))
            .Aggregate(ctx.MkTrue(), (x, y) => ctx.MkAnd(x, y));
    } 

    private BoolExpr GetDependClause(Context ctx, Dictionary<string, PackageModel> packages)
    {
        return Versions.Values
            .SelectMany((p, i) => p.Conflicts.Select(c =>
            {
                var conflictPackage = packages[c.PackageName];
                var conflictPackageVersionId = conflictPackage.GetVersionId(c.Version)
                    ?? throw new InvalidDataException($"No version id found for package: {c.PackageName} and version {c.Version}");

                return c.RelOp switch
                {
                    RelationOperator.Equal => ctx.MkOr(ctx.MkNot(inst[i]), conflictPackage.inst[conflictPackageVersionId]),
                    RelationOperator.NotEqual => ctx.MkOr(ctx.MkNot(inst[i]), conflictPackage.ile[conflictPackageVersionId - 1], conflictPackage.ige[conflictPackageVersionId + 1]),
                    RelationOperator.GreaterOrEqual => ctx.MkOr(ctx.MkNot(inst[i]), conflictPackage.ige[conflictPackageVersionId]),
                    RelationOperator.LessOrEqual => ctx.MkOr(ctx.MkNot(inst[i]), conflictPackage.ile[conflictPackageVersionId]),
                    _ => throw new NotImplementedException()
                };
            }))
            .Aggregate(ctx.MkTrue(), (x, y) => ctx.MkAnd(x, y));
    }

    public List<BoolExpr> GetHardClauses(Context ctx, Dictionary<string, PackageModel> packages)
    {
        return new List<BoolExpr>()
        {
            GetVersionClause(ctx),
            GetConflictClause(ctx, packages),
            GetDependClause(ctx, packages)
        };
    }

    public class BoolExprs
    {
        private BoolExpr[] exprs;
        private BoolExpr defaultValue;

        public BoolExprs(IEnumerable<BoolExpr> exprs, BoolExpr defaultValue)
        {
            this.exprs = exprs.ToArray();
            this.defaultValue = defaultValue;
        }
        public BoolExpr this[int idx]
        {
            get => idx >= 0 && idx < exprs.Length ? exprs[idx] : defaultValue;
        }
    }
}
