using Ical.Net;

namespace ICalendarHelper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CalendarHelper ch = new CalendarHelper(new Calendar());
            while (true)
            {
                Console.Clear();
                ch.WriteMenu();
                ch.Input();
            }
        }
    }
}
