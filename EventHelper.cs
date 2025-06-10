using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace To_Do
{
    enum STATUS
    {
        TENTATIVE,
        CONFIRMED,
        CANCELLED
    };

    internal class EventHelper
    {
        //private Guid _uid;
        //private DateTime _dtStamp;
        //private DateTime _dtStart;
        //private DateTime _dtEnd;
        //private string? _summary;
        //private string? _description;
        //private string? _location;
        //private string? _categories;
        //private string? _class;
        //private STATUS? _status;
        //private int? _sequence;
        //private int? _priority;
        //private bool? _transp;
        //private string? _organizer;
        //private string? _attendence;
        //private string? _contact;
        //private string? _url;
        //private string? _rRule;
        //private string? _exDate;
        //private string? _rDate;
        //private string? _recurenceID;
        //private string? _relatedTo;
        //private string? _attach;
        //private DateTime _created;
        //private DateTime _lastModified;
        //private string? _geo;
        //private Alarm? _vAlarm;

        public static CalendarEvent Create()
        {
            CalendarEvent newEvent = new CalendarEvent
            {
                Uid = Guid.NewGuid().ToString(),
                DtStamp = new CalDateTime(DateTime.UtcNow),
                Sequence = 0,
                Created = new CalDateTime(DateTime.UtcNow),
                LastModified = new CalDateTime(DateTime.UtcNow),
            };

            Console.Clear();
            Console.WriteLine("========== VYTVOŘENÍ EVENTU ==========");

            while (string.IsNullOrWhiteSpace(newEvent.Summary))
            {
                Console.WriteLine("Zadejte název události:");
                newEvent.Summary = Console.ReadLine();
            }

            Console.WriteLine("Zadejte popis (může být prázdný):");
            newEvent.Description = Console.ReadLine();

            Console.WriteLine("Zadejte lokaci (může být prázdná):");
            newEvent.Location = Console.ReadLine();

            Console.WriteLine("Zadejte kategorie (oddělené čárkou, nebo prázdné):");
            string categories = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(categories))
            {
                foreach (var catergory in categories.Split(','))
                {
                    newEvent.Categories.Add(catergory.Trim());
                }
            }

            Console.WriteLine("Zadejte začátek (ve formátu dd. MM. yyyy):");
            DateTime startDate = CalendarHelper.GetDateFromUser(Console.ReadLine());

            Console.WriteLine("Zadejte konec (dd. MM. yyyy HH:mm) (pokud prázdné, bude stejný):");
            string endInput = Console.ReadLine();

            DateTime endDate;
            if (!string.IsNullOrWhiteSpace(endInput))
                endDate = CalendarHelper.GetDateFromUser(endInput);
            else
                endDate = startDate;

            newEvent.Start = new CalDateTime(startDate);
            newEvent.End = new CalDateTime(endDate);

            Console.WriteLine("Stav události? (CONFIRMED, TENTATIVE, CANCELLED) nebo prázdné:");
            string status = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(status))
                newEvent.Status = status.ToUpper();

            if (CalendarHelper.AreYouSure("Chcete připomenutí?"))
            {
                Console.WriteLine("Kolik minut před začátkem chcete upozornění?");
                if (int.TryParse(Console.ReadLine(), out int minutesBefore))
                {
                    Alarm alarm = new Alarm
                    {
                        Action = AlarmAction.Display,
                        Summary = $"Připomenutí: {newEvent.Summary}",
                        Trigger = new Trigger(Duration.FromMinutes(-minutesBefore))
                    };

                    newEvent.Alarms.Add(alarm);
                }
            }

            if (CalendarHelper.AreYouSure("Chcete opakovat tuto událost? "))
                newEvent.RecurrenceRules = AddRecurrence();

            return newEvent;
        }

        //public static CalendarEvent CreateAlarm()
        //{
        //    CalendarEvent newEvent = new CalendarEvent
        //    {
        //        Uid = Guid.NewGuid().ToString(),
        //        DtStamp = new CalDateTime(DateTime.Now),
        //        Sequence = 0,
        //        Created = new CalDateTime(DateTime.Now),
        //        LastModified = new CalDateTime(DateTime.Now),
        //    };

        //    while (string.IsNullOrWhiteSpace(newEvent.Summary))
        //    {
        //        Console.WriteLine("Zadejte název:");
        //        newEvent.Summary = Console.ReadLine();
        //    }

        //    Console.WriteLine("Kdy chcete oznámení");

        //    DateTime date = GetDateFromUser(Console.ReadLine());

        //    newEvent.Start = new CalDateTime(date);



        //    Alarm alarm = new Alarm
        //    {
        //        Action = AlarmAction.Display,
        //        Summary = $"Připomenutí: {newEvent.Summary}",
        //        Trigger = new Trigger(Duration.FromMinutes(0))
        //    };

        //    newEvent.Alarms.Add(alarm);

        //    if (AreYouSure("Chcete opakovat tento připomenutí?"))
        //    {
        //        newEvent.RecurrenceRules = AddRecurrence();
        //    }

        //    return newEvent;
        //}

        static List<RecurrencePattern> AddRecurrence()
        {
            Console.WriteLine("Jak často? (daily, weekly, monthly, yearly):");

            FrequencyType frequency;

            while (!Enum.TryParse(Console.ReadLine(), true, out frequency))
                Console.WriteLine("Zadejte znovu. (daily, weekly, monthly, yearly)");

            Console.WriteLine("Za jak dlouho se to má opakovat? (např. 1 = každý týden, 2 = každý druhý týden...):");

            int interval;
            while (!int.TryParse(Console.ReadLine(), out interval))
                Console.WriteLine("Zadejte znovu (např. 1 = každý týden, 2 = každý druhý týden...):");

            var recurRule = new RecurrencePattern
            {
                Frequency = frequency,
                Interval = interval
            };

            while (true)
            {
                Console.WriteLine("Zadejte konec opakování (počet výskytů nebo datum dd. MM. yyyy nebo prázdné):");
                var untilInput = Console.ReadLine();

                DateTime untilDate;
                if (int.TryParse(untilInput, out int count))
                {
                    recurRule.Count = count;
                    if (CalendarHelper.AreYouSure($"Zadali jsete {count} počet opakování, chcete to použít?")) break;
                }
                else if (DateTime.TryParseExact(untilInput,
                        "dd. MM. yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out untilDate))
                {
                    recurRule.Until = new CalDateTime(untilDate);
                    if (CalendarHelper.AreYouSure($"Zadali jsete opakování do {untilDate}, chcete to použít?")) break;
                }

            }

            return new List<RecurrencePattern> { recurRule };
        }

        public static void Show(Ical.Net.Calendar calendar)
        {
            int index = 1;
            foreach (var e in calendar.Events)
            {
                Console.WriteLine($"========== EVENT {index} ==========");
                Console.WriteLine($"Název: {e.Summary}");
                Console.WriteLine($"Popis: {e.Description}");
                Console.WriteLine($"Lokace: {e.Location ?? "Žádná"}");

                Console.WriteLine($"Začátek: {e.Start?.Value}");
                Console.WriteLine($"Konec: {e.End?.Value}");

                if (!e.Duration.HasValue) 
                    Console.WriteLine($"Délka: {e.End?.Value.Subtract(e.Start.Value)}");
                else 
                    Console.WriteLine($"Délka: {e.Duration}");

                Console.WriteLine($"Vytvořeno: {e.Created?.Value}");
                Console.WriteLine($"Naposledy upraveno: {e.LastModified?.Value}");

                Console.WriteLine($"Status: {e.Status}");
                Console.WriteLine($"Počet upravení: {e.Sequence}");

                index++;
                Console.WriteLine();
            }
        }

        public static void Remove(Ical.Net.Calendar calendar)
        {
            bool opakovat = true;
            while (opakovat)
            {
                Console.WriteLine("========== EVENT K ODEBRÁNÍ ==========");
                int index = 0;
                foreach (var e in calendar.Events)
                {
                    index++;
                    Console.WriteLine($"{index}. {e.Summary} (Uid: {e.Uid})");
                }

                if (index == 0) { Console.WriteLine("Není nic dostupného k odebrání."); Console.WriteLine("..."); Console.ReadKey(); return; }

                Console.WriteLine();

                Console.WriteLine("Zadejte číslo od 1. do " + index);

                int number;
                while (!int.TryParse(Console.ReadLine(), out number))
                    Console.WriteLine("Nesprávně zadané číslo, zadejte znovu číslo od 1. do " + index);

                var eventToRemove = calendar.Events[number - 1];

                if (CalendarHelper.AreYouSure($"Chcete skutečně odebrat tento Event s názvem: {eventToRemove.Summary} a Uid: {eventToRemove.Uid}?"))
                {
                    calendar.Events.Remove(eventToRemove);
                    Console.WriteLine("Event byl odebrán.");
                }

                if (CalendarHelper.AreYouSure("Chcete zadat další k odebrání?"))
                    opakovat = true;
                else
                    opakovat = false;
            }
        }
    }
}