using System;
using System.Linq;
using System.Reflection;
using Fallout.Application.ValueInjection;
using Fallout.Kernel.IO;
using Fallout.Kernel;
using Fallout.Application.IO;

namespace Fallout.Application.Tooling;

public class LatestMyGetVersionAttribute : ValueInjectionAttributeBase
{
    private readonly string _feed;
    private readonly string _package;

    public LatestMyGetVersionAttribute(string feed, string package)
    {
        _feed = feed;
        _package = package;
    }

    public override object GetValue(MemberInfo member, object instance)
    {
        var content = HttpTasks.HttpDownloadString($"https://www.myget.org/RSS/{_feed}");
        return XmlTasks.XmlPeekFromString(content, ".//title")
            // TODO: regex?
            .First(x => x.Contains($"/{_package} "))
            .Split('(').Last()
            .Split(')').First()
            .TrimStart("version ");
    }
}
