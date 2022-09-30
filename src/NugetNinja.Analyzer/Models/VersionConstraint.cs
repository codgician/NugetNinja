// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Microsoft.NugetNinja.Core;

namespace Microsoft.NugetNinja.Analyzer.Models;

public class VersionConstraint
{
    public string PackageName { get; private set; }

    public RelationOperator RelOp { get; private set; }

    public NugetVersion Version { get; private set; }

    public VersionConstraint(string packageName, RelationOperator relOp, NugetVersion version)
    {
        PackageName = packageName;
        RelOp = relOp;
        Version = version;
    }

    public override string ToString()
    {
        var relOpStr = RelOp switch
        {
            RelationOperator.Equal => "==",
            RelationOperator.NotEqual => "!=",
            RelationOperator.LessOrEqual => "<=",
            RelationOperator.GreaterOrEqual => ">=",
            _ => throw new NotImplementedException("Unrecognized relation operator")
        };

        return $"{(PackageName, relOpStr, Version)}";
    }
}
