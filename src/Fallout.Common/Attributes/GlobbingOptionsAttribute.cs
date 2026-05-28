using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Common.Execution;

namespace Fallout.Common.IO;

/// <summary>
/// Allows to configure the case-sensitivity used for globbing operations in <see cref="PathConstruction"/>.
/// </summary>
public class GlobbingOptionsAttribute : BuildExtensionAttributeBase, IOnBuildCreated
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
