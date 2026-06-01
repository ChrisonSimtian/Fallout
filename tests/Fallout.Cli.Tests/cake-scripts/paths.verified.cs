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
using Fallout.Common;
using static Fallout.Application.ControlFlow;
using static Fallout.Application.Tools.DotNet.DotNetTasks;
using static Fallout.Application.Tools.MSBuild.MSBuildTasks;
using static Fallout.Application.Tools.SignTool.SignToolTasks;
using static Fallout.Application.Tools.NuGet.NuGetTasks;
using static Fallout.Kernel.IO.TextTasks;
using static Fallout.Kernel.IO.XmlTasks;
using static Fallout.Common.EnvironmentInfo;

class Build : FalloutBuild
{
    AbsolutePath LocalPackagesDir => RootDirectory / ".." / "LocalPackages";

    AbsolutePath SourceFolder => RootDirectory / "source";

    AbsolutePath PublishDir => RootDirectory / "publish";

    AbsolutePath SignToolPath => RootDirectory / "certificates" / "signtool.exe";

    private string Convert(AbsolutePath file)
    {
        file = (AbsolutePath)file;
        CopyFile(RootDirectory / projectFile/ $"{projectFile}.nuspec", "nuspec");
    }

    private void NoConvert()
    {
        var nodes = doc.SelectNodes("Project/PropertyGroup/RuntimeIdentifiers");
        var node = doc.SelectSingleNode("Project/PropertyGroup/RuntimeIdentifiers");
    }
}