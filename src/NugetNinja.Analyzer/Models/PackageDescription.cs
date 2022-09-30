// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.NugetNinja.Core;

namespace Microsoft.NugetNinja.Analyzer.Models;

public class PackageDescription
{
    public string Name { get; private set; }
    public NugetVersion Version { get; private set; }

    public bool Installed { get; private set; }
    public VersionConstraint[] Conflicts { get; private set; }
    public VersionConstraint[] Depends { get; private set; }

    public (string, NugetVersion) NameVersion => (Name, Version);

    public PackageDescription(
        string name, 
        NugetVersion version, 
        bool installed, 
        VersionConstraint[] conflicts, 
        VersionConstraint[] depends)
    {
        Name = name;
        Version = version;
        Installed = installed;
        Conflicts = conflicts;
        Depends = depends;
    }
}
