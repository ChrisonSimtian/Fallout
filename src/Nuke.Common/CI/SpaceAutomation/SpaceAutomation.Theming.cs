// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using Nuke.Common.Execution;

namespace Nuke.Common.CI.SpaceAutomation;

public partial class SpaceAutomation
{
    internal override string OutputTemplate => Logging.StandardOutputTemplate;
}
