using AmusementPark.Таблицы;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace AmusementPark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Настройка подключения к PostgreSQL
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseNpgsql("Host=localhost;Port=5433;Database=AmusementParkDB;Username=postgres;Password=admin")
                .Options;

            // Создаём контекст БЕЗ using, чтобы он жил всё время работы программы
            ApplicationContext db;
            try
            {
                db = new ApplicationContext(options);

                // Для чистого старта: удаляем старую БД и создаём новую (Id начнутся с 1)
                // Если не хотите терять данные при каждом запуске, закомментируйте следующую строку
                db.Database.EnsureDeleted();

                // Создаём БД и все таблицы, если их нет
                db.Database.EnsureCreated();

                Console.WriteLine("База данных PostgreSQL готова к работе.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения к PostgreSQL: {ex.Message}");
                Console.WriteLine("Проверьте, запущен ли сервер PostgreSQL и правильность пароля.");
                Console.WriteLine("Нажмите любую клавишу для выхода...");
                Console.ReadKey();
                return;
            }

            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("=== СИСТЕМА УПРАВЛЕНИЯ ПАРКОМ АТТРАКЦИОНОВ ===");
                Console.WriteLine("1. Аккаунт (пользователи, профили, роли)");
                Console.WriteLine("2. Билеты и посещения (билеты, платежи, посещения)");
                Console.WriteLine("3. Аттракционы (аттракционы, группы, посещения аттракционов)");
                Console.WriteLine("4. Тарифы (цены, категории, статусы)");
                Console.WriteLine("0. Выход");
                Console.Write("\nВыберите раздел: ");

                string? choice = Console.ReadLine();
                Console.Clear();

                switch (choice)
                {
                    case "1": AccountMenu(db); break;
                    case "2": TicketMenu(db); break;
                    case "3": RideMenu(db); break;
                    case "4": SettingsMenu(db); break;
                    case "0": exit = true; break;
                    default: Console.WriteLine("Неверный выбор!"); break;
                }

                if (!exit)
                {
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        #region Аккаунт
        static void AccountMenu(ApplicationContext db)
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("=== АККАУНТ ===");
                Console.WriteLine("1. Пользователи");
                Console.WriteLine("2. Профили");
                Console.WriteLine("3. Роли");
                Console.WriteLine("0. Назад");
                Console.Write("\nВыберите сущность: ");

                string? choice = Console.ReadLine();
                Console.Clear();

                switch (choice)
                {
                    case "1":
                        EntityMenu(db, "User",
                        () => ListUsers(db),
                        () => AddUser(db),
                        () => UpdateUser(db),
                        () => DeleteUser(db)); break;
                    case "2":
                        EntityMenu(db, "Profile",
                        () => ListProfiles(db),
                        () => AddProfile(db),
                        () => UpdateProfile(db),
                        () => DeleteProfile(db)); break;
                    case "3":
                        EntityMenu(db, "Role",
                        () => ListRoles(db),
                        () => AddRole(db),
                        () => UpdateRole(db),
                        () => DeleteRole(db)); break;
                    case "0": back = true; break;
                    default: Console.WriteLine("Неверный выбор!"); break;
                }
            }
        }

        static void EntityMenu(ApplicationContext db, string entityName, Action list, Action add, Action update, Action delete)
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine($"=== {entityName} ===");
                Console.WriteLine("1. Показать все");
                Console.WriteLine("2. Добавить");
                Console.WriteLine("3. Изменить");
                Console.WriteLine("4. Удалить");
                Console.WriteLine("0. Назад");
                Console.Write("\nВыберите действие: ");

                string? choice = Console.ReadLine();
                Console.Clear();

                switch (choice)
                {
                    case "1": list(); break;
                    case "2": add(); break;
                    case "3": update(); break;
                    case "4": delete(); break;
                    case "0": back = true; break;
                    default: Console.WriteLine("Неверный выбор!"); break;
                }

                if (choice != "0")
                {
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        // Роли
        static void ListRoles(ApplicationContext db)
        {
            var roles = db.Role.ToList();
            Console.WriteLine("=== РОЛИ ===");
            foreach (var r in roles)
                Console.WriteLine($"{r.Id} | {r.Name}");
        }

        static void AddRole(ApplicationContext db)
        {
            Console.Write("Название роли: ");
            string? name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Название не может быть пустым.");
                return;
            }
            var role = new Role { Name = name };
            db.Role.Add(role);
            db.SaveChanges();
            Console.WriteLine($"Роль добавлена. ID: {role.Id}");
        }

        static void UpdateRole(ApplicationContext db)
        {
            Console.Write("ID роли для изменения: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var role = db.Role.Find(id);
            if (role == null)
            {
                Console.WriteLine("Роль не найдена.");
                return;
            }
            Console.Write($"Новое название (было: {role.Name}): ");
            string? name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name))
                role.Name = name;
            db.SaveChanges();
            Console.WriteLine("Роль обновлена.");
        }

        static void DeleteRole(ApplicationContext db)
        {
            Console.Write("ID роли для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var role = db.Role.Find(id);
            if (role == null)
            {
                Console.WriteLine("Роль не найдена.");
                return;
            }
            // Проверка, есть ли пользователи с этой ролью
            if (db.Users.Any(u => u.RoleId == id))
            {
                Console.WriteLine("Нельзя удалить роль, так как есть пользователи с этой ролью.");
                return;
            }
            db.Role.Remove(role);
            db.SaveChanges();
            Console.WriteLine("Роль удалена.");
        }

        // Пользователи
        static void ListUsers(ApplicationContext db)
        {
            var users = db.Users.Include(u => u.Role).ToList();
            Console.WriteLine("=== ПОЛЬЗОВАТЕЛИ ===");
            foreach (var u in users)
                Console.WriteLine($"{u.Id} | {u.Email} | Роль: {u.Role?.Name} | Создан: {u.Date_Created}");
        }

        static void AddUser(ApplicationContext db)
        {
            Console.Write("Email: ");
            string? email = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("Email не может быть пустым.");
                return;
            }
            Console.Write("Пароль: ");
            string? password = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Пароль не может быть пустым.");
                return;
            }
            Console.Write("ID роли: ");
            if (!int.TryParse(Console.ReadLine(), out int roleId))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            if (db.Role.Find(roleId) == null)
            {
                Console.WriteLine("Роль не найдена.");
                return;
            }
            var user = new User
            {
                Email = email,
                Password = password,
                Date_Created = DateTime.Now,
                RoleId = roleId
            };
            db.Users.Add(user);
            db.SaveChanges();
            Console.WriteLine($"Пользователь добавлен. ID: {user.Id}");
        }

        static void UpdateUser(ApplicationContext db)
        {
            Console.Write("ID пользователя для изменения: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var user = db.Users.Find(id);
            if (user == null)
            {
                Console.WriteLine("Пользователь не найден.");
                return;
            }
            Console.Write($"Новый Email (было: {user.Email}): ");
            string? email = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(email))
                user.Email = email;
            Console.Write($"Новый пароль (было: {user.Password}): ");
            string? password = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(password))
                user.Password = password;
            Console.Write($"Новый ID роли (было: {user.RoleId}): ");
            if (int.TryParse(Console.ReadLine(), out int roleId) && db.Role.Find(roleId) != null)
                user.RoleId = roleId;
            db.SaveChanges();
            Console.WriteLine("Пользователь обновлён.");
        }

        static void DeleteUser(ApplicationContext db)
        {
            Console.Write("ID пользователя для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var user = db.Users.Find(id);
            if (user == null)
            {
                Console.WriteLine("Пользователь не найден.");
                return;
            }
            // Проверка связанного профиля
            if (db.Profile.Any(p => p.User_Id == id))
            {
                Console.WriteLine("У пользователя есть профиль. Сначала удалите профиль.");
                return;
            }
            db.Users.Remove(user);
            db.SaveChanges();
            Console.WriteLine("Пользователь удалён.");
        }

        // Профили
        static void ListProfiles(ApplicationContext db)
        {
            var profiles = db.Profile.Include(p => p.User).ToList();
            Console.WriteLine("=== ПРОФИЛИ ===");
            foreach (var p in profiles)
                Console.WriteLine($"{p.Id} | {p.First_Name} {p.Last_Name} | {p.Birth_Date} | {p.Phone} | Пользователь: {p.User?.Email}");
        }

        static void AddProfile(ApplicationContext db)
        {
            Console.Write("ID пользователя: ");
            if (!int.TryParse(Console.ReadLine(), out int userId))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            if (db.Users.Find(userId) == null)
            {
                Console.WriteLine("Пользователь не найден.");
                return;
            }
            Console.Write("Имя: ");
            string? firstName = Console.ReadLine() ?? "";
            Console.Write("Фамилия: ");
            string? lastName = Console.ReadLine() ?? "";
            Console.Write("Дата рождения (ГГГГ-ММ-ДД): ");
            if (!DateOnly.TryParse(Console.ReadLine(), out DateOnly birth))
            {
                Console.WriteLine("Ошибка: неверный формат даты.");
                return;
            }
            Console.Write("Телефон: ");
            string? phone = Console.ReadLine() ?? "";
            var profile = new Profile
            {
                User_Id = userId,
                First_Name = firstName,
                Last_Name = lastName,
                Birth_Date = birth,
                Phone = phone
            };
            db.Profile.Add(profile);
            db.SaveChanges();
            Console.WriteLine($"Профиль создан. ID: {profile.Id}");
        }

        static void UpdateProfile(ApplicationContext db)
        {
            Console.Write("ID профиля для изменения: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var profile = db.Profile.Find(id);
            if (profile == null)
            {
                Console.WriteLine("Профиль не найден.");
                return;
            }
            Console.Write($"Новое имя (было: {profile.First_Name}): ");
            string? fn = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(fn)) profile.First_Name = fn;
            Console.Write($"Новая фамилия (было: {profile.Last_Name}): ");
            string? ln = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(ln)) profile.Last_Name = ln;
            Console.Write($"Новая дата рождения (было: {profile.Birth_Date}): ");
            if (DateOnly.TryParse(Console.ReadLine(), out DateOnly birth)) profile.Birth_Date = birth;
            Console.Write($"Новый телефон (было: {profile.Phone}): ");
            string? phone = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(phone)) profile.Phone = phone;
            db.SaveChanges();
            Console.WriteLine("Профиль обновлён.");
        }

        static void DeleteProfile(ApplicationContext db)
        {
            Console.Write("ID профиля для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var profile = db.Profile.Find(id);
            if (profile == null)
            {
                Console.WriteLine("Профиль не найден.");
                return;
            }
            // Проверка связанных билетов
            if (db.Ticket.Any(t => t.Profile_id == id))
            {
                Console.WriteLine("У профиля есть билеты. Сначала удалите билеты.");
                return;
            }
            db.Profile.Remove(profile);
            db.SaveChanges();
            Console.WriteLine("Профиль удалён.");
        }
        #endregion

        #region Билеты и посещения
        static void TicketMenu(ApplicationContext db)
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("=== БИЛЕТЫ И ПОСЕЩЕНИЯ ===");
                Console.WriteLine("1. Билеты");
                Console.WriteLine("2. Платежи");
                Console.WriteLine("3. Посещения парка");
                Console.WriteLine("0. Назад");
                Console.Write("\nВыберите сущность: ");

                string? choice = Console.ReadLine();
                Console.Clear();

                switch (choice)
                {
                    case "1":
                        EntityMenu(db, "Ticket",
                        () => ListTickets(db),
                        () => AddTicket(db),
                        () => UpdateTicket(db),
                        () => DeleteTicket(db)); break;
                    case "2":
                        EntityMenu(db, "Payment",
                        () => ListPayments(db),
                        () => AddPayment(db),
                        () => UpdatePayment(db),
                        () => DeletePayment(db)); break;
                    case "3":
                        EntityMenu(db, "Visit",
                        () => ListVisits(db),
                        () => AddVisit(db),
                        () => UpdateVisit(db),
                        () => DeleteVisit(db)); break;
                    case "0": back = true; break;
                    default: Console.WriteLine("Неверный выбор!"); break;
                }
            }
        }

        // Билеты
        static void ListTickets(ApplicationContext db)
        {
            var tickets = db.Ticket.Include(t => t.Profile).Include(t => t.Price).Include(t => t.Payment).ToList();
            Console.WriteLine("=== БИЛЕТЫ ===");
            foreach (var t in tickets)
                Console.WriteLine($"ID: {t.Id} | Владелец: {t.Profile?.First_Name} {t.Profile?.Last_Name} | Цена: {t.Price?.Amount} руб. | Период: {t.Start_Time} - {t.End_Time} | Оплачен: {t.Payment?.Payment_Date}");
        }

        static void AddTicket(ApplicationContext db)
        {
            Console.Write("ID профиля: ");
            if (!int.TryParse(Console.ReadLine(), out int profileId))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            if (db.Profile.Find(profileId) == null)
            {
                Console.WriteLine("Профиль не найден.");
                return;
            }
            Console.Write("ID цены: ");
            if (!int.TryParse(Console.ReadLine(), out int priceId))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var price = db.Price.Find(priceId);
            if (price == null)
            {
                Console.WriteLine("Цена не найдена.");
                return;
            }
            Console.Write("Начало (ГГГГ-ММ-ДД ЧЧ:ММ): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime start))
            {
                Console.WriteLine("Ошибка: неверный формат даты.");
                return;
            }
            Console.Write("Конец (ГГГГ-ММ-ДД ЧЧ:ММ): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime end))
            {
                Console.WriteLine("Ошибка: неверный формат даты.");
                return;
            }
            var ticket = new Ticket
            {
                Profile_id = profileId,
                Price_id = priceId,
                Start_Time = start,
                End_Time = end
            };
            db.Ticket.Add(ticket);
            db.SaveChanges();

            // Автоматически создаём платёж
            db.Payment.Add(new Payment
            {
                Ticket_Id = ticket.Id,
                Amount = price.Amount,
                Payment_Date = DateTime.Now
            });
            db.SaveChanges();
            Console.WriteLine($"Билет #{ticket.Id} добавлен, платёж создан на {price.Amount} руб.");
        }

        static void UpdateTicket(ApplicationContext db)
        {
            Console.Write("ID билета для изменения: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var ticket = db.Ticket.Find(id);
            if (ticket == null)
            {
                Console.WriteLine("Билет не найден.");
                return;
            }
            Console.Write($"Новое начало (было: {ticket.Start_Time}): ");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime start))
                ticket.Start_Time = start;
            Console.Write($"Новый конец (было: {ticket.End_Time}): ");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime end))
                ticket.End_Time = end;
            // Можно изменить цену, но лучше не менять Price_id, т.к. платёж уже есть
            db.SaveChanges();
            Console.WriteLine("Билет обновлён.");
        }

        static void DeleteTicket(ApplicationContext db)
        {
            Console.Write("ID билета для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var ticket = db.Ticket.Find(id);
            if (ticket == null)
            {
                Console.WriteLine("Билет не найден.");
                return;
            }
            // Удаляем связанные платежи и посещения (каскадно вручную)
            var payments = db.Payment.Where(p => p.Ticket_Id == id);
            db.Payment.RemoveRange(payments);
            var visits = db.Visit.Where(v => v.Ticket_id == id);
            db.Visit.RemoveRange(visits);
            db.Ticket.Remove(ticket);
            db.SaveChanges();
            Console.WriteLine("Билет и связанные данные удалены.");
        }

        // Платежи
        static void ListPayments(ApplicationContext db)
        {
            var payments = db.Payment.Include(p => p.Ticket).ToList();
            Console.WriteLine("=== ПЛАТЕЖИ ===");
            foreach (var p in payments)
                Console.WriteLine($"ID: {p.Id} | Билет ID: {p.Ticket_Id} | Сумма: {p.Amount} | Дата: {p.Payment_Date}");
        }

        static void AddPayment(ApplicationContext db)
        {
            Console.Write("ID билета: ");
            if (!int.TryParse(Console.ReadLine(), out int ticketId))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            if (db.Ticket.Find(ticketId) == null)
            {
                Console.WriteLine("Билет не найден.");
                return;
            }
            Console.Write("Сумма: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            Console.Write("Дата платежа (ГГГГ-ММ-ДД): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime date))
                date = DateTime.Now;
            var payment = new Payment
            {
                Ticket_Id = ticketId,
                Amount = amount,
                Payment_Date = date
            };
            db.Payment.Add(payment);
            db.SaveChanges();
            Console.WriteLine("Платёж добавлен.");
        }

        static void UpdatePayment(ApplicationContext db)
        {
            Console.Write("ID платежа для изменения: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var payment = db.Payment.Find(id);
            if (payment == null)
            {
                Console.WriteLine("Платёж не найден.");
                return;
            }
            Console.Write($"Новая сумма (было: {payment.Amount}): ");
            if (decimal.TryParse(Console.ReadLine(), out decimal amount))
                payment.Amount = amount;
            Console.Write($"Новая дата (было: {payment.Payment_Date}): ");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime date))
                payment.Payment_Date = date;
            db.SaveChanges();
            Console.WriteLine("Платёж обновлён.");
        }

        static void DeletePayment(ApplicationContext db)
        {
            Console.Write("ID платежа для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var payment = db.Payment.Find(id);
            if (payment == null)
            {
                Console.WriteLine("Платёж не найден.");
                return;
            }
            db.Payment.Remove(payment);
            db.SaveChanges();
            Console.WriteLine("Платёж удалён.");
        }

        // Посещения парка (Visit)
        static void ListVisits(ApplicationContext db)
        {
            var visits = db.Visit.Include(v => v.Ticket).ToList();
            Console.WriteLine("=== ПОСЕЩЕНИЯ ПАРКА ===");
            foreach (var v in visits)
                Console.WriteLine($"ID: {v.Id} | Билет ID: {v.Ticket_id} | Вход: {v.Entry_Time} | Выход: {v.Exit_Time}");
        }

        static void AddVisit(ApplicationContext db)
        {
            Console.Write("ID билета: ");
            if (!int.TryParse(Console.ReadLine(), out int ticketId))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            if (db.Ticket.Find(ticketId) == null)
            {
                Console.WriteLine("Билет не найден.");
                return;
            }
            Console.Write("Время входа: ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime entry))
            {
                Console.WriteLine("Ошибка: неверный формат даты.");
                return;
            }
            Console.Write("Время выхода: ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime exit))
            {
                Console.WriteLine("Ошибка: неверный формат даты.");
                return;
            }
            db.Visit.Add(new Visit
            {
                Ticket_id = ticketId,
                Entry_Time = entry,
                Exit_Time = exit
            });
            db.SaveChanges();
            Console.WriteLine("Посещение добавлено.");
        }

        static void UpdateVisit(ApplicationContext db)
        {
            Console.Write("ID посещения для изменения: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var visit = db.Visit.Find(id);
            if (visit == null)
            {
                Console.WriteLine("Посещение не найдено.");
                return;
            }
            Console.Write($"Новое время входа (было: {visit.Entry_Time}): ");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime entry))
                visit.Entry_Time = entry;
            Console.Write($"Новое время выхода (было: {visit.Exit_Time}): ");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime exit))
                visit.Exit_Time = exit;
            db.SaveChanges();
            Console.WriteLine("Посещение обновлено.");
        }

        static void DeleteVisit(ApplicationContext db)
        {
            Console.Write("ID посещения для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var visit = db.Visit.Find(id);
            if (visit == null)
            {
                Console.WriteLine("Посещение не найдено.");
                return;
            }
            db.Visit.Remove(visit);
            db.SaveChanges();
            Console.WriteLine("Посещение удалено.");
        }
        #endregion

        #region Аттракционы
        static void RideMenu(ApplicationContext db)
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("=== АТТРАКЦИОНЫ ===");
                Console.WriteLine("1. Аттракционы");
                Console.WriteLine("2. Группы аттракционов");
                Console.WriteLine("3. Посещения аттракционов");
                Console.WriteLine("0. Назад");
                Console.Write("\nВыберите сущность: ");

                string? choice = Console.ReadLine();
                Console.Clear();

                switch (choice)
                {
                    case "1":
                        EntityMenu(db, "Ride",
                        () => ListRides(db),
                        () => AddRide(db),
                        () => UpdateRide(db),
                        () => DeleteRide(db)); break;
                    case "2":
                        EntityMenu(db, "RideGroup",
                        () => ListRideGroups(db),
                        () => AddRideGroup(db),
                        () => UpdateRideGroup(db),
                        () => DeleteRideGroup(db)); break;
                    case "3":
                        EntityMenu(db, "RideVisit",
                        () => ListRideVisits(db),
                        () => AddRideVisit(db),
                        () => UpdateRideVisit(db),
                        () => DeleteRideVisit(db)); break;
                    case "0": back = true; break;
                    default: Console.WriteLine("Неверный выбор!"); break;
                }
            }
        }

        // Аттракционы
        static void ListRides(ApplicationContext db)
        {
            var rides = db.Ride.Include(r => r.Group).ToList();
            Console.WriteLine("=== АТТРАКЦИОНЫ ===");
            foreach (var r in rides)
                Console.WriteLine($"{r.Id} | {r.Name} | Группа: {r.Group?.Name} | Мин. возраст: {r.Min_Age} | Длительность: {r.Duration_Minuts} мин");
        }

        static void AddRide(ApplicationContext db)
        {
            Console.Write("Название аттракциона: ");
            string? name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Название не может быть пустым.");
                return;
            }
            Console.Write("ID группы: ");
            if (!int.TryParse(Console.ReadLine(), out int groupId))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            if (db.RideGroup.Find(groupId) == null)
            {
                Console.WriteLine("Группа не найдена.");
                return;
            }
            Console.Write("Мин. возраст: ");
            if (!int.TryParse(Console.ReadLine(), out int minAge))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            Console.Write("Длительность (мин): ");
            if (!int.TryParse(Console.ReadLine(), out int duration))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var ride = new Ride
            {
                Name = name,
                Group_id = groupId,
                Min_Age = minAge,
                Duration_Minuts = duration
            };
            db.Ride.Add(ride);
            db.SaveChanges();
            Console.WriteLine($"Аттракцион добавлен. ID: {ride.Id}");
        }

        static void UpdateRide(ApplicationContext db)
        {
            Console.Write("ID аттракциона для изменения: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var ride = db.Ride.Find(id);
            if (ride == null)
            {
                Console.WriteLine("Аттракцион не найден.");
                return;
            }
            Console.Write($"Новое название (было: {ride.Name}): ");
            string? name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name)) ride.Name = name;
            Console.Write($"Новый ID группы (было: {ride.Group_id}): ");
            if (int.TryParse(Console.ReadLine(), out int groupId) && db.RideGroup.Find(groupId) != null)
                ride.Group_id = groupId;
            Console.Write($"Новый мин. возраст (было: {ride.Min_Age}): ");
            if (int.TryParse(Console.ReadLine(), out int minAge)) ride.Min_Age = minAge;
            Console.Write($"Новая длительность (было: {ride.Duration_Minuts}): ");
            if (int.TryParse(Console.ReadLine(), out int duration)) ride.Duration_Minuts = duration;
            db.SaveChanges();
            Console.WriteLine("Аттракцион обновлён.");
        }

        static void DeleteRide(ApplicationContext db)
        {
            Console.Write("ID аттракциона для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var ride = db.Ride.Find(id);
            if (ride == null)
            {
                Console.WriteLine("Аттракцион не найден.");
                return;
            }
            // Проверка связанных посещений
            if (db.RideVisit.Any(rv => rv.Ride_id == id))
            {
                Console.WriteLine("Есть посещения этого аттракциона. Сначала удалите их.");
                return;
            }
            db.Ride.Remove(ride);
            db.SaveChanges();
            Console.WriteLine("Аттракцион удалён.");
        }

        // Группы аттракционов
        static void ListRideGroups(ApplicationContext db)
        {
            var groups = db.RideGroup.ToList();
            Console.WriteLine("=== ГРУППЫ АТТРАКЦИОНОВ ===");
            foreach (var g in groups)
                Console.WriteLine($"{g.Id} | {g.Name}");
        }

        static void AddRideGroup(ApplicationContext db)
        {
            Console.Write("Название группы: ");
            string? name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Название не может быть пустым.");
                return;
            }
            var group = new RideGroup { Name = name };
            db.RideGroup.Add(group);
            db.SaveChanges();
            Console.WriteLine($"Группа добавлена. ID: {group.Id}");
        }

        static void UpdateRideGroup(ApplicationContext db)
        {
            Console.Write("ID группы для изменения: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var group = db.RideGroup.Find(id);
            if (group == null)
            {
                Console.WriteLine("Группа не найдена.");
                return;
            }
            Console.Write($"Новое название (было: {group.Name}): ");
            string? name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name)) group.Name = name;
            db.SaveChanges();
            Console.WriteLine("Группа обновлена.");
        }

        static void DeleteRideGroup(ApplicationContext db)
        {
            Console.Write("ID группы для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var group = db.RideGroup.Find(id);
            if (group == null)
            {
                Console.WriteLine("Группа не найдена.");
                return;
            }
            if (db.Ride.Any(r => r.Group_id == id))
            {
                Console.WriteLine("Есть аттракционы в этой группе. Сначала удалите их.");
                return;
            }
            db.RideGroup.Remove(group);
            db.SaveChanges();
            Console.WriteLine("Группа удалена.");
        }

        // Посещения аттракционов
        static void ListRideVisits(ApplicationContext db)
        {
            var rv = db.RideVisit.Include(r => r.Visit).Include(r => r.Ride).ToList();
            Console.WriteLine("=== ПОСЕЩЕНИЯ АТТРАКЦИОНОВ ===");
            foreach (var v in rv)
                Console.WriteLine($"ID: {v.Id} | Посещение ID: {v.Visit_Id} | Аттракцион: {v.Ride?.Name} | Доступ: {v.Access_Time} | Выход: {v.Exit_Time}");
        }

        static void AddRideVisit(ApplicationContext db)
        {
            Console.Write("ID посещения парка (Visit): ");
            if (!int.TryParse(Console.ReadLine(), out int visitId))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            if (db.Visit.Find(visitId) == null)
            {
                Console.WriteLine("Посещение не найдено.");
                return;
            }
            Console.Write("ID аттракциона: ");
            if (!int.TryParse(Console.ReadLine(), out int rideId))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            if (db.Ride.Find(rideId) == null)
            {
                Console.WriteLine("Аттракцион не найден.");
                return;
            }
            Console.Write("Время доступа: ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime access))
            {
                Console.WriteLine("Ошибка: неверный формат даты.");
                return;
            }
            Console.Write("Время выхода: ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime exit))
            {
                Console.WriteLine("Ошибка: неверный формат даты.");
                return;
            }
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

        static void UpdateRideVisit(ApplicationContext db)
        {
            Console.Write("ID записи для изменения: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var rv = db.RideVisit.Find(id);
            if (rv == null)
            {
                Console.WriteLine("Запись не найдена.");
                return;
            }
            Console.Write($"Новый ID посещения (было: {rv.Visit_Id}): ");
            if (int.TryParse(Console.ReadLine(), out int visitId) && db.Visit.Find(visitId) != null)
                rv.Visit_Id = visitId;
            Console.Write($"Новый ID аттракциона (было: {rv.Ride_id}): ");
            if (int.TryParse(Console.ReadLine(), out int rideId) && db.Ride.Find(rideId) != null)
                rv.Ride_id = rideId;
            Console.Write($"Новое время доступа (было: {rv.Access_Time}): ");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime access))
                rv.Access_Time = access;
            Console.Write($"Новое время выхода (было: {rv.Exit_Time}): ");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime exit))
                rv.Exit_Time = exit;
            db.SaveChanges();
            Console.WriteLine("Запись обновлена.");
        }

        static void DeleteRideVisit(ApplicationContext db)
        {
            Console.Write("ID записи для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var rv = db.RideVisit.Find(id);
            if (rv == null)
            {
                Console.WriteLine("Запись не найдена.");
                return;
            }
            db.RideVisit.Remove(rv);
            db.SaveChanges();
            Console.WriteLine("Запись удалена.");
        }
        #endregion

        #region Настройки и цены
        static void SettingsMenu(ApplicationContext db)
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("=== Тарифы ===");
                Console.WriteLine("1. Цены");
                Console.WriteLine("2. Категории");
                Console.WriteLine("3. Статусы");
                Console.WriteLine("0. Назад");
                Console.Write("\nВыберите сущность: ");

                string? choice = Console.ReadLine();
                Console.Clear();

                switch (choice)
                {
                    case "1":
                        EntityMenu(db, "Price",
                        () => ListPrices(db),
                        () => AddPrice(db),
                        () => UpdatePrice(db),
                        () => DeletePrice(db)); break;
                    case "2":
                        EntityMenu(db, "Category",
                        () => ListCategories(db),
                        () => AddCategory(db),
                        () => UpdateCategory(db),
                        () => DeleteCategory(db)); break;
                    case "3":
                        EntityMenu(db, "Status",
                        () => ListStatuses(db),
                        () => AddStatus(db),
                        () => UpdateStatus(db),
                        () => DeleteStatus(db)); break;
                    case "0": back = true; break;
                    default: Console.WriteLine("Неверный выбор!"); break;
                }
            }
        }

        // Цены
        static void ListPrices(ApplicationContext db)
        {
            var prices = db.Price.Include(p => p.Status).Include(p => p.Category).ToList();
            Console.WriteLine("=== ЦЕНЫ ===");
            foreach (var p in prices)
                Console.WriteLine($"{p.Id} | Статус: {p.Status?.Name} | Категория: {p.Category?.Name} | Сумма: {p.Amount}");
        }

        static void AddPrice(ApplicationContext db)
        {
            Console.Write("ID статуса: ");
            if (!int.TryParse(Console.ReadLine(), out int statusId))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            if (db.Status.Find(statusId) == null)
            {
                Console.WriteLine("Статус не найден.");
                return;
            }
            Console.Write("ID категории: ");
            if (!int.TryParse(Console.ReadLine(), out int catId))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            if (db.Category.Find(catId) == null)
            {
                Console.WriteLine("Категория не найдена.");
                return;
            }
            Console.Write("Сумма: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var price = new Price
            {
                Status_Id = statusId,
                Category_Id = catId,
                Amount = amount
            };
            db.Price.Add(price);
            db.SaveChanges();
            Console.WriteLine($"Цена добавлена. ID: {price.Id}");
        }

        static void UpdatePrice(ApplicationContext db)
        {
            Console.Write("ID цены для изменения: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var price = db.Price.Find(id);
            if (price == null)
            {
                Console.WriteLine("Цена не найдена.");
                return;
            }
            Console.Write($"Новый ID статуса (было: {price.Status_Id}): ");
            if (int.TryParse(Console.ReadLine(), out int statusId) && db.Status.Find(statusId) != null)
                price.Status_Id = statusId;
            Console.Write($"Новый ID категории (было: {price.Category_Id}): ");
            if (int.TryParse(Console.ReadLine(), out int catId) && db.Category.Find(catId) != null)
                price.Category_Id = catId;
            Console.Write($"Новая сумма (было: {price.Amount}): ");
            if (decimal.TryParse(Console.ReadLine(), out decimal amount))
                price.Amount = amount;
            db.SaveChanges();
            Console.WriteLine("Цена обновлена.");
        }

        static void DeletePrice(ApplicationContext db)
        {
            Console.Write("ID цены для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var price = db.Price.Find(id);
            if (price == null)
            {
                Console.WriteLine("Цена не найдена.");
                return;
            }
            if (db.Ticket.Any(t => t.Price_id == id))
            {
                Console.WriteLine("Есть билеты с этой ценой. Сначала удалите билеты.");
                return;
            }
            db.Price.Remove(price);
            db.SaveChanges();
            Console.WriteLine("Цена удалена.");
        }

        // Категории
        static void ListCategories(ApplicationContext db)
        {
            var cats = db.Category.ToList();
            Console.WriteLine("=== КАТЕГОРИИ ===");
            foreach (var c in cats)
                Console.WriteLine($"{c.Id} | {c.Name}");
        }

        static void AddCategory(ApplicationContext db)
        {
            Console.Write("Название категории: ");
            string? name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Название не может быть пустым.");
                return;
            }
            var cat = new Category { Name = name };
            db.Category.Add(cat);
            db.SaveChanges();
            Console.WriteLine($"Категория добавлена. ID: {cat.Id}");
        }

        static void UpdateCategory(ApplicationContext db)
        {
            Console.Write("ID категории для изменения: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var cat = db.Category.Find(id);
            if (cat == null)
            {
                Console.WriteLine("Категория не найдена.");
                return;
            }
            Console.Write($"Новое название (было: {cat.Name}): ");
            string? name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name)) cat.Name = name;
            db.SaveChanges();
            Console.WriteLine("Категория обновлена.");
        }

        static void DeleteCategory(ApplicationContext db)
        {
            Console.Write("ID категории для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var cat = db.Category.Find(id);
            if (cat == null)
            {
                Console.WriteLine("Категория не найдена.");
                return;
            }
            if (db.Price.Any(p => p.Category_Id == id))
            {
                Console.WriteLine("Есть цены с этой категорией. Сначала удалите их.");
                return;
            }
            db.Category.Remove(cat);
            db.SaveChanges();
            Console.WriteLine("Категория удалена.");
        }

        // Статусы
        static void ListStatuses(ApplicationContext db)
        {
            var statuses = db.Status.ToList();
            Console.WriteLine("=== СТАТУСЫ ===");
            foreach (var s in statuses)
                Console.WriteLine($"{s.Id} | {s.Name}");
        }

        static void AddStatus(ApplicationContext db)
        {
            Console.Write("Название статуса: ");
            string? name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Название не может быть пустым.");
                return;
            }
            var status = new Status { Name = name };
            db.Status.Add(status);
            db.SaveChanges();
            Console.WriteLine($"Статус добавлен. ID: {status.Id}");
        }

        static void UpdateStatus(ApplicationContext db)
        {
            Console.Write("ID статуса для изменения: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var status = db.Status.Find(id);
            if (status == null)
            {
                Console.WriteLine("Статус не найден.");
                return;
            }
            Console.Write($"Новое название (было: {status.Name}): ");
            string? name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name)) status.Name = name;
            db.SaveChanges();
            Console.WriteLine("Статус обновлён.");
        }

        static void DeleteStatus(ApplicationContext db)
        {
            Console.Write("ID статуса для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            var status = db.Status.Find(id);
            if (status == null)
            {
                Console.WriteLine("Статус не найден.");
                return;
            }
            if (db.Price.Any(p => p.Status_Id == id))
            {
                Console.WriteLine("Есть цены с этим статусом. Сначала удалите их.");
                return;
            }
            db.Status.Remove(status);
            db.SaveChanges();
            Console.WriteLine("Статус удалён.");
        }
        #endregion
    }
}