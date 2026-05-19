// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;

namespace Nuke.Common.Tools.Unity.Logging;

internal class MatchedBlock
{
    public BlockMatcher BlockMatcher { get; }
    public string Name { get; }
    public MatchType MatchType { get; }

    public MatchedBlock(BlockMatcher blockMatcher, string name, MatchType matchType)
    {
        BlockMatcher = blockMatcher;
        Name = name;
        MatchType = matchType;
    }

    public MatchType MatchesEnd(string message)
    {
        return BlockMatcher.MatchesEnd(message);
    }
}
