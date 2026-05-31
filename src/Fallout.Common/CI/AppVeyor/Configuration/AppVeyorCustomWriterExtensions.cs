using System;
using System.Linq;
using Fallout.Application.Utilities;
using Fallout.Kernel;

namespace Fallout.Common.CI.AppVeyor.Configuration;

public static class AppVeyorCustomWriterExtensions
{
    public static IDisposable WriteBlock(this CustomFileWriter writer, string text)
    {
        return DelegateDisposable
            .CreateBracket(() => writer.WriteLine(text))
            .CombineWith(writer.Indent());
    }
}
