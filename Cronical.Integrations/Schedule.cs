using System.Collections;

namespace Cronical.Integrations;

public class Schedule
{
    public BitArray Weekdays { get; } = new(7);
    public BitArray Months { get; } = new(12);
    public BitArray Days { get; } = new(31);
    public BitArray Hours { get; } = new(24);
    public BitArray Minutes { get; } = new(60);

    public bool Matches(DateTime test) =>
        Minutes.Get(test.Minute) &&
        Hours.Get(test.Hour) &&
        Days.Get(test.Day - 1) &&
        Months.Get(test.Month - 1) &&
        Weekdays.Get((int)test.DayOfWeek);
}
