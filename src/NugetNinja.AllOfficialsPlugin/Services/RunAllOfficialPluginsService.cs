﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.NugetNinja.Core;
using Microsoft.NugetNinja.DeprecatedPackagePlugin;
using Microsoft.NugetNinja.PossiblePackageUpgradePlugin;
using Microsoft.NugetNinja.UselessPackageReferencePlugin;
using Microsoft.NugetNinja.UselessProjectReferencePlugin;

namespace Microsoft.NugetNinja.AllOfficialsPlugin;

public class RunAllOfficialPluginsService : IEntryService
{
    private readonly ILogger<RunAllOfficialPluginsService> _logger;
    private readonly Extractor _extractor;
    private readonly List<IActionDetector> _pluginDetectors;

    public RunAllOfficialPluginsService(
        ILogger<RunAllOfficialPluginsService> logger,
        Extractor extractor,
        DeprecatedPackageDetector deprecatedPackageDetector,
        PackageReferenceUpgradeDetector packageReferenceUpgradeDetector,
        UselessPackageReferenceDetector uselessPackageReferenceDetector,
        UselessProjectReferenceDetector uselessProjectReferenceDetector)
    {
        _logger = logger;
        _extractor = extractor;
        _pluginDetectors = new List<IActionDetector>
        {
            uselessPackageReferenceDetector,
            uselessProjectReferenceDetector,
            packageReferenceUpgradeDetector,
            deprecatedPackageDetector
        };
    }

    public async Task OnServiceStartedAsync(string path, bool shouldTakeAction)
    {
        foreach (var plugin in _pluginDetectors)
        {
            _logger.LogInformation($"Parsing files to build project structure based on path: '{path}'...");
            var model = await _extractor.Parse(path);

            _logger.LogInformation($"Analysing possible actions via {plugin.GetType().Name}...");
            var actions = plugin.AnalyzeAsync(model);

            await foreach (var action in actions)
            {
                _logger.LogWarning(action.BuildMessage());
                if (shouldTakeAction)
                {
                    await action.TakeActionAsync();
                }
            }
        }
    }
}
