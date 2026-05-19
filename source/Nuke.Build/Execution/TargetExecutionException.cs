// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Nuke.Common.Execution;

[Serializable]
internal class TargetExecutionException : Exception
{
    public TargetExecutionException(string targetName, Exception inner)
        : base($"Target '{targetName}' has thrown an exception.", inner)
    {
    }

    protected TargetExecutionException(
        SerializationInfo info,
        StreamingContext context)
        : base(info, context)
    {
    }
}
