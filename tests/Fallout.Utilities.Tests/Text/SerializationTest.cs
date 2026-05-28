using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Fallout.Common.IO;
using Fallout.Common.Utilities;
using Fallout.Utilities.Text.Yaml;
using Xunit;

namespace Fallout.Common.Tests;

public class SerializationTest
{
    [Fact]
    public void JsonTest()
    {
        var data = CreateData("Json");
#pragma warning disable CS0618 // Test pins Newtonsoft round-trip semantics; STJ equivalents will get their own test cases in v11.
        var content = data.ToJson();
        var copy = content.GetJson<Data>();
#pragma warning restore CS0618

        copy.Should().BeEquivalentTo(data);
    }

    [Fact]
    public void YamlTest()
    {
        var data = CreateData("Yaml");
        var content = data.ToYaml();
        var copy = content.GetYaml<Data>();

        copy.Should().BeEquivalentTo(data);
    }

    [Fact]
    public void XmlTest()
    {
        var data = CreateData("Xml");
        var content = data.ToXml();
        var copy = content.GetXml<Data>();

        copy.Should().BeEquivalentTo(data);
    }

    private static Data CreateData(string name)
    {
        return new Data
               {
                   String = name,
                   Number = 5,
                   Boolean = true,
                   Nested = new Data
                            {
                                Boolean = false
                            }
               };
    }

    public class Data
    {
        public string String { get; set; }
        public int Number { get; set; }
        public bool Boolean { get; set; }

        public Data Nested { get; set; }
    }
}
