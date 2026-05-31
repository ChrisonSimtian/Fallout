using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Fallout.Common;
using Fallout.Common.Execution;
using Fallout.Common.IO;
using Fallout.Solutions;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Tools.GitVersion;
using Fallout.Common.Tools.SignTool;
using Fallout.Common.Utilities.Collections;
using Fallout.Application;
using Fallout.Application.Tools.DotNet;
using Fallout.Application.Tools.MSBuild;
using Fallout.Application.Tools.SignTool;
using Fallout.Application.Tools.NuGet;
using Fallout.Common.IO;
using Fallout.Common.IO;
using Fallout.Common;
using static Fallout.Application.ControlFlow;
using static Fallout.Application.Tools.DotNet.DotNetTasks;
using static Fallout.Application.Tools.MSBuild.MSBuildTasks;
using static Fallout.Application.Tools.SignTool.SignToolTasks;
using static Fallout.Application.Tools.NuGet.NuGetTasks;
using static Fallout.Common.IO.TextTasks;
using static Fallout.Common.IO.XmlTasks;
using static Fallout.Common.EnvironmentInfo;

class Build : FalloutBuild
{
    //////////////////////////////////////////////////////////////////////
    // ARGUMENTS
    //////////////////////////////////////////////////////////////////////
    [Parameter] readonly string Target = "Default";
    [Parameter] readonly string Configuration = "Release";
}