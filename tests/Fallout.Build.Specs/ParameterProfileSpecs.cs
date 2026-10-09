using System;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Fallout.Build.Execution;
using Fallout.Common.Execution;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities;
using FluentAssertions;
using Xunit;

namespace Fallout.Common.Specs;

[Collection(ProcessGlobalStateCollection.Name)]
public sealed class ParameterProfileSpecs : IDisposable
{
    private readonly AbsolutePath root = AbsolutePath.Temp("parameter-profiles").CreateDirectory();
    private readonly BuildContext context = BuildContext.Activate();
    private readonly ProfileBuild build;

    public ParameterProfileSpecs()
    {
        (root / ".fallout").CreateDirectory();
        build = new ProfileBuild(root);
    }

    [Fact]
    public void Enumeration_strings_load_without_consumer_json_converters()
    {
        var value = Read("Configuration", typeof(Configuration), "\"Release\"");

        value.Should().BeSameAs(Configuration.Release);
    }

    [Fact]
    public void Enumeration_arrays_load_from_strings()
    {
        var value = Read("Configurations", typeof(Configuration[]), "[\"Debug\",\"Release\"]");

        value.Should().BeEquivalentTo(new[] { Configuration.Debug, Configuration.Release });
    }

    [Theory]
    [InlineData("\"Quiet\"")]
    [InlineData("3")]
    public void Enum_values_load_from_names_or_numbers(string json)
    {
        Read("Verbosity", typeof(Verbosity), json).Should().Be(Verbosity.Quiet);
    }

    [Fact]
    public void Enum_arrays_load_from_strings()
    {
        Read("Verbosities", typeof(Verbosity[]), "[\"Normal\",\"Quiet\"]")
            .Should().BeEquivalentTo(new[] { Verbosity.Normal, Verbosity.Quiet });
    }

    [Fact]
    public void Declared_json_converters_keep_precedence()
    {
        Read("CustomAuth", typeof(CustomSignInMode), "\"external\"").Should().Be(CustomSignInMode.Hanko);
    }

    [Fact]
    public void Named_profiles_override_default_parameters()
    {
        (root / ".fallout" / "parameters.json").WriteAllText("{\"Configuration\":\"Debug\"}");
        (root / ".fallout" / "parameters.release.json").WriteAllText("{\"Configuration\":\"Release\"}");
        build.Profiles = new[] { "release" };

        LoadFiles();

        ParameterService.GetParameter<Configuration>("Configuration").Should().BeSameAs(Configuration.Release);
    }

    private object Read(string name, Type type, string json)
    {
        var parameters = new JsonObject { [name] = JsonNode.Parse(json) };
        (root / ".fallout" / "parameters.json").WriteAllText(parameters.ToJsonString());
        LoadFiles();
        return ParameterService.Instance.GetParameter(name, type.GetNullableType(), separator: null);
    }

    private void LoadFiles()
    {
        var extension = new ArgumentsFromParametersFileAttribute { Build = build };
        extension.OnBuildCreated(Array.Empty<ExecutableTarget>());
    }

    public void Dispose()
    {
        context.Dispose();
        root.DeleteDirectory();
    }

    private sealed class ProfileBuild(AbsolutePath root) : FalloutBuild, IFalloutBuild
    {
        public string[] Profiles { get; set; } = Array.Empty<string>();
        AbsolutePath IFalloutBuild.RootDirectory => root;
        string[] IFalloutBuild.LoadedLocalProfiles => Profiles;
    }

    [TypeConverter(typeof(TypeConverter<Configuration>))]
    private sealed class Configuration : Enumeration
    {
        public static readonly Configuration Debug = new() { Value = nameof(Debug) };
        public static readonly Configuration Release = new() { Value = nameof(Release) };
    }

    [JsonConverter(typeof(CustomSignInModeConverter))]
    private enum CustomSignInMode { Mock, Hanko }

    private sealed class CustomSignInModeConverter : JsonConverter<CustomSignInMode>
    {
        public override CustomSignInMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString() == "external" ? CustomSignInMode.Hanko : CustomSignInMode.Mock;
        }

        public override void Write(Utf8JsonWriter writer, CustomSignInMode value, JsonSerializerOptions options)
        {
            writer.WriteStringValue("external");
        }
    }
}
