// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace NugetNinja.Analyzer.Models;

public class UserRequest
{
    public string[] Install { get; set; } = Array.Empty<string>();

    public string[] Remove { get; set; } = Array.Empty<string>();
}
