using System;
using System.Linq;

namespace Fallout.Application.CI;

public enum AzurePipelinesTestResultsType
{
    JUnit,
    NUnit,
    VSTest,
    XUnit,
    CTest
}
