// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Nuke.Common.Utilities;

namespace Nuke.Common.Tests;

public static class HostInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        NukeBuild.Host = new SilentHost();
    }

    private class SilentHost : Host
    {
        protected internal override IDisposable WriteBlock(string text)
        {
            return DelegateDisposable.CreateBracket();
        }
    }
}
