// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Fallout.Common;

namespace Fallout.Components;

[PublicAPI]
[ParameterPrefix(Twitter)]
public interface IHazTwitterCredentials : INukeBuild
{
    public const string Twitter = nameof(Twitter);

    [Parameter] [Secret] string ConsumerKey => TryGetValue(() => ConsumerKey);
    [Parameter] [Secret] string ConsumerSecret => TryGetValue(() => ConsumerSecret);
    [Parameter] [Secret] string AccessToken => TryGetValue(() => AccessToken);
    [Parameter] [Secret] string AccessTokenSecret => TryGetValue(() => AccessTokenSecret);
}