using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Fallout.Application;
using Fallout.Core;

namespace Fallout.Application.Tests;

public static class HostInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        FalloutBuild.Host = new SilentHost();
    }

    private class SilentHost : Host
    {
        protected internal override IDisposable WriteBlock(string text)
        {
            return DelegateDisposable.CreateBracket();
        }
    }
}
