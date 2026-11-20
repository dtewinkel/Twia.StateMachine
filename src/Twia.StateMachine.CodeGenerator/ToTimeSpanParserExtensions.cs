using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;

namespace Twia.StateMachine.CodeGenerator;

public static class ToTimeSpanParserExtensions
{
    public static bool TryParsePeriod(this string? periodString, [NotNullWhen(true)] out TimeSpan? timeSpan)
    {
        if(string.IsNullOrWhiteSpace(periodString))
        {
            timeSpan = null;
            return false;
        }

        if (!TimeSpan.TryParse(periodString, CultureInfo.InvariantCulture, out var period))
        {
            try
            {
                period = XmlConvert.ToTimeSpan(periodString);
            }
            catch (FormatException)
            {
                timeSpan = null;
                return false;
            }
        }

        timeSpan = period;
        return timeSpan > TimeSpan.Zero && timeSpan < TimeSpan.FromDays(32);
    }
}