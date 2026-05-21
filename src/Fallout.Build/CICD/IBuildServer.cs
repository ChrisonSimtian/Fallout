// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE


namespace Fallout.Common.CI;

public interface IBuildServer
{
    string Branch { get; }

    string Commit { get; }
}
