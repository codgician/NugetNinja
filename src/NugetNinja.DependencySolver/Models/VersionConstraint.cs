// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace NugetNinja.DependencySolver.Models;

using Microsoft.NugetNinja.Core;

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
