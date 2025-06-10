using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace To_Do
{
    internal class ToDoHelper
    {
        enum STATUS
        {
            NeedsAction,
            Completed,
            InProcess,
            Cancelled
        };

        public static Todo Create()
        {
            var newToDo = new Todo
            {
                Uid = Guid.NewGuid().ToString(),
                DtStamp = new CalDateTime(DateTime.UtcNow),
            };

            Console.Clear();
            Console.WriteLine("========== VYTVOŘENÍ TODO ==========");

            Console.WriteLine("Zadejte začátek (ve formátu dd. MM. yyyy):");
            DateTime startDate = CalendarHelper.GetDateFromUser(Console.ReadLine());

            Console.WriteLine("Zadejte konec (ve formátu dd. MM. yyyy):");
            DateTime endDate = CalendarHelper.GetDateFromUser(Console.ReadLine());

            newToDo.Start = new CalDateTime(startDate);
            newToDo.Due = new CalDateTime(endDate);

            Console.Write("Zadejte název: ");
            string summary = Console.ReadLine();

            newToDo.Summary = summary;

            Console.Write("Zadejte popis: ");
            string description = Console.ReadLine();

            newToDo.Description = description;

            Console.Write("Zadejte důležitost (0-9, kde 0 je žádná a 1 je největší, 9 nejmenší): ");

            bool succ = int.TryParse(Console.ReadLine(), out int prio);
            int priority = succ ? prio : 0;

            newToDo.Priority = priority;

            Console.Write("Zadej status (NEEDSACTION, COMPLETED, CANCELLED, INPROGRESS): ");
            STATUS stat;

            while (!Enum.TryParse(Console.ReadLine(), true, out stat))
                Console.Write("Zadej znovu status (NEEDSACTION, COMPLETED, CANCELLED, INPROGRESS): ");

            switch (stat)
            {
                case STATUS.Cancelled:
                    newToDo.Status = TodoStatus.Cancelled;
                    break;

                case STATUS.Completed:
                    newToDo.Status += TodoStatus.Completed;
                    break;

                case STATUS.InProcess:
                    newToDo.Status = TodoStatus.InProcess;
                    break;

                case STATUS.NeedsAction:
                    newToDo.Status = TodoStatus.NeedsAction;
                    break;

                default:
                    newToDo.Status = TodoStatus.NeedsAction;
                    break;
            }

            return newToDo;
        }

        public static void Show(Calendar calendar)
        {
            int index = 1;
            foreach (var todo in calendar.Todos)
            {
                Console.WriteLine($"========== TODO {index} ==========");

                Console.WriteLine($"Název: {todo.Summary}");
                Console.WriteLine($"Popis: {todo.Description}");
                Console.WriteLine($"Zařátek: {todo.DtStart?.Value.ToString("yyyy-MM-dd") ?? "N/A"}");
                Console.WriteLine($"Do kdy: {todo.Due?.Value.ToString("yyyy-MM-dd") ?? "N/A"}");
                Console.WriteLine($"Status: {todo.Status}");
                Console.WriteLine($"Důležitost: {todo.Priority}");
                Console.WriteLine($"Vytvořeno: {todo.Created?.Value.ToString("yyyy-MM-dd") ?? "N/A"}");
                Console.WriteLine($"Naposledy upraveno: {todo.LastModified?.Value.ToString("yyyy-MM-dd") ?? "N/A"}");
                Console.WriteLine($"Dokončeno: {todo.Completed?.Value.ToString("yyyy-MM-dd") ?? "N/A"}");

                if (todo.Categories.Count > 0)
                {
                    Console.WriteLine("Kategorie: " + string.Join(", ", todo.Categories));
                }

                if (todo.Attendees.Count > 0)
                {
                    Console.WriteLine("Účastníci:");
                    foreach (var attendee in todo.Attendees)
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
                Console.WriteLine("========== TODO K ODEBRÁNÍ ==========");
                int index = 0;
                foreach (var toDo in calendar.Todos)
                {
                    index++;
                    Console.WriteLine($"{index}. {toDo.Summary} (Uid: {toDo.Uid})");
                }

                if (index == 0) { Console.WriteLine("Není nic dostupného k odebrání."); Console.WriteLine("..."); Console.ReadKey(); return; }

                Console.WriteLine();

                Console.WriteLine("Zadejte číslo od 1. do " + index);

                int number;
                while (!int.TryParse(Console.ReadLine(), out number))
                    Console.WriteLine("Nesprávně zadané číslo, zadejte znovu číslo od 1. do " + index);

                var toDoToRemove = calendar.Todos[number - 1];

                if (CalendarHelper.AreYouSure($"Chcete skutečně odebrat tento TODO s názvem: {toDoToRemove.Summary} a Uid: {toDoToRemove.Uid}?"))
                {
                    calendar.Todos.Remove(toDoToRemove);
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
