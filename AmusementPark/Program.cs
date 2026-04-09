using AmusementPark.Таблицы;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace AmusementPark
{
    public class Program
    {
        public static void Main()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseNpgsql("Host=localhost;Port=5433;Database=AmusementParkDB;Username=postgres;Password=admin")
                .Options;

            try
            {
                using var db = new ApplicationContext(options);
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                Console.WriteLine("База данных готова.");

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("=== ПАРК АТТРАКЦИОНОВ ===");
                    Console.WriteLine("1. Аккаунт");
                    Console.WriteLine("2. Билеты");
                    Console.WriteLine("3. Аттракционы");
                    Console.WriteLine("4. Тарифы");
                    Console.WriteLine("0. Выход");
                    Console.Write("Выбор: ");
                    string choice = Console.ReadLine() ?? "";

                    if (choice == "0") break;
                    if (choice == "1") AccountMenu(db);
                    else if (choice == "2") TicketMenu(db);
                    else if (choice == "3") RideMenu(db);
                    else if (choice == "4") TariffMenu(db);
                    else Console.WriteLine("Неверный выбор");

                    if (choice != "0")
                    {
                        Console.WriteLine("Нажмите любую клавишу...");
                        Console.ReadKey();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.ReadKey();
            }
        }

        // ==================== АККАУНТ ====================
        static void AccountMenu(ApplicationContext db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== АККАУНТ ===");
                Console.WriteLine("1. Пользователи");
                Console.WriteLine("2. Профили");
                Console.WriteLine("3. Роли");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "0") break;
                if (choice == "1") UsersMenu(db);
                else if (choice == "2") ProfilesMenu(db);
                else if (choice == "3") RolesMenu(db);
            }
        }

        static void RolesMenu(ApplicationContext db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== РОЛИ ===");
                Console.WriteLine("1. Показать");
                Console.WriteLine("2. Добавить");
                Console.WriteLine("3. Изменить");
                Console.WriteLine("4. Удалить");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "0") break;

                if (choice == "1")
                {
                    Console.WriteLine("\nСписок ролей:");
                    foreach (var r in db.Role)
                        Console.WriteLine($"{r.Id} | {r.Name}");
                }
                else if (choice == "2")
                {
                    Console.Write("Название: ");
                    string name = (Console.ReadLine() ?? "").Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        Console.WriteLine("Название не может быть пустым.");
                    }
                    else
                    {
                        db.Role.Add(new Role { Name = name });
                        db.SaveChanges();
                        Console.WriteLine("Добавлено.");
                    }
                }
                else if (choice == "3")
                {
                    Console.Write("ID роли: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var role = db.Role.Find(id);
                        if (role == null)
                        {
                            Console.WriteLine("Роль не найдена.");
                        }
                        else
                        {
                            Console.Write($"Новое название (было: {role.Name}): ");
                            string name = (Console.ReadLine() ?? "").Trim();
                            if (!string.IsNullOrEmpty(name))
                            {
                                role.Name = name;
                                db.SaveChanges();
                                Console.WriteLine("Обновлено.");
                            }
                            else
                            {
                                Console.WriteLine("Название не изменено.");
                            }
                        }
                    }
                }
                else if (choice == "4")
                {
                    Console.Write("ID роли: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var role = db.Role.Find(id);
                        if (role == null)
                        {
                            Console.WriteLine("Роль не найдена.");
                        }
                        else if (db.Users.Any(u => u.RoleId == id))
                        {
                            Console.WriteLine("Нельзя удалить: есть пользователи с этой ролью.");
                        }
                        else
                        {
                            db.Role.Remove(role);
                            db.SaveChanges();
                            Console.WriteLine("Удалена.");
                        }
                    }
                }

                if (choice != "0")
                {
                    Console.WriteLine("Нажмите любую клавишу...");
                    Console.ReadKey();
                }
            }
        }

        static void UsersMenu(ApplicationContext db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ПОЛЬЗОВАТЕЛИ ===");
                Console.WriteLine("1. Показать");
                Console.WriteLine("2. Добавить");
                Console.WriteLine("3. Изменить");
                Console.WriteLine("4. Удалить");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "0") break;

                if (choice == "1")
                {
                    var users = db.Users.Include(u => u.Role).ToList();
                    Console.WriteLine("\nСписок пользователей:");
                    foreach (var u in users)
                        Console.WriteLine($"{u.Id} | {u.Email} | Роль: {u.Role?.Name ?? "—"} | Создан: {u.Date_Created}");
                }
                else if (choice == "2")
                {
                    Console.Write("Email: ");
                    string email = (Console.ReadLine() ?? "").Trim();
                    Console.Write("Пароль: ");
                    string password = Console.ReadLine() ?? "";
                    Console.Write("ID роли: ");
                    string input = Console.ReadLine() ?? "";

                    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    {
                        Console.WriteLine("Email и пароль обязательны.");
                    }
                    else if (!int.TryParse(input, out int roleId))
                    {
                        Console.WriteLine("Некорректный ID роли.");
                    }
                    else if (db.Role.Find(roleId) == null)
                    {
                        Console.WriteLine("Роль не найдена.");
                    }
                    else
                    {
                        db.Users.Add(new User
                        {
                            Email = email,
                            Password = password,
                            RoleId = roleId,
                            Date_Created = DateTime.Now
                        });
                        db.SaveChanges();
                        Console.WriteLine("Пользователь добавлен.");
                    }
                }
                else if (choice == "3")
                {
                    Console.Write("ID пользователя: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var user = db.Users.Find(id);
                        if (user == null)
                        {
                            Console.WriteLine("Пользователь не найден.");
                        }
                        else
                        {
                            Console.Write($"Email [{user.Email}]: ");
                            string email = (Console.ReadLine() ?? "").Trim();
                            if (!string.IsNullOrEmpty(email)) user.Email = email;

                            Console.Write($"Пароль (оставьте пустым, чтобы не менять): ");
                            string password = Console.ReadLine() ?? "";
                            if (!string.IsNullOrEmpty(password)) user.Password = password;

                            Console.Write($"ID роли [{user.RoleId}]: ");
                            string roleInput = Console.ReadLine() ?? "";
                            if (!string.IsNullOrEmpty(roleInput) && int.TryParse(roleInput, out int roleId) && db.Role.Find(roleId) != null)
                            {
                                user.RoleId = roleId;
                            }

                            db.SaveChanges();
                            Console.WriteLine("Обновлено.");
                        }
                    }
                }
                else if (choice == "4")
                {
                    Console.Write("ID пользователя: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var user = db.Users.Find(id);
                        if (user == null)
                        {
                            Console.WriteLine("Пользователь не найден.");
                        }
                        else if (db.Profile.Any(p => p.User_Id == id))
                        {
                            Console.WriteLine("У пользователя есть профиль. Сначала удалите профиль.");
                        }
                        else
                        {
                            db.Users.Remove(user);
                            db.SaveChanges();
                            Console.WriteLine("Пользователь удалён.");
                        }
                    }
                }

                if (choice != "0")
                {
                    Console.WriteLine("Нажмите любую клавишу...");
                    Console.ReadKey();
                }
            }
        }

        static void ProfilesMenu(ApplicationContext db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ПРОФИЛИ ===");
                Console.WriteLine("1. Показать");
                Console.WriteLine("2. Добавить");
                Console.WriteLine("3. Изменить");
                Console.WriteLine("4. Удалить");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "0") break;

                if (choice == "1")
                {
                    var profiles = db.Profile.Include(p => p.User).ToList();
                    Console.WriteLine("\nСписок профилей:");
                    foreach (var p in profiles)
                        Console.WriteLine($"{p.Id} | {p.First_Name} {p.Last_Name} | {p.Birth_Date} | {p.Phone} | User: {p.User?.Email ?? "—"}");
                }
                else if (choice == "2")
                {
                    Console.Write("ID пользователя: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int userId) || db.Users.Find(userId) == null)
                    {
                        Console.WriteLine("Пользователь не найден.");
                    }
                    else
                    {
                        Console.Write("Имя: ");
                        string firstName = (Console.ReadLine() ?? "").Trim();
                        Console.Write("Фамилия: ");
                        string lastName = (Console.ReadLine() ?? "").Trim();
                        Console.Write("Дата рождения (ГГГГ-ММ-ДД): ");
                        string dateStr = Console.ReadLine() ?? "";
                        if (!DateOnly.TryParse(dateStr, out DateOnly birth))
                        {
                            Console.WriteLine("Некорректная дата.");
                        }
                        else
                        {
                            Console.Write("Телефон: ");
                            string phone = (Console.ReadLine() ?? "").Trim();
                            db.Profile.Add(new Profile
                            {
                                User_Id = userId,
                                First_Name = firstName,
                                Last_Name = lastName,
                                Birth_Date = birth,
                                Phone = phone
                            });
                            db.SaveChanges();
                            Console.WriteLine("Профиль добавлен.");
                        }
                    }
                }
                else if (choice == "3")
                {
                    Console.Write("ID профиля: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var profile = db.Profile.Find(id);
                        if (profile == null)
                        {
                            Console.WriteLine("Профиль не найден.");
                        }
                        else
                        {
                            Console.Write($"Имя [{profile.First_Name}]: ");
                            string fn = (Console.ReadLine() ?? "").Trim();
                            if (!string.IsNullOrEmpty(fn)) profile.First_Name = fn;

                            Console.Write($"Фамилия [{profile.Last_Name}]: ");
                            string ln = (Console.ReadLine() ?? "").Trim();
                            if (!string.IsNullOrEmpty(ln)) profile.Last_Name = ln;

                            Console.Write($"Дата рождения [{profile.Birth_Date}]: ");
                            string dateStr = Console.ReadLine() ?? "";
                            if (DateOnly.TryParse(dateStr, out DateOnly birth)) profile.Birth_Date = birth;

                            Console.Write($"Телефон [{profile.Phone}]: ");
                            string phone = (Console.ReadLine() ?? "").Trim();
                            if (!string.IsNullOrEmpty(phone)) profile.Phone = phone;

                            db.SaveChanges();
                            Console.WriteLine("Профиль обновлён.");
                        }
                    }
                }
                else if (choice == "4")
                {
                    Console.Write("ID профиля: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var profile = db.Profile.Find(id);
                        if (profile == null)
                        {
                            Console.WriteLine("Профиль не найден.");
                        }
                        else if (db.Ticket.Any(t => t.Profile_id == id))
                        {
                            Console.WriteLine("У профиля есть билеты. Сначала удалите билеты.");
                        }
                        else
                        {
                            db.Profile.Remove(profile);
                            db.SaveChanges();
                            Console.WriteLine("Профиль удалён.");
                        }
                    }
                }

                if (choice != "0")
                {
                    Console.WriteLine("Нажмите любую клавишу...");
                    Console.ReadKey();
                }
            }
        }

        // ==================== БИЛЕТЫ ====================
        static void TicketMenu(ApplicationContext db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== БИЛЕТЫ ===");
                Console.WriteLine("1. Билеты");
                Console.WriteLine("2. Платежи");
                Console.WriteLine("3. Посещения");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "0") break;
                if (choice == "1") TicketsMenu(db);
                else if (choice == "2") PaymentsMenu(db);
                else if (choice == "3") VisitsMenu(db);
            }
        }

        static void TicketsMenu(ApplicationContext db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== БИЛЕТЫ ===");
                Console.WriteLine("1. Показать");
                Console.WriteLine("2. Добавить");
                Console.WriteLine("3. Изменить");
                Console.WriteLine("4. Удалить");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "0") break;

                if (choice == "1")
                {
                    var tickets = db.Ticket
                        .Include(t => t.Profile)
                        .Include(t => t.Price)
                        .Include(t => t.Payment)
                        .ToList();
                    Console.WriteLine("\nСписок билетов:");
                    foreach (var t in tickets)
                        Console.WriteLine($"{t.Id} | {t.Profile?.First_Name} {t.Profile?.Last_Name} | {t.Price?.Amount} руб. | {t.Start_Time} - {t.End_Time}");
                }
                else if (choice == "2")
                {
                    Console.Write("ID профиля: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int profileId) || db.Profile.Find(profileId) == null)
                    {
                        Console.WriteLine("Профиль не найден.");
                    }
                    else
                    {
                        Console.Write("ID цены: ");
                        string priceInput = Console.ReadLine() ?? "";
                        if (!int.TryParse(priceInput, out int priceId) || db.Price.Find(priceId) == null)
                        {
                            Console.WriteLine("Цена не найдена.");
                        }
                        else
                        {
                            Console.Write("Начало (ГГГГ-ММ-ДД ЧЧ:ММ): ");
                            string startStr = Console.ReadLine() ?? "";
                            if (!DateTime.TryParse(startStr, out DateTime start))
                            {
                                Console.WriteLine("Некорректная дата начала.");
                            }
                            else
                            {
                                Console.Write("Конец (ГГГГ-ММ-ДД ЧЧ:ММ): ");
                                string endStr = Console.ReadLine() ?? "";
                                if (!DateTime.TryParse(endStr, out DateTime end))
                                {
                                    Console.WriteLine("Некорректная дата окончания.");
                                }
                                else
                                {
                                    var ticket = new Ticket
                                    {
                                        Profile_id = profileId,
                                        Price_id = priceId,
                                        Start_Time = start,
                                        End_Time = end
                                    };
                                    db.Ticket.Add(ticket);
                                    db.SaveChanges();

                                    var price = db.Price.Find(priceId);
                                    db.Payment.Add(new Payment
                                    {
                                        Ticket_Id = ticket.Id,
                                        Amount = price!.Amount,
                                        Payment_Date = DateTime.Now
                                    });
                                    db.SaveChanges();
                                    Console.WriteLine($"Билет #{ticket.Id} создан, платёж на {price.Amount} руб.");
                                }
                            }
                        }
                    }
                }
                else if (choice == "3")
                {
                    Console.Write("ID билета: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var ticket = db.Ticket.Find(id);
                        if (ticket == null)
                        {
                            Console.WriteLine("Билет не найден.");
                        }
                        else
                        {
                            Console.Write($"Начало [{ticket.Start_Time}]: ");
                            string startStr = Console.ReadLine() ?? "";
                            if (DateTime.TryParse(startStr, out DateTime start)) ticket.Start_Time = start;

                            Console.Write($"Конец [{ticket.End_Time}]: ");
                            string endStr = Console.ReadLine() ?? "";
                            if (DateTime.TryParse(endStr, out DateTime end)) ticket.End_Time = end;

                            db.SaveChanges();
                            Console.WriteLine("Обновлено.");
                        }
                    }
                }
                else if (choice == "4")
                {
                    Console.Write("ID билета: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var ticket = db.Ticket.Find(id);
                        if (ticket == null)
                        {
                            Console.WriteLine("Билет не найден.");
                        }
                        else
                        {
                            db.Payment.RemoveRange(db.Payment.Where(p => p.Ticket_Id == id));
                            db.Visit.RemoveRange(db.Visit.Where(v => v.Ticket_id == id));
                            db.Ticket.Remove(ticket);
                            db.SaveChanges();
                            Console.WriteLine("Билет и связанные данные удалены.");
                        }
                    }
                }

                if (choice != "0")
                {
                    Console.WriteLine("Нажмите любую клавишу...");
                    Console.ReadKey();
                }
            }
        }

        static void PaymentsMenu(ApplicationContext db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ПЛАТЕЖИ ===");
                Console.WriteLine("1. Показать");
                Console.WriteLine("2. Добавить");
                Console.WriteLine("3. Изменить");
                Console.WriteLine("4. Удалить");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "0") break;

                if (choice == "1")
                {
                    var payments = db.Payment.Include(p => p.Ticket).ToList();
                    Console.WriteLine("\nСписок платежей:");
                    foreach (var p in payments)
                        Console.WriteLine($"{p.Id} | Билет #{p.Ticket_Id} | {p.Amount} руб. | {p.Payment_Date}");
                }
                else if (choice == "2")
                {
                    Console.Write("ID билета: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int ticketId) || db.Ticket.Find(ticketId) == null)
                    {
                        Console.WriteLine("Билет не найден.");
                    }
                    else
                    {
                        Console.Write("Сумма: ");
                        string amountStr = Console.ReadLine() ?? "";
                        if (!decimal.TryParse(amountStr, out decimal amount))
                        {
                            Console.WriteLine("Некорректная сумма.");
                        }
                        else
                        {
                            Console.Write("Дата (ГГГГ-ММ-ДД): ");
                            string dateStr = Console.ReadLine() ?? "";
                            if (!DateTime.TryParse(dateStr, out DateTime date))
                                date = DateTime.Now;

                            db.Payment.Add(new Payment
                            {
                                Ticket_Id = ticketId,
                                Amount = amount,
                                Payment_Date = date
                            });
                            db.SaveChanges();
                            Console.WriteLine("Платёж добавлен.");
                        }
                    }
                }
                else if (choice == "3")
                {
                    Console.Write("ID платежа: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var payment = db.Payment.Find(id);
                        if (payment == null)
                        {
                            Console.WriteLine("Платёж не найден.");
                        }
                        else
                        {
                            Console.Write($"Сумма [{payment.Amount}]: ");
                            string amountStr = Console.ReadLine() ?? "";
                            if (decimal.TryParse(amountStr, out decimal amount)) payment.Amount = amount;

                            Console.Write($"Дата [{payment.Payment_Date}]: ");
                            string dateStr = Console.ReadLine() ?? "";
                            if (DateTime.TryParse(dateStr, out DateTime date)) payment.Payment_Date = date;

                            db.SaveChanges();
                            Console.WriteLine("Обновлено.");
                        }
                    }
                }
                else if (choice == "4")
                {
                    Console.Write("ID платежа: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var payment = db.Payment.Find(id);
                        if (payment == null)
                        {
                            Console.WriteLine("Платёж не найден.");
                        }
                        else
                        {
                            db.Payment.Remove(payment);
                            db.SaveChanges();
                            Console.WriteLine("Удалён.");
                        }
                    }
                }

                if (choice != "0")
                {
                    Console.WriteLine("Нажмите любую клавишу...");
                    Console.ReadKey();
                }
            }
        }

        static void VisitsMenu(ApplicationContext db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ПОСЕЩЕНИЯ ПАРКА ===");
                Console.WriteLine("1. Показать");
                Console.WriteLine("2. Добавить");
                Console.WriteLine("3. Изменить");
                Console.WriteLine("4. Удалить");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "0") break;

                if (choice == "1")
                {
                    var visits = db.Visit.Include(v => v.Ticket).ToList();
                    Console.WriteLine("\nСписок посещений:");
                    foreach (var v in visits)
                        Console.WriteLine($"{v.Id} | Билет #{v.Ticket_id} | Вход: {v.Entry_Time} | Выход: {v.Exit_Time}");
                }
                else if (choice == "2")
                {
                    Console.Write("ID билета: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int ticketId) || db.Ticket.Find(ticketId) == null)
                    {
                        Console.WriteLine("Билет не найден.");
                    }
                    else
                    {
                        Console.Write("Время входа: ");
                        string entryStr = Console.ReadLine() ?? "";
                        if (!DateTime.TryParse(entryStr, out DateTime entry))
                        {
                            Console.WriteLine("Некорректное время входа.");
                        }
                        else
                        {
                            Console.Write("Время выхода: ");
                            string exitStr = Console.ReadLine() ?? "";
                            if (!DateTime.TryParse(exitStr, out DateTime exit))
                            {
                                Console.WriteLine("Некорректное время выхода.");
                            }
                            else
                            {
                                db.Visit.Add(new Visit
                                {
                                    Ticket_id = ticketId,
                                    Entry_Time = entry,
                                    Exit_Time = exit
                                });
                                db.SaveChanges();
                                Console.WriteLine("Посещение добавлено.");
                            }
                        }
                    }
                }
                else if (choice == "3")
                {
                    Console.Write("ID посещения: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var visit = db.Visit.Find(id);
                        if (visit == null)
                        {
                            Console.WriteLine("Посещение не найдено.");
                        }
                        else
                        {
                            Console.Write($"Вход [{visit.Entry_Time}]: ");
                            string entryStr = Console.ReadLine() ?? "";
                            if (DateTime.TryParse(entryStr, out DateTime entry)) visit.Entry_Time = entry;

                            Console.Write($"Выход [{visit.Exit_Time}]: ");
                            string exitStr = Console.ReadLine() ?? "";
                            if (DateTime.TryParse(exitStr, out DateTime exit)) visit.Exit_Time = exit;

                            db.SaveChanges();
                            Console.WriteLine("Обновлено.");
                        }
                    }
                }
                else if (choice == "4")
                {
                    Console.Write("ID посещения: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var visit = db.Visit.Find(id);
                        if (visit == null)
                        {
                            Console.WriteLine("Посещение не найдено.");
                        }
                        else
                        {
                            db.Visit.Remove(visit);
                            db.SaveChanges();
                            Console.WriteLine("Удалено.");
                        }
                    }
                }

                if (choice != "0")
                {
                    Console.WriteLine("Нажмите любую клавишу...");
                    Console.ReadKey();
                }
            }
        }

        // ==================== АТТРАКЦИОНЫ ====================
        static void RideMenu(ApplicationContext db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== АТТРАКЦИОНЫ ===");
                Console.WriteLine("1. Аттракционы");
                Console.WriteLine("2. Группы");
                Console.WriteLine("3. Посещения аттракционов");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "0") break;
                if (choice == "1") RidesMenu(db);
                else if (choice == "2") RideGroupsMenu(db);
                else if (choice == "3") RideVisitsMenu(db);
            }
        }

        static void RidesMenu(ApplicationContext db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== АТТРАКЦИОНЫ ===");
                Console.WriteLine("1. Показать");
                Console.WriteLine("2. Добавить");
                Console.WriteLine("3. Изменить");
                Console.WriteLine("4. Удалить");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "0") break;

                if (choice == "1")
                {
                    var rides = db.Ride.Include(r => r.Group).ToList();
                    Console.WriteLine("\nСписок аттракционов:");
                    foreach (var r in rides)
                        Console.WriteLine($"{r.Id} | {r.Name} | Группа: {r.Group?.Name} | Мин. возраст: {r.Min_Age} | Длительность: {r.Duration_Minuts} мин");
                }
                else if (choice == "2")
                {
                    Console.Write("Название: ");
                    string name = (Console.ReadLine() ?? "").Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        Console.WriteLine("Название не может быть пустым.");
                    }
                    else
                    {
                        Console.Write("ID группы: ");
                        string groupInput = Console.ReadLine() ?? "";
                        if (!int.TryParse(groupInput, out int groupId) || db.RideGroup.Find(groupId) == null)
                        {
                            Console.WriteLine("Группа не найдена.");
                        }
                        else
                        {
                            Console.Write("Мин. возраст: ");
                            string ageInput = Console.ReadLine() ?? "";
                            if (!int.TryParse(ageInput, out int minAge))
                            {
                                Console.WriteLine("Некорректный возраст.");
                            }
                            else
                            {
                                Console.Write("Длительность (мин): ");
                                string durInput = Console.ReadLine() ?? "";
                                if (!int.TryParse(durInput, out int duration))
                                {
                                    Console.WriteLine("Некорректная длительность.");
                                }
                                else
                                {
                                    db.Ride.Add(new Ride
                                    {
                                        Name = name,
                                        Group_id = groupId,
                                        Min_Age = minAge,
                                        Duration_Minuts = duration
                                    });
                                    db.SaveChanges();
                                    Console.WriteLine("Аттракцион добавлен.");
                                }
                            }
                        }
                    }
                }
                else if (choice == "3")
                {
                    Console.Write("ID аттракциона: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var ride = db.Ride.Find(id);
                        if (ride == null)
                        {
                            Console.WriteLine("Аттракцион не найден.");
                        }
                        else
                        {
                            Console.Write($"Название [{ride.Name}]: ");
                            string name = (Console.ReadLine() ?? "").Trim();
                            if (!string.IsNullOrEmpty(name)) ride.Name = name;

                            Console.Write($"ID группы [{ride.Group_id}]: ");
                            string groupInput = Console.ReadLine() ?? "";
                            if (int.TryParse(groupInput, out int groupId) && db.RideGroup.Find(groupId) != null)
                                ride.Group_id = groupId;

                            Console.Write($"Мин. возраст [{ride.Min_Age}]: ");
                            string ageInput = Console.ReadLine() ?? "";
                            if (int.TryParse(ageInput, out int minAge)) ride.Min_Age = minAge;

                            Console.Write($"Длительность [{ride.Duration_Minuts}]: ");
                            string durInput = Console.ReadLine() ?? "";
                            if (int.TryParse(durInput, out int duration)) ride.Duration_Minuts = duration;

                            db.SaveChanges();
                            Console.WriteLine("Обновлено.");
                        }
                    }
                }
                else if (choice == "4")
                {
                    Console.Write("ID аттракциона: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var ride = db.Ride.Find(id);
                        if (ride == null)
                        {
                            Console.WriteLine("Аттракцион не найден.");
                        }
                        else if (db.RideVisit.Any(rv => rv.Ride_id == id))
                        {
                            Console.WriteLine("Есть посещения этого аттракциона.");
                        }
                        else
                        {
                            db.Ride.Remove(ride);
                            db.SaveChanges();
                            Console.WriteLine("Удалён.");
                        }
                    }
                }

                if (choice != "0")
                {
                    Console.WriteLine("Нажмите любую клавишу...");
                    Console.ReadKey();
                }
            }
        }

        static void RideGroupsMenu(ApplicationContext db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ГРУППЫ АТТРАКЦИОНОВ ===");
                Console.WriteLine("1. Показать");
                Console.WriteLine("2. Добавить");
                Console.WriteLine("3. Изменить");
                Console.WriteLine("4. Удалить");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "0") break;

                if (choice == "1")
                {
                    var groups = db.RideGroup.ToList();
                    Console.WriteLine("\nСписок групп:");
                    foreach (var g in groups)
                        Console.WriteLine($"{g.Id} | {g.Name}");
                }
                else if (choice == "2")
                {
                    Console.Write("Название: ");
                    string name = (Console.ReadLine() ?? "").Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        Console.WriteLine("Название не может быть пустым.");
                    }
                    else
                    {
                        db.RideGroup.Add(new RideGroup { Name = name });
                        db.SaveChanges();
                        Console.WriteLine("Группа добавлена.");
                    }
                }
                else if (choice == "3")
                {
                    Console.Write("ID группы: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var group = db.RideGroup.Find(id);
                        if (group == null)
                        {
                            Console.WriteLine("Группа не найдена.");
                        }
                        else
                        {
                            Console.Write($"Новое название [{group.Name}]: ");
                            string name = (Console.ReadLine() ?? "").Trim();
                            if (!string.IsNullOrEmpty(name)) group.Name = name;
                            db.SaveChanges();
                            Console.WriteLine("Обновлено.");
                        }
                    }
                }
                else if (choice == "4")
                {
                    Console.Write("ID группы: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var group = db.RideGroup.Find(id);
                        if (group == null)
                        {
                            Console.WriteLine("Группа не найдена.");
                        }
                        else if (db.Ride.Any(r => r.Group_id == id))
                        {
                            Console.WriteLine("В группе есть аттракционы.");
                        }
                        else
                        {
                            db.RideGroup.Remove(group);
                            db.SaveChanges();
                            Console.WriteLine("Удалена.");
                        }
                    }
                }

                if (choice != "0")
                {
                    Console.WriteLine("Нажмите любую клавишу...");
                    Console.ReadKey();
                }
            }
        }

        static void RideVisitsMenu(ApplicationContext db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ПОСЕЩЕНИЯ АТТРАКЦИОНОВ ===");
                Console.WriteLine("1. Показать");
                Console.WriteLine("2. Добавить");
                Console.WriteLine("3. Изменить");
                Console.WriteLine("4. Удалить");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "0") break;

                if (choice == "1")
                {
                    var rideVisits = db.RideVisit.Include(rv => rv.Visit).Include(rv => rv.Ride).ToList();
                    Console.WriteLine("\nСписок посещений аттракционов:");
                    foreach (var rv in rideVisits)
                        Console.WriteLine($"{rv.Id} | Посещение #{rv.Visit_Id} | Аттракцион: {rv.Ride?.Name} | Доступ: {rv.Access_Time} | Выход: {rv.Exit_Time}");
                }
                else if (choice == "2")
                {
                    Console.Write("ID посещения парка: ");
                    string visitInput = Console.ReadLine() ?? "";
                    if (!int.TryParse(visitInput, out int visitId) || db.Visit.Find(visitId) == null)
                    {
                        Console.WriteLine("Посещение не найдено.");
                    }
                    else
                    {
                        Console.Write("ID аттракциона: ");
                        string rideInput = Console.ReadLine() ?? "";
                        if (!int.TryParse(rideInput, out int rideId) || db.Ride.Find(rideId) == null)
                        {
                            Console.WriteLine("Аттракцион не найден.");
                        }
                        else
                        {
                            Console.Write("Время доступа: ");
                            string accessStr = Console.ReadLine() ?? "";
                            if (!DateTime.TryParse(accessStr, out DateTime access))
                            {
                                Console.WriteLine("Некорректное время доступа.");
                            }
                            else
                            {
                                Console.Write("Время выхода: ");
                                string exitStr = Console.ReadLine() ?? "";
                                if (!DateTime.TryParse(exitStr, out DateTime exit))
                                {
                                    Console.WriteLine("Некорректное время выхода.");
                                }
                                else
                                {
                                    db.RideVisit.Add(new RideVisit
                                    {
                                        Visit_Id = visitId,
                                        Ride_id = rideId,
                                        Access_Time = access,
                                        Exit_Time = exit
                                    });
                                    db.SaveChanges();
                                    Console.WriteLine("Посещение аттракциона добавлено.");
                                }
                            }
                        }
                    }
                }
                else if (choice == "3")
                {
                    Console.Write("ID записи: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var rv = db.RideVisit.Find(id);
                        if (rv == null)
                        {
                            Console.WriteLine("Запись не найдена.");
                        }
                        else
                        {
                            Console.Write($"ID посещения [{rv.Visit_Id}]: ");
                            string visitInput = Console.ReadLine() ?? "";
                            if (int.TryParse(visitInput, out int visitId) && db.Visit.Find(visitId) != null)
                                rv.Visit_Id = visitId;

                            Console.Write($"ID аттракциона [{rv.Ride_id}]: ");
                            string rideInput = Console.ReadLine() ?? "";
                            if (int.TryParse(rideInput, out int rideId) && db.Ride.Find(rideId) != null)
                                rv.Ride_id = rideId;

                            Console.Write($"Доступ [{rv.Access_Time}]: ");
                            string accessStr = Console.ReadLine() ?? "";
                            if (DateTime.TryParse(accessStr, out DateTime access)) rv.Access_Time = access;

                            Console.Write($"Выход [{rv.Exit_Time}]: ");
                            string exitStr = Console.ReadLine() ?? "";
                            if (DateTime.TryParse(exitStr, out DateTime exit)) rv.Exit_Time = exit;

                            db.SaveChanges();
                            Console.WriteLine("Обновлено.");
                        }
                    }
                }
                else if (choice == "4")
                {
                    Console.Write("ID записи: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var rv = db.RideVisit.Find(id);
                        if (rv == null)
                        {
                            Console.WriteLine("Запись не найдена.");
                        }
                        else
                        {
                            db.RideVisit.Remove(rv);
                            db.SaveChanges();
                            Console.WriteLine("Удалена.");
                        }
                    }
                }

                if (choice != "0")
                {
                    Console.WriteLine("Нажмите любую клавишу...");
                    Console.ReadKey();
                }
            }
        }

        // ==================== ТАРИФЫ ====================
        static void TariffMenu(ApplicationContext db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ТАРИФЫ ===");
                Console.WriteLine("1. Цены");
                Console.WriteLine("2. Категории");
                Console.WriteLine("3. Статусы");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "0") break;
                if (choice == "1") PricesMenu(db);
                else if (choice == "2") CategoriesMenu(db);
                else if (choice == "3") StatusesMenu(db);
            }
        }

        static void PricesMenu(ApplicationContext db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ЦЕНЫ ===");
                Console.WriteLine("1. Показать");
                Console.WriteLine("2. Добавить");
                Console.WriteLine("3. Изменить");
                Console.WriteLine("4. Удалить");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "0") break;

                if (choice == "1")
                {
                    var prices = db.Price.Include(p => p.Status).Include(p => p.Category).ToList();
                    Console.WriteLine("\nСписок цен:");
                    foreach (var p in prices)
                        Console.WriteLine($"{p.Id} | Статус: {p.Status?.Name} | Категория: {p.Category?.Name} | {p.Amount} руб.");
                }
                else if (choice == "2")
                {
                    Console.Write("ID статуса: ");
                    string statusInput = Console.ReadLine() ?? "";
                    if (!int.TryParse(statusInput, out int statusId) || db.Status.Find(statusId) == null)
                    {
                        Console.WriteLine("Статус не найден.");
                    }
                    else
                    {
                        Console.Write("ID категории: ");
                        string catInput = Console.ReadLine() ?? "";
                        if (!int.TryParse(catInput, out int catId) || db.Category.Find(catId) == null)
                        {
                            Console.WriteLine("Категория не найдена.");
                        }
                        else
                        {
                            Console.Write("Сумма: ");
                            string amountStr = Console.ReadLine() ?? "";
                            if (!decimal.TryParse(amountStr, out decimal amount))
                            {
                                Console.WriteLine("Некорректная сумма.");
                            }
                            else
                            {
                                db.Price.Add(new Price
                                {
                                    Status_Id = statusId,
                                    Category_Id = catId,
                                    Amount = amount
                                });
                                db.SaveChanges();
                                Console.WriteLine("Цена добавлена.");
                            }
                        }
                    }
                }
                else if (choice == "3")
                {
                    Console.Write("ID цены: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var price = db.Price.Find(id);
                        if (price == null)
                        {
                            Console.WriteLine("Цена не найдена.");
                        }
                        else
                        {
                            Console.Write($"ID статуса [{price.Status_Id}]: ");
                            string statusInput = Console.ReadLine() ?? "";
                            if (int.TryParse(statusInput, out int statusId) && db.Status.Find(statusId) != null)
                                price.Status_Id = statusId;

                            Console.Write($"ID категории [{price.Category_Id}]: ");
                            string catInput = Console.ReadLine() ?? "";
                            if (int.TryParse(catInput, out int catId) && db.Category.Find(catId) != null)
                                price.Category_Id = catId;

                            Console.Write($"Сумма [{price.Amount}]: ");
                            string amountStr = Console.ReadLine() ?? "";
                            if (decimal.TryParse(amountStr, out decimal amount))
                                price.Amount = amount;

                            db.SaveChanges();
                            Console.WriteLine("Обновлено.");
                        }
                    }
                }
                else if (choice == "4")
                {
                    Console.Write("ID цены: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var price = db.Price.Find(id);
                        if (price == null)
                        {
                            Console.WriteLine("Цена не найдена.");
                        }
                        else if (db.Ticket.Any(t => t.Price_id == id))
                        {
                            Console.WriteLine("Есть билеты с этой ценой.");
                        }
                        else
                        {
                            db.Price.Remove(price);
                            db.SaveChanges();
                            Console.WriteLine("Удалена.");
                        }
                    }
                }

                if (choice != "0")
                {
                    Console.WriteLine("Нажмите любую клавишу...");
                    Console.ReadKey();
                }
            }
        }

        static void CategoriesMenu(ApplicationContext db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== КАТЕГОРИИ ===");
                Console.WriteLine("1. Показать");
                Console.WriteLine("2. Добавить");
                Console.WriteLine("3. Изменить");
                Console.WriteLine("4. Удалить");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "0") break;

                if (choice == "1")
                {
                    var categories = db.Category.ToList();
                    Console.WriteLine("\nСписок категорий:");
                    foreach (var c in categories)
                        Console.WriteLine($"{c.Id} | {c.Name}");
                }
                else if (choice == "2")
                {
                    Console.Write("Название: ");
                    string name = (Console.ReadLine() ?? "").Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        Console.WriteLine("Название не может быть пустым.");
                    }
                    else
                    {
                        db.Category.Add(new Category { Name = name });
                        db.SaveChanges();
                        Console.WriteLine("Категория добавлена.");
                    }
                }
                else if (choice == "3")
                {
                    Console.Write("ID категории: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var category = db.Category.Find(id);
                        if (category == null)
                        {
                            Console.WriteLine("Категория не найдена.");
                        }
                        else
                        {
                            Console.Write($"Новое название [{category.Name}]: ");
                            string name = (Console.ReadLine() ?? "").Trim();
                            if (!string.IsNullOrEmpty(name)) category.Name = name;
                            db.SaveChanges();
                            Console.WriteLine("Обновлено.");
                        }
                    }
                }
                else if (choice == "4")
                {
                    Console.Write("ID категории: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var category = db.Category.Find(id);
                        if (category == null)
                        {
                            Console.WriteLine("Категория не найдена.");
                        }
                        else if (db.Price.Any(p => p.Category_Id == id))
                        {
                            Console.WriteLine("Есть цены с этой категорией.");
                        }
                        else
                        {
                            db.Category.Remove(category);
                            db.SaveChanges();
                            Console.WriteLine("Удалена.");
                        }
                    }
                }

                if (choice != "0")
                {
                    Console.WriteLine("Нажмите любую клавишу...");
                    Console.ReadKey();
                }
            }
        }

        static void StatusesMenu(ApplicationContext db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== СТАТУСЫ ===");
                Console.WriteLine("1. Показать");
                Console.WriteLine("2. Добавить");
                Console.WriteLine("3. Изменить");
                Console.WriteLine("4. Удалить");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "0") break;

                if (choice == "1")
                {
                    var statuses = db.Status.ToList();
                    Console.WriteLine("\nСписок статусов:");
                    foreach (var s in statuses)
                        Console.WriteLine($"{s.Id} | {s.Name}");
                }
                else if (choice == "2")
                {
                    Console.Write("Название: ");
                    string name = (Console.ReadLine() ?? "").Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        Console.WriteLine("Название не может быть пустым.");
                    }
                    else
                    {
                        db.Status.Add(new Status { Name = name });
                        db.SaveChanges();
                        Console.WriteLine("Статус добавлен.");
                    }
                }
                else if (choice == "3")
                {
                    Console.Write("ID статуса: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var status = db.Status.Find(id);
                        if (status == null)
                        {
                            Console.WriteLine("Статус не найден.");
                        }
                        else
                        {
                            Console.Write($"Новое название [{status.Name}]: ");
                            string name = (Console.ReadLine() ?? "").Trim();
                            if (!string.IsNullOrEmpty(name)) status.Name = name;
                            db.SaveChanges();
                            Console.WriteLine("Обновлено.");
                        }
                    }
                }
                else if (choice == "4")
                {
                    Console.Write("ID статуса: ");
                    string input = Console.ReadLine() ?? "";
                    if (!int.TryParse(input, out int id))
                    {
                        Console.WriteLine("Некорректный ID.");
                    }
                    else
                    {
                        var status = db.Status.Find(id);
                        if (status == null)
                        {
                            Console.WriteLine("Статус не найден.");
                        }
                        else if (db.Price.Any(p => p.Status_Id == id))
                        {
                            Console.WriteLine("Есть цены с этим статусом.");
                        }
                        else
                        {
                            db.Status.Remove(status);
                            db.SaveChanges();
                            Console.WriteLine("Удалён.");
                        }
                    }
                }

                if (choice != "0")
                {
                    Console.WriteLine("Нажмите любую клавишу...");
                    Console.ReadKey();
                }
            }
        }
    }
}