// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.NugetNinja.Core;

namespace NugetNinja.Solver.Models;

public class PackageUniverse
{
    private readonly Dictionary<string, SortedDictionary<NugetVersion, PackageDescription>> _dict;

    public PackageUniverse()
    {
        _dict = new Dictionary<string, SortedDictionary<NugetVersion, PackageDescription>>();
    }

    public PackageUniverse(IEnumerable<PackageDescription> packages)
    {
        _dict = packages
            .GroupBy(p => p.Name, p => p)
            .ToDictionary(
                g => g.Key,
                g => new SortedDictionary<NugetVersion, PackageDescription>(
                    g.ToDictionary(p => p.Version, p => p)));
    }

    public PackageDescription? GetPackageDescription(string name, NugetVersion version)
    {
        try
        {
            return _dict[name][version];
        }
        catch
        {
            return null;
        }
    }

    public List<NugetVersion>? GetPackageVersions(string name)
    {
        _dict.TryGetValue(name, out var descriptions);
        return descriptions?.Select(x => x.Value.Version).ToList();
    }

    public NugetVersion? GetPackageMaxVersion(string name)
    {
        _dict.TryGetValue(name, out var descriptions);
        return descriptions?.Keys.Last();
    }
}
