// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using Nuke.Common.Utilities;

namespace Nuke.Common.CI.TravisCI;

public partial class TravisCI
{
    protected internal override IDisposable WriteBlock(string text)
    {
        return DelegateDisposable.CreateBracket(
            () => Console.WriteLine($"travis_fold:start:{text}"),
            () => Console.WriteLine($"travis_fold:end:{text}"));
    }
}
