using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Fallout.Application.Execution;
using Fallout.Application.Solutions;
using Fallout.Application.Tooling;
using Fallout.Application.Tools.GitVersion;
using Fallout.Kernel.Collections;
using Fallout.Application;
using Fallout.Application.Tools.DotNet;
using Fallout.Application.Tools.MSBuild;
using Fallout.Application.Tools.SignTool;
using Fallout.Application.Tools.NuGet;
using Fallout.Kernel.IO;
using Fallout.Kernel;
using static Fallout.Application.ControlFlow;
using static Fallout.Application.Tools.DotNet.DotNetTasks;
using static Fallout.Application.Tools.MSBuild.MSBuildTasks;
using static Fallout.Application.Tools.SignTool.SignToolTasks;
using static Fallout.Application.Tools.NuGet.NuGetTasks;
using static Fallout.Kernel.IO.TextTasks;
using static Fallout.Kernel.IO.XmlTasks;
using static Fallout.Kernel.EnvironmentInfo;

class Build : FalloutBuild
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .Executes(() =>
    {
        System.Console.WriteLine();
    });
}