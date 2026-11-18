using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Fallout.Build.Execution;
using Fallout.Common.CI;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;
using Fallout.Common.ValueInjection;

namespace Fallout.Common.Execution;

public class ArgumentsFromParametersFileAttribute : BuildExtensionAttributeBase, IOnBuildCreated
{
    private static readonly JsonSerializerOptions serializerOptions = new()
    {
        Converters = { new ParameterJsonConverterFactory() }
    };

    public void OnBuildCreated(IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        if (!Constants.GetFalloutDirectory(Build.RootDirectory).DirectoryExists())
        {
            return;
        }

        // IEnumerable<string> ConvertToArguments(string profile, string name, string[] values)
        // {
        //     var member = parameterMembers.SingleOrDefault(x => ParameterService.GetParameterMemberName(x).EqualsOrdinalIgnoreCase(name));
        //     var scalarType = member?.GetMemberType().GetScalarType();
        //     var mustDecrypt = (member?.HasCustomAttribute<SecretAttribute>() ?? false) && !BuildServerConfigurationGeneration.IsActive;
        //     var decryptedValues = values.Select(x => mustDecrypt ? DecryptValue(profile, name, x) : x);
        //     var convertedValues = decryptedValues.Select(x => ConvertValue(scalarType, x)).ToList();
        //     Log.Verbose("Passing value for {Member} ({Value})",
        //         member?.GetDisplayName() ?? "<unresolved>",
        //         !mustDecrypt ? convertedValues.JoinComma() : "secret");
        //     return new[] { $"--{ParameterService.GetParameterDashedName(name)}" }.Concat(convertedValues);
        // }
        //

        //
        // // TODO: Abstract AbsolutePath/Solution/Project etc.
        // string ConvertValue(Type scalarType, string value)
        //     => scalarType == typeof(AbsolutePath) ||
        //        typeof(Solution).IsAssignableFrom(scalarType) ||
        //        scalarType == typeof(Project)
        //         ? EnvironmentInfo.WorkingDirectory.GetUnixRelativePathTo(FalloutBuild.RootDirectory / value)
        //         : value;

        var parameterMembers = ValueInjectionUtility.GetParameterMembers(Build.GetType(), includeUnlisted: true);
        var parameterObjectsAndProfiles = new[]
            {
                (File: Constants.GetDefaultParametersFile(Build.RootDirectory), Profile: Constants.DefaultProfileName)
            }
            .Where(x => File.Exists(x.File))
            .Concat(Build.LoadedLocalProfiles.Select(x =>
                (File: Constants.GetParametersProfileFile(Build.RootDirectory, x), Profile: x)))
            .ForEachLazy(x => Assert.FileExists(x.File))
            .Select(x => (JsonObject: JsonNode.Parse(File.ReadAllText(x.File)).NotNull().AsObject(), x.Profile))
            .Reverse();

        var passwords = new Dictionary<string, string>();

        string DecryptValue(string profile, string name, string value)
            => EncryptionUtility.Decrypt(
                value,
                passwords[profile] = passwords.GetValueOrDefault(profile) ??
                                     CredentialStore.GetPassword(profile, Build.RootDirectory),
                name);

        ParameterService.Instance.ArgumentsFromFilesService = (parameter, destinationType) =>
        {
            var (value, profile) = parameterObjectsAndProfiles.Select(x => (Value: x.JsonObject[parameter], x.Profile))
                .Where(x => x.Value != null)
                .FirstOrDefault();

            if (value == null)
            {
                return null;
            }

            var member = parameterMembers.SingleOrDefault(x =>
                ParameterService.GetParameterMemberName(x).EqualsOrdinalIgnoreCase(parameter));

            var scalarType = member?.GetMemberType().GetScalarType();
            if (typeof(IAbsolutePathHolder).IsAssignableFrom(scalarType))
            {
                return value.GetValue<string>().Apply(x =>
                    !PathConstruction.HasPathRoot(x) ? Build.RootDirectory / x : (AbsolutePath)x);
            }

            if ((member?.HasCustomAttribute<SecretAttribute>() ?? false) &&
                !BuildServerConfigurationGeneration.IsActive)
            {
                return DecryptValue(profile, parameter, value.GetValue<string>());
            }

            return value.Deserialize(destinationType, serializerOptions);
        };
    }

    private sealed class ParameterJsonConverterFactory : JsonConverterFactory
    {
        private readonly EnumerationJsonConverterFactory enumerationConverter = new();
        private readonly JsonStringEnumConverter enumConverter = new();

        public override bool CanConvert(Type typeToConvert)
        {
            // Skip types with their own [JsonConverter]; converters in the options would otherwise win over the attribute.
            return !Attribute.IsDefined(typeToConvert, typeof(JsonConverterAttribute), inherit: false) &&
                   (enumerationConverter.CanConvert(typeToConvert) || enumConverter.CanConvert(typeToConvert));
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return enumerationConverter.CanConvert(typeToConvert)
                ? enumerationConverter.CreateConverter(typeToConvert, options)
                : enumConverter.CreateConverter(typeToConvert, options);
        }
    }
}
