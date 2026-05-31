using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Fallout.Common.Execution;
using Fallout.Common.Utilities;

namespace Fallout.Common;

public partial class Host
{
    internal static Host Instance { get; private set; }

    internal static Host Default =>
        AvailableTypes
            .OrderBy(x => x.IsAssignableTo(typeof(Terminal)))
            .ThenBy(x => x == typeof(Terminal))
            .Select(CreateHost)
            .First(x => x.IsActive);

    internal static IEnumerable<Type> AvailableTypes
        => AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => x.IsPublic)
            .Where(x => x.IsSubclassOf(typeof(Host)));

    // ADR-0005 (#3): discovery probes this overridable member instead of reflecting for a
    // magic-string `IsRunning{TypeName}` static. The default falls back to that legacy convention,
    // so every existing host keeps working unchanged; a new adapter just overrides IsActive (no
    // static, no name-matching). Host constructors are side-effect-free, so probing by construction
    // is cheap, and because First() stops at the active host it is the last constructed — and so
    // remains Host.Instance (set in the base ctor).
    protected internal virtual bool IsActive => IsRunningByConvention(GetType());

    private static bool IsRunningByConvention(Type hostType)
    {
        var propertyName = $"IsRunning{hostType.Name}";
        var member = hostType.GetProperty(propertyName, ReflectionUtility.Static)
            .NotNull($"Host type '{hostType.Name}' defines no property '{propertyName}'");
        return member.GetValue<bool>();
    }

    private static Host CreateHost(Type hostType)
    {
        return (Host)Activator.CreateInstance(hostType, nonPublic: true);
    }

    private class TypeConverter : System.ComponentModel.TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string stringValue)
            {
                var matchingHosts = AvailableTypes.Where(x => x.FullName.EndsWithOrdinalIgnoreCase(stringValue)).ToList();
                Assert.HasSingleItem(matchingHosts);
                return CreateHost(matchingHosts.Single());
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return false;
        }
    }
}
