using System;
using System.Globalization;
using static Bot.Builder.Community.Dialogs.Luis.BuiltIn.DateTime;

namespace Bot.Builder.Community.Dialogs.Luis
{
    /// <summary>
    /// Policy for interpreting LUIS resolutions.
    /// </summary>
    public interface ICalendarPlus
    {
        Calendar Calendar { get; }

        CalendarWeekRule WeekRule { get; }

        DayOfWeek FirstDayOfWeek { get; }

        int HourFor(DayPart dayPart);
    }
}
