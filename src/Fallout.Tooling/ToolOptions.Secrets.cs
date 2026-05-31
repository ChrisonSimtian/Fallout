using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fallout.Common;
using Fallout.Kernel;

namespace Fallout.Application.Tooling;

partial class ToolOptions
{
    internal partial IEnumerable<string> GetSecrets()
    {
        return (ProcessRedactedSecrets ?? [])
            .Concat(InternalOptions
                .Select(kv => (Node: kv.Value, Property: _allProperties[kv.Key]))
                .Select(x => (x.Node, x.Property, Attribute: x.Property.GetCustomAttribute<ArgumentAttribute>()))
                .Where(x => x.Attribute?.Secret ?? false)
                .Select(x =>
                {
                    Assert.True(x.Property.GetMemberType() == typeof(string));
                    return x.Node.GetValue<string>();
                }));
    }
}
