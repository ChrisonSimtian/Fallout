// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Fallout.Common.Tools.AzureKeyVault
{
    /// <summary>Attribute to obtain a secret from the Azure KeyVault defined by <see cref="AzureKeyVaultConfigurationAttribute"/>.</summary>
    [PublicAPI]
    public class AzureKeyVaultSecretAttribute : AzureKeyVaultAttributeBase
    {
        private readonly string _secretName;

        public AzureKeyVaultSecretAttribute(string secretName = null)
        {
            _secretName = secretName;
        }

        protected override object GetValue(AzureKeyVaultConfiguration configuration, MemberInfo member)
        {
            return ParameterService.GetParameter<string>(member.Name) ??
                   AzureKeyVaultTasks.GetSecret(configuration, _secretName ?? member.Name);
        }
    }
}
