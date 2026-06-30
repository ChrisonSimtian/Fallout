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
    private void Convert()
    {
        DotNetBuild(_ => _
            .SetProjectFile(RootDirectory / "source")
            .SetConfiguration(configuration)
            .SetProcessArgumentConfigurator(args => args.Add($"/p:Version={nugetVersion}")));
        DotNetTest(_ => _
            .SetProjectFile(testProjectFile)
            .SetConfiguration(configuration)
            .SetNoBuild(true));
        DotNetPack(_ => _
            .SetProjectFile(octopusClientFolder)
            .SetProcessArgumentConfigurator(args =>
        {
            args.Add($"/p:Version={nugetVersion}");
            args.Add("/p:NuspecFile=file.nuspec");
            return args;
        })
            .SetConfiguration(configuration)
            .SetOutputDirectory(artifactsDir)
            .SetNoBuild(true)
            .SetIncludeSymbols(false)
            .SetVerbosity(DotNetVerbosity.Normal));
        SignTool(_ => _
            .SetFiles(files)
            .SetProcessToolPath(RootDirectory / "certificates" / "signtool.exe")
            .SetTimeStampUri(new Uri("http://rfc3161timestamp.globalsign.com/advanced"))
            .SetTimeStampDigestAlgorithm(SignToolDigestAlgorithm.Sha256)
            .SetCertPath(signingCertificatePath)
            .SetPassword(signingCertificatePassword));
    }
}