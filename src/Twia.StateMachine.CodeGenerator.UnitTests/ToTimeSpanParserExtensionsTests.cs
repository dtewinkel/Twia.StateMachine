using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[TestClass]
public sealed class ToTimeSpanParserExtensionsTests
{
    [TestMethod]
    [DataRow("00:00:01", 10_000_000L)]
    [DataRow("1.02:03:04", 937_840_000_000L)]
    public void TryParsePeriod_TimeSpanFormat_ReturnsParsedDuration(string periodString, long expectedTicks)
    {
        var result = ToTimeSpanParserExtensions.TryParsePeriod(periodString, out var timeSpan);

        result.Should().BeTrue();
        timeSpan.Should().Be(TimeSpan.FromTicks(expectedTicks));
    }

    [TestMethod]
    public void TryParsePeriod_XmlDurationFormat_ReturnsParsedDuration()
    {
        var result = ToTimeSpanParserExtensions.TryParsePeriod("P1DT2H3M4S", out var timeSpan);

        result.Should().BeTrue();
        timeSpan.Should().Be(new TimeSpan(1, 2, 3, 4));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("not a period")]
    public void TryParsePeriod_InvalidOrBlankInput_ReturnsFalseAndNull(string periodString)
    {
        var result = ToTimeSpanParserExtensions.TryParsePeriod(periodString, out var timeSpan);

        result.Should().BeFalse();
        timeSpan.Should().BeNull();
    }

    [TestMethod]
    public void TryParsePeriod_NullInput_ReturnsFalseAndNull()
    {
        var result = ToTimeSpanParserExtensions.TryParsePeriod(null, out var timeSpan);

        result.Should().BeFalse();
        timeSpan.Should().BeNull();
    }

    [TestMethod]
    [DataRow("00:00:00", 0L)]
    [DataRow("-00:00:01", -10_000_000L)]
    [DataRow("32.00:00:00", 27_648_000_000_000L)]
    public void TryParsePeriod_DurationOutsideAllowedRange_ReturnsFalseWithParsedDuration(string periodString, long expectedTicks)
    {
        var result = ToTimeSpanParserExtensions.TryParsePeriod(periodString, out var timeSpan);

        result.Should().BeFalse();
        timeSpan.Should().Be(TimeSpan.FromTicks(expectedTicks));
    }
}
