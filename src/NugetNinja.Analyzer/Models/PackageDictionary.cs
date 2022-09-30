// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using Microsoft.NugetNinja.Core;

namespace Microsoft.NugetNinja.Analyzer.Models;

public class PackageDictionary<T> : IEnumerable<T>
{
    protected readonly Dictionary<string, SortedDictionary<NugetVersion, T>> _dict;

    public IEnumerable<string> Names => _dict.Keys;

    public IEnumerable<T> AllValues => _dict.Values.SelectMany(d => d.Values);

    public T? this[(string, NugetVersion) index]
    {
        get => Get(index);
        set => Set(index, value);
    }

    public PackageDictionary()
    {
        _dict = new Dictionary<string, SortedDictionary<NugetVersion, T>>();
    }

    public PackageDictionary(Dictionary<string, SortedDictionary<NugetVersion, T>> dict)
    {
        _dict = dict;
    }

    public PackageDictionary(IEnumerable<(string, NugetVersion, T)> values)
    {
        _dict = values
            .GroupBy(p => p.Item1, p => p)
            .ToDictionary(
                g => g.Key,
                g => new SortedDictionary<NugetVersion, T>(
                    g.ToDictionary(p => p.Item2, p => p.Item3)));
    }

    public PackageDictionary(IEnumerable<(string, NugetVersion)> packages, T initValue)
    {
        _dict = packages
            .GroupBy(p => p.Item1, p => p)
            .ToDictionary(
                g => g.Key,
                g => new SortedDictionary<NugetVersion, T>(
                    g.ToDictionary(p => p.Item2, _ => initValue)));
    }

    public T? Get((string, NugetVersion) key)
    {
        T? result = default;
        var (name, version) = key;
        _dict.TryGetValue(name, out var dict1);
        dict1?.TryGetValue(version, out result);
        return result;
    }

    public void Set((string, NugetVersion) key, T? value)
    {
        var (name, version) = key;

        if (!_dict.ContainsKey(name))
        {
            _dict[name] = new SortedDictionary<NugetVersion, T>();
        }

        _dict[name][version] = value;
    }

    public NugetVersion[] GetPackageVersions(string name)
    {
        _dict.TryGetValue(name, out var dict1);
        return dict1?.Keys.ToArray() ?? Array.Empty<NugetVersion>();
    }

    // todo: improve efficiency
    public NugetVersion? GetPrevPackageVersion(string name, NugetVersion version)
    {
        var versions = GetPackageVersions(name);
        var index = 1;
        NugetVersion? result = null;
        for (; index < versions.Length; index++)
        {
            if (versions[index] == version)
            {
                result = versions[index - 1];
            }
        }

        return result;
    }

    // todo: improve efficiency
    public NugetVersion? GetNextPackageVersion(string name, NugetVersion version)
    {
        var versions = GetPackageVersions(name);
        var index = 0;
        NugetVersion? result = null;
        for (; index < versions.Length - 1; index++)
        {
            if (versions[index] == version)
            {
                result = versions[index + 1];
            }
        }

        return result;
    }

    public NugetVersion? GetPackageMaxVerison(string name)
    {
        _dict.TryGetValue(name, out var dict1);
        return dict1?.Keys.Last();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => AllValues.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => AllValues.GetEnumerator();
}
