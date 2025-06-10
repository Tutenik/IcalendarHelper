using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using System.Net.Mail;

namespace ICalendarHelper
{
    internal class FreeBusyHelper
    {
        public static FreeBusy Create()
        {
            var fb = new FreeBusy
            {
                Uid = Guid.NewGuid().ToString(),
                DtStamp = new CalDateTime(DateTime.UtcNow),
            };

            Console.Clear();
            Console.WriteLine("========== VYTVOŘENÍ FREEBUSY ==========");

            Console.WriteLine("Zadejte e-mail organizátora:");

            MailAddress email;
            while (!MailAddress.TryCreate(Console.ReadLine(), out email))
                Console.WriteLine("Zadejte znovu e-mail organizátora:");

            fb.Organizer = new Organizer($"mailto:{email.Address}");

            Console.WriteLine("Zadejte od (dd. MM. yyyy):");
            DateTime start = CalendarHelper.GetDateFromUser(Console.ReadLine());

            Console.WriteLine("Zadejte do (dd. MM. yyyy):)");
            DateTime end = CalendarHelper.GetDateFromUser(Console.ReadLine());

            fb.Start = new CalDateTime(start);
            fb.End = new CalDateTime(end);

            return fb;
        }

        public static void Show(Calendar calendar)
        {
            int index = 1;
            foreach (var freeBusy in calendar.FreeBusy)
            {
                Console.WriteLine($"========== EVENT {index} ==========");
                Console.WriteLine($"Začátek: {freeBusy.Start?.Value:yyyy-MM-dd HH:mm}");
                Console.WriteLine($"Konec: {freeBusy.End?.Value:yyyy-MM-dd HH:mm}");

                if (freeBusy.Organizer != null)
                    Console.WriteLine($"Organizer: {freeBusy.Organizer.CommonName} <{freeBusy.Organizer.Value.OriginalString}>");

                if (freeBusy.Attendees.Count > 0)
                {
                    Console.WriteLine("Attendees:");
                    foreach (var attendee in freeBusy.Attendees)
                    {
                        Console.WriteLine($" - {attendee.CommonName} <{attendee.Value.OriginalString}>");
                    }
                }

                index++;
                Console.WriteLine();
            }
        }

        public static void Remove(Ical.Net.Calendar calendar)
        {
            bool opakovat = true;

            while (opakovat)
            {
                Console.WriteLine("========== FREEBUSY K ODEBRÁNÍ ==========");

                int index = 0;
                foreach (var freeBusy in calendar.FreeBusy)
                {
                    index++;
                    Console.WriteLine($"{index}. Začátek: {freeBusy.Start?.Value:yyyy-MM-dd HH:mm}, Konec: {freeBusy.End?.Value:yyyy-MM-dd HH:mm} (Uid: {freeBusy.Uid})");
                }

                if (index == 0) { Console.WriteLine("Není nic dostupného k odebrání."); Console.WriteLine("..."); Console.ReadKey(); return; }

                Console.WriteLine();

                Console.WriteLine("Zadejte číslo od 1. do " + index);

                int number;
                while (!int.TryParse(Console.ReadLine(), out number))
                    Console.WriteLine("Nesprávně zadané číslo, zadejte znovu číslo od 1. do " + index);

                var freeBusyToRemove = calendar.FreeBusy[number - 1];

                if (CalendarHelper.AreYouSure($"Chcete skutečně odebrat tento FREEBUSY se zařátkem: {freeBusyToRemove.Start?.Value:yyyy-MM-dd HH:mm}, koncem: {freeBusyToRemove.End?.Value:yyyy-MM-dd HH:mm} a Uid: {freeBusyToRemove.Uid}?"))
                {
                    calendar.FreeBusy.Remove(freeBusyToRemove);
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