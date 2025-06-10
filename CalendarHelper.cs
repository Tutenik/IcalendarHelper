using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;
using System;
using System.Globalization;

namespace To_Do
{
    internal class CalendarHelper
    {
        private Ical.Net.Calendar _calendar;

        public CalendarHelper(Ical.Net.Calendar calendar)
        {
            _calendar = calendar;

            TimeZoneInfo.TryConvertWindowsIdToIanaId(TimeZoneInfo.Local.Id, RegionInfo.CurrentRegion.TwoLetterISORegionName, out string? timezone);
            _calendar.AddTimeZone(new VTimeZone(timezone ?? "Europe/Prague"));
        }

        public void WriteMenu()
        {
            Console.WriteLine("========== MENU ==========");
            Console.WriteLine("1. Zobrazit");
            Console.WriteLine("2. Přidat");
            Console.WriteLine("3. Odebrat");
            Console.WriteLine("4. Exportovat");
            Console.WriteLine("5. Importovat");
            Console.WriteLine("6. Zavřit");
            Console.WriteLine();
        }

        public void Input()
        {
            string? input = Console.ReadLine();
            bool opakovat = true;

            while (opakovat)
            {
                opakovat = false;
                switch (input.ToLower())
                {
                    case "1":
                    case "zobrazit":
                        Show();
                        break;

                    case "2":
                    case "pridat":
                    case "přidat":
                        Add();
                        break;

                    case "3":
                    case "odebrat":
                        Remove();
                        break;

                    case "4":
                    case "exportovat":
                        Export();
                        break;

                    case "5":
                    case "importovat":
                        Import();
                        break;

                    case "6":
                    case "zavrit":
                    case "zavřít":
                        if (AreYouSure("Nezapoměli jste si uložit přidané položky")) Export();
                        Environment.Exit(0);
                        break;

                    default:
                        Console.WriteLine("Nesprávný vstup, zadejte znovu.");
                        input = Console.ReadLine();
                        opakovat = true;
                        break;
                }
            }
        }

        private void Export()
        {
            Console.Clear();
            Console.WriteLine("========== EXPORT ==========");

            while (true)
            {
                Console.WriteLine("Zadejte cestu (nechte prázdné pro uložení do složdy dokumentů):");

                string? input = Console.ReadLine();
                string finalPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); ;

                if (Directory.Exists(Path.GetDirectoryName(input)))
                {
                    finalPath = input;
                }

                Console.WriteLine();

                Console.WriteLine("Pojmenujte soubor: ");
                string nameInput = Console.ReadLine();
                finalPath += $@"\{nameInput}.ics";

                if (AreYouSure($"Chcete použít tuto cestu: {finalPath}"))
                {
                    var serializer = new CalendarSerializer();
                    var serializedCalendar = serializer.SerializeToString(_calendar);
                    File.WriteAllText(finalPath, serializedCalendar);

                    Console.WriteLine("...");
                    Console.ReadKey();
                    break;
                }
            }
        }

        private void Import()
        {
            Console.Clear();
            Console.WriteLine("========== IMPORT ==========");

            while (true)
            {
                Console.WriteLine("Přetáhněte soubor:");
                string file = Console.ReadLine();

                if (!Path.Exists(file))
                {
                    Console.WriteLine("Cesta neexistuje.");
                    continue;
                }

                string icsContent = File.ReadAllText(file);
                _calendar = Ical.Net.Calendar.Load(icsContent);
                Console.WriteLine("Importováno správně.");
                Console.ReadKey();
                break;
            }
        }

        private void Show()
        {
            Console.Clear();
            Console.WriteLine("========== CO CHCETE ZOPRAZIT? ==========");
            Console.WriteLine("1. Událost");
            Console.WriteLine("2. To-Do");
            Console.WriteLine("3. Volno");
            Console.WriteLine("4. Vše");
            Console.WriteLine();

            switch (Console.ReadLine())
            {
                case "1":
                    EventHelper.Show(_calendar);
                    Console.WriteLine("...");
                    Console.ReadKey();
                    break;

                case "2":
                    ToDoHelper.Show(_calendar);
                    Console.WriteLine("...");
                    Console.ReadKey();
                    break;

                case "3": 
                    FreeBusyHelper.Show(_calendar);
                    Console.WriteLine("...");
                    Console.ReadKey();
                    break;

                case "4":
                    EventHelper.Show(_calendar);
                    ToDoHelper.Show(_calendar);
                    FreeBusyHelper.Show(_calendar);
                    Console.WriteLine("...");
                    Console.ReadKey();
                    break;

                default:
                    break;
            }
        }

        private void Add()
        {
            Console.Clear();
            Console.WriteLine("========== CO CHCETE PŘIDAT ==========");
            Console.WriteLine("1. Událost");
            Console.WriteLine("2. To-Do");
            Console.WriteLine("3. Volno");
            Console.WriteLine();

            switch (Console.ReadLine().ToLower())
            {
                case "1":
                    _calendar.Events.Add(EventHelper.Create());
                    Console.WriteLine("Event byl vytvořen úspěšně.");
                    Console.ReadKey();
                    break;
                case "2":
                    _calendar.Todos.Add(ToDoHelper.Create());
                    Console.WriteLine("ToDo byl vytvořen úspěšně.");
                    Console.ReadKey();
                    break;
                case "3":
                    _calendar.FreeBusy.Add(FreeBusyHelper.CreateFreeBusyComponent());
                    Console.WriteLine("FreeBusy byl vytvořen úspěšně.");
                    Console.ReadKey();
                    break;
                default:
                    break;
            }
        }

        private void Remove()
        {
            Console.Clear();
            Console.WriteLine("========== CO CHCETE ODEBRAT ==========");
            Console.WriteLine("1. Událost");
            Console.WriteLine("2. To-Do");
            Console.WriteLine("3. Volno");
            Console.WriteLine();

            switch (Console.ReadLine())
            {
                case "1":
                    EventHelper.Remove(_calendar);
                    Console.WriteLine("Event byl odebrán úspěšně.");
                    Console.ReadKey();
                    break;

                case "2":
                    ToDoHelper.Remove(_calendar);
                    Console.WriteLine("ToDo byl odebrán úspěšně.");
                    Console.ReadKey();
                    break;

                case "3":
                    FreeBusyHelper.Remove(_calendar);
                    Console.WriteLine("FreeBusy byl odebrán úspěšně.");
                    Console.ReadKey();
                    break;

                default:
                    break;
            }
        }

        public static DateTime GetDateFromUser(string input)
        {
            DateTime date;

            bool parseSucc = DateTime.TryParseExact(input,
                    "dd. MM. yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out date);

            if (parseSucc)
            {
                Console.WriteLine("zvolili jse datum: " + date.ToShortDateString());

                if (AreYouSure("Chcete ho použít?"))
                {
                    if (AreYouSure("Chcete Přidat čas k datu?"))
                    {
                        TimeSpan hours = GetTimeFromUser();
                        DateTime dateWithHours = date.Add(hours);
                        date = dateWithHours;
                    }
                }
                else parseSucc = false;
            }


            while (!parseSucc)
            {
                Console.WriteLine("Zadejte datum znovu: ");

                parseSucc = DateTime.TryParseExact(Console.ReadLine(),
                    "dd. MM. yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out date);

                if (!parseSucc) continue;

                Console.WriteLine("zvolili jse datum: " + date.ToShortDateString());

                if (!AreYouSure("Chcete ho použít?"))
                {
                    parseSucc = false;
                    continue;
                }

                if (AreYouSure("Chcete Přidat čas k datu?"))
                {
                    TimeSpan hours = GetTimeFromUser();
                    DateTime dateWithHours = date.Add(hours);
                    date = dateWithHours;
                }
            }

            return date;
        }

        public static TimeSpan GetTimeFromUser()
        {
            while (true)
            {
                Console.WriteLine("Zadejte čas (hh:mm): ");
                bool parsecuss = TimeSpan.TryParseExact(Console.ReadLine(),
                    @"hh\:mm",
                    CultureInfo.InvariantCulture,
                    TimeSpanStyles.None,
                    out TimeSpan hours);

                if (!parsecuss) continue;

                Console.WriteLine("zvolili jste čas: " + hours);

                if (!AreYouSure("Chcete ho použít?")) continue;

                return hours;
            }
        }

        public static bool AreYouSure(string question)
        {
            while (true)
            {
                Console.WriteLine(question + " (Y/N)");

                switch (Console.ReadLine().ToLower())
                {
                    case "n":
                    case "ne":
                    case "no":
                        return false;

                    case "y":
                    case "ano":
                    case "yes":
                        return true;

                    default:
                        continue;
                }

            }
        }
    }
}
