// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using JetBrains.Annotations;

namespace Fallout.Common.CI;

public interface IBuildServer
{
    [CanBeNull]
    string Branch { get; }

    [CanBeNull]
    string Commit { get; }
}
