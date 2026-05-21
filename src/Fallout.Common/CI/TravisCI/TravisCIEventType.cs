// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;

// ReSharper disable InconsistentNaming

namespace Fallout.Common.CI.TravisCI;

public enum TravisCIEventType
{
    push,
    pull_request,
    api,
    cron
}
