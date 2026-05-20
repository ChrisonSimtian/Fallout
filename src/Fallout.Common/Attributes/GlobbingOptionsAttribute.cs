// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Fallout.Common.Execution;

namespace Fallout.Common.IO;

/// <summary>
/// Allows to configure the case-sensitivity used for globbing operations in <see cref="PathConstruction"/>.
/// </summary>
[PublicAPI]
public sealed class GlobbingOptionsAttribute : BuildExtensionAttributeBase, IOnBuildCreated
{
    private readonly GlobbingCaseSensitivity _caseSensitivity;

    public GlobbingOptionsAttribute(GlobbingCaseSensitivity caseSensitivity)
    {
        _caseSensitivity = caseSensitivity;
    }

    public void OnBuildCreated(IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        Globbing.GlobbingCaseSensitivity = _caseSensitivity;
    }
}
