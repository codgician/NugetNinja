// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace NugetNinja.DependencySolver.Models;

using Microsoft.NugetNinja.Core;

public class PackageCudf
{
    public string Name { get; private set; }   
    public NugetVersion Version { get; private set; }

    public bool Installed { get; private set; }
    public List<VersionConstraint> Conflicts { get; private set; }
    public List<VersionConstraint> Depends { get; private set; }

    public PackageCudf(string name, NugetVersion version, bool installed, List<VersionConstraint> conflicts, List<VersionConstraint> depends)
    {
        Name = name;
        Version = version;
        Installed = installed;
        Conflicts = conflicts;
        Depends = depends;
    }
}
