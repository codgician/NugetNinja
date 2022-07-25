﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.NugetNinja;

public interface IAction
{
    public string BuildMessage();

    public void TakeAction();
}
