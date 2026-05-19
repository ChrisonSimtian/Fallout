// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using FluentAssertions;
using Xunit;

// ReSharper disable ArgumentsStyleLiteral

namespace Nuke.Common.Tests;

public class ControlFlowTest
{
    [Fact]
    public void Test()
    {
        var executions = 0;

        void OnSecondExecution()
        {
            executions++;
            if (executions != 2)
                throw new Exception(executions.ToString());
        }

        ControlFlow.ExecuteWithRetry(OnSecondExecution);
        executions.Should().Be(2);
    }
}
