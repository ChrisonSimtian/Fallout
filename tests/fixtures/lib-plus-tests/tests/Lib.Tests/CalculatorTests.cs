using Lib;
using Xunit;

public class CalculatorTests
{
    [Fact]
    public void Add_sums_its_arguments() => Assert.Equal(5, Calculator.Add(2, 3));
}
