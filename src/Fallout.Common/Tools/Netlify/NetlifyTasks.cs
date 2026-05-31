using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Common.Utilities;
using Fallout.Application.Tooling;
using Fallout.Infrastructure.Tooling;

namespace Fallout.Application.Tools.Netlify;

partial class NetlifyTasks
{
    protected override object GetResult<T>(ToolOptions options, IReadOnlyCollection<Output> output)
    {
        if (options is NetlifySitesCreateSettings)
        {
            return output.EnsureOnlyStd().Select(x => x.Text)
                .Single(x => x.Contains("Site ID:"))
                .SplitSpace().Last();
        }

        return null;
    }
}
