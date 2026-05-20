// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Fallout.Common.Utilities.Collections;
using Xunit;

namespace Fallout.Common.Tests;

public class DictionaryExtensionsTest
{
    [Fact]
    public static void ToGeneric()
    {
        var sourceDictionary = new Dictionary<string, string> { { "key", "value" }, { "key2", "value2" } };
        IDictionary dict = sourceDictionary;
        dict.ToGeneric<string, string>().Should().Equal(sourceDictionary);
    }
}
