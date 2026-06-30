using System;
using System.Linq;
using System.Reflection;

using Fallout.Common;
namespace Fallout.Application.Tools.Signing.AzureKeyVault
{
    /// <summary>Attribute to obtain the KeyVault defined by <see cref="AzureKeyVaultConfigurationAttribute"/> to retrieve multiple items.</summary>
    public class AzureKeyVaultAttribute : AzureKeyVaultAttributeBase
    {
        protected override object GetValue(AzureKeyVaultConfiguration configuration, MemberInfo member)
        {
            return AzureKeyVaultTasks.LoadVault(configuration);
        }
    }
}
