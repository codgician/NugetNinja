// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.NugetNinja.Core;

namespace NugetNinja.Solver.Models;

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
}
