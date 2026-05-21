// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;
using Fallout.Common.Utilities;

namespace Fallout.Common.Tooling;

[DebuggerStepThrough]
[DebuggerNonUserCode]
public static class ProcessExtensions
{
    public static IProcess AssertWaitForExit(
        this IProcess process)
    {
        Assert.True(process != null);
        Assert.True(process.WaitForExit());
        return process;
    }

    public static IProcess AssertZeroExitCode(
        this IProcess process)
    {
        process.AssertWaitForExit();

        if (process.ExitCode != 0)
            throw new ProcessException(process);

        return process;
    }

    public static IReadOnlyCollection<Output> EnsureOnlyStd(this IReadOnlyCollection<Output> output)
    {
        foreach (var o in output)
            Assert.True(o.Type == OutputType.Std);

        return output;
    }

    public static string StdToText(this IEnumerable<Output> output)
    {
        return output.Where(x => x.Type == OutputType.Std)
            .Select(x => x.Text)
            .JoinNewLine();
    }

    public static T StdToJson<T>(this IEnumerable<Output> output)
    {
        return output.StdToText().GetJson<T>();
    }

    public static JObject StdToJson(this IEnumerable<Output> output)
    {
        return output.StdToJson<JObject>();
    }
}
