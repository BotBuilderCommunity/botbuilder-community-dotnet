using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bot.Builder.Community.Dialogs.Luis.BuiltIn.DateTime;

namespace Bot.Builder.Community.Dialogs.Luis
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/Gregorian_calendar
    /// </summary>
    public sealed class WesternCalendarPlus : ICalendarPlus
    {
        Calendar ICalendarPlus.Calendar => CultureInfo.InvariantCulture.Calendar;

        DayOfWeek ICalendarPlus.FirstDayOfWeek => DayOfWeek.Sunday;

        CalendarWeekRule ICalendarPlus.WeekRule => CalendarWeekRule.FirstDay;

        int ICalendarPlus.HourFor(DayPart dayPart)
        {
            switch (dayPart)
            {
                case DayPart.MO: return 9;
                case DayPart.MI: return 12;
                case DayPart.AF: return 15;
                case DayPart.EV: return 18;
                case DayPart.NI: return 21;
                default: throw new NotImplementedException();
            }
        }
    }
}
