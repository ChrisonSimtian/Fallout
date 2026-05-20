// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

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
