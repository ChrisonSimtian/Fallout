using System;
using System.Linq;
using System.Text.Json.Nodes;
using Fallout.Common.Execution;
using FluentAssertions;
using Xunit;

namespace Fallout.Common.Specs;

public class GuidSchemaSpecs
{
    [Fact]
    public void Guid_parameters_use_the_runtime_string_shape()
    {
        var schema = JsonNode.Parse(SchemaUtility.GetJsonString(new GuidBuild()));
        var properties = schema["allOf"][0]["properties"];

        properties["Id"]["type"].GetValue<string>().Should().Be("string");
        properties["Id"]["format"].GetValue<string>().Should().Be("guid");
        properties["OptionalId"]["type"].AsArray().Select(x => x.GetValue<string>())
            .Should().BeEquivalentTo(new[] { "string", "null" });
        properties["Ids"]["items"]["type"].GetValue<string>().Should().Be("string");
        properties["Ids"]["items"]["format"].GetValue<string>().Should().Be("guid");
        schema["definitions"]["Nested"]["properties"]["Id"]["type"].GetValue<string>().Should().Be("string");
    }

    [Fact]
    public void Schema_line_endings_are_stable_across_operating_systems()
    {
        SchemaUtility.GetJsonString(new GuidBuild()).Should().NotContain("\r\n");
    }

#pragma warning disable CS0649
    private sealed class GuidBuild : FalloutBuild
    {
        [Parameter] public Guid Id;
        [Parameter] public Guid? OptionalId;
        [Parameter] public Guid[] Ids;
        [Parameter] public Nested Object;
    }

    private sealed class Nested
    {
        public Guid Id;
    }
#pragma warning restore CS0649
}
