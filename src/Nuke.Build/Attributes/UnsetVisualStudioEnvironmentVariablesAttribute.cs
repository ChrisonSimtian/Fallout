// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common.Utilities.Collections;

namespace Nuke.Common.Execution;

[PublicAPI]
public class UnsetVisualStudioEnvironmentVariablesAttribute : BuildExtensionAttributeBase, IOnBuildCreated
{
    public void OnBuildCreated(IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        new[]
        {
            "MSBuildLoadMicrosoftTargetsReadOnly",
            "VisualStudioDir",
            "VisualStudioEdition",
            "VisualStudioVersion",
            "VSAPPIDDIR",
            "VSAPPIDNAME",
            "VSLANG",
            "VSLOGGER_UNIQUEID",
            "VSSKUEDITION"
        }.ForEach(x => Environment.SetEnvironmentVariable(x, value: null));
    }
}
