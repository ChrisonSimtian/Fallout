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
using Fallout.Application.Tools.DotNet;
using Fallout.Application.Tools.Versioning.GitVersion;
using Fallout.Application.Tools.Signing.SignTool;
using Fallout.Common.Utilities.Collections;
using Fallout.Common;
using Fallout.Application.Tools.DotNet;
using Fallout.Application.Tools.DotNet.MSBuild;
using Fallout.Application.Tools.Signing.SignTool;
using Fallout.Application.Tools.DotNet.NuGet;
using Fallout.Common.IO;
using Fallout.Common.IO;
using Fallout.Common;
using static Fallout.Common.ControlFlow;
using static Fallout.Application.Tools.DotNet.DotNetTasks;
using static Fallout.Application.Tools.DotNet.MSBuild.MSBuildTasks;
using static Fallout.Application.Tools.Signing.SignTool.SignToolTasks;
using static Fallout.Application.Tools.DotNet.NuGet.NuGetTasks;
using static Fallout.Common.IO.TextTasks;
using static Fallout.Common.IO.XmlTasks;
using static Fallout.Common.EnvironmentInfo;

class Build : FalloutBuild
{
    [Parameter] readonly string Target = "Default";
    [Parameter] readonly bool Boolean;
    [Parameter] readonly string Configuration = "Release";
    [Parameter("where")] readonly string ParameterRename;
    [Parameter] [Secret] readonly string CertificatePassword;
    [Parameter] [Secret] readonly object AwsAccessKeyId = GetVariable<string>("AWS_ACCESS_KEY") ?? "XXXX";
    GitVersion GitVersionInfo;
    string NuGetVersion;
}