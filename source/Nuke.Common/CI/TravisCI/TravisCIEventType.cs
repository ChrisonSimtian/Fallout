// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming

namespace Nuke.Common.CI.TravisCI;

[PublicAPI]
public enum TravisCIEventType
{
    push,
    pull_request,
    api,
    cron
}
