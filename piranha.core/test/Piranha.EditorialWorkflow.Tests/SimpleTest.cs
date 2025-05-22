using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Piranha.EditorialWorkflow.Tests;

public class SimpleTest
{
    [Fact]
    public void Test_Addition()
    {
        var result = 2 + 2;
        Assert.Equal(4, result);
    }
}
