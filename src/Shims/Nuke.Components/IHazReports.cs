// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;

namespace Nuke.Components;

[Obsolete("Renamed to IHasReports. The IHaz* names were legacy NUKE conventions; renamed for clarity in Fallout v11. This alias is shipped only in the Nuke.Components transition shim and will be removed in v12.")]
public interface IHazReports : IHasReports { }
