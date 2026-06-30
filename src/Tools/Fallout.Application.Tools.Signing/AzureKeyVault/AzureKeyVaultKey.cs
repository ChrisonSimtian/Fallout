using System;
using System.Linq;
using Azure.Security.KeyVault.Keys;

using Fallout.Common;
namespace Fallout.Application.Tools.Signing.AzureKeyVault
{
    public class AzureKeyVaultKey
    {
        public JsonWebKey Key { get; internal set; }
        public string Secret { get; internal set; }
    }
}
