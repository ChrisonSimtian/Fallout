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
        build = new ProfileBuild(root);
    }

    [Fact]
    public void Enumerations_load_from_strings()
    {
        Read<Configuration>("Configuration", "\"Release\"").Should().BeSameAs(Configuration.Release);
    }

    [Theory]
    [InlineData("\"Quiet\"")]
    [InlineData("3")]
    public void Enum_values_load_from_names_or_numbers(string json)
    {
        Read<Verbosity>("Verbosity", json).Should().Be(Verbosity.Quiet);
    }

    [Fact]
    public void Guids_load_from_strings()
    {
        var id = Guid.NewGuid();

        Read<Guid>("Id", $"\"{id}\"").Should().Be(id);
    }

    [Fact]
    public void Declared_json_converters_keep_precedence()
    {
        Read<Mode>("Mode", "\"custom-alias\"").Should().Be(Mode.Custom);
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

    private T Read<T>(string name, string json)
    {
        var parameters = new JsonObject { [name] = JsonNode.Parse(json) };
        (root / ".fallout" / "parameters.json").WriteAllText(parameters.ToJsonString());
        LoadFiles();
        return (T)ParameterService.Instance.GetParameter(name, typeof(T).GetNullableType(), separator: null);
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

    [JsonConverter(typeof(ModeConverter))]
    private enum Mode { Default, Custom }

    private sealed class ModeConverter : JsonConverter<Mode>
    {
        public override Mode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.GetString() == "custom-alias" ? Mode.Custom : Mode.Default;

        public override void Write(Utf8JsonWriter writer, Mode value, JsonSerializerOptions options)
            => throw new NotSupportedException();
    }
}
