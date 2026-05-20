// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Azure.Security.KeyVault.Keys;

namespace Fallout.Common.Tools.AzureKeyVault
{
    public class AzureKeyVaultKey
    {
        public JsonWebKey Key { get; internal set; }
        public string Secret { get; internal set; }
    }
}
