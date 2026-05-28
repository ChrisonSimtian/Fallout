using System;
using System.Linq;
using System.Xml.Linq;

namespace Fallout.Common.Utilities;

public static partial class XElementExtensions
{
    public static string GetAttributeValue(this XElement element, string name)
    {
        return element.Attribute(name).NotNull().Value;
    }
}
