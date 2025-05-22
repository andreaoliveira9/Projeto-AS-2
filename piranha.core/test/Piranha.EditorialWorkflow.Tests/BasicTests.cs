/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Xunit;

namespace Piranha.EditorialWorkflow.Tests;

/// <summary>
/// Simple test to verify xUnit is working
/// </summary>
public class BasicTests
{
    [Fact]
    public void Should_Pass_Basic_Test()
    {
        // Arrange
        var expected = 42;
        
        // Act
        var actual = 42;
        
        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_Test_String_Operations()
    {
        // Arrange
        var input = "Piranha Editorial Workflow";
        
        // Act
        var result = input.ToUpper();
        
        // Assert
        Assert.Equal("PIRANHA EDITORIAL WORKFLOW", result);
        Assert.Contains("EDITORIAL", result);
    }
}
