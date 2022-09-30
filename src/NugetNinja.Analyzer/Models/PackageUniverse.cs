// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.NugetNinja.Core;

namespace Microsoft.NugetNinja.Analyzer.Models;

public class PackageUniverse : PackageDictionary<PackageDescription>
{
    public PackageUniverse()
    {
    }

    public PackageUniverse(IEnumerable<PackageDescription> packages) 
        : base(packages.Select(p => (p.Name, p.Version, p)))
    {
    }

    public PackageDescription[] GetAllPackageDescriptions() => AllValues.ToArray();

    public PackageDescription? GetPackageDescription(string name, NugetVersion version) => Get((name, version));
}
