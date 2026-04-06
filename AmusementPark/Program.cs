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
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseSqlite("Filename=../../../AmusementPark.db")
                .Options;

            using var db = new ApplicationContext(options);
            db.Database.EnsureCreated();

            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("=== СИСТЕМА УПРАВЛЕНИЯ ПАРКОМ АТТРАКЦИОНОВ ===");
                Console.WriteLine("1. Добавить роль");
                Console.WriteLine("2. Добавить пользователя");
                Console.WriteLine("3. Добавить категорию билета");
                Console.WriteLine("4. Добавить статус билета");
                Console.WriteLine("5. Добавить цену");
                Console.WriteLine("6. Добавить профиль");
                Console.WriteLine("7. Добавить аттракцион");
                Console.WriteLine("8. Продать билет");
                Console.WriteLine("9. Зарегистрировать посещение");
                Console.WriteLine("10. Показать всех пользователей");
                Console.WriteLine("11. Показать все билеты");
                Console.WriteLine("12. Показать статистику");
                Console.WriteLine("0. Выход");
                Console.Write("\nВыберите действие: ");

                string? choice = Console.ReadLine();
                Console.Clear();

                switch (choice)
                {
                    case "1": AddRole(db); break;
                    case "2": AddUser(db); break;
                    case "3": AddCategory(db); break;
                    case "4": AddStatus(db); break;
                    case "5": AddPrice(db); break;
                    case "6": AddProfile(db); break;
                    case "7": AddRide(db); break;
                    case "8": SellTicket(db); break;
                    case "9": RegisterVisit(db); break;
                    case "10": ShowAllUsers(db); break;
                    case "11": ShowAllTickets(db); break;
                    case "12": ShowStatistics(db); break;
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

            var role = db.Role.Find(roleId);
            if (role == null)
            {
                Console.WriteLine("Роль с таким ID не найдена.");
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

        static void AddPrice(ApplicationContext db)
        {
            Console.Write("ID статуса: ");
            if (!int.TryParse(Console.ReadLine(), out int statusId))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            Console.Write("ID категории: ");
            if (!int.TryParse(Console.ReadLine(), out int catId))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }
            Console.Write("Сумма: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }

            if (db.Status.Find(statusId) == null)
            {
                Console.WriteLine("Статус не найден.");
                return;
            }
            if (db.Category.Find(catId) == null)
            {
                Console.WriteLine("Категория не найдена.");
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

        static void AddRide(ApplicationContext db)
        {
            Console.Write("Название аттракциона: ");
            string? name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Название не может быть пустым.");
                return;
            }

            Console.Write("ID группы аттракционов: ");
            if (!int.TryParse(Console.ReadLine(), out int groupId))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }

            // Проверка существования группы
            if (db.RideGroup.Find(groupId) == null)
            {
                Console.WriteLine("Группа аттракционов не найдена.");
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

        static void SellTicket(ApplicationContext db)
        {
            Console.Write("ID профиля: ");
            if (!int.TryParse(Console.ReadLine(), out int profileId))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }

            Console.Write("ID цены: ");
            if (!int.TryParse(Console.ReadLine(), out int priceId))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }

            var profile = db.Profile.Find(profileId);
            if (profile == null)
            {
                Console.WriteLine("Профиль не найден.");
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

            db.Payment.Add(new Payment
            {
                Ticket_Id = ticket.Id,
                Amount = price.Amount,
                Payment_Date = DateTime.Now
            });
            db.SaveChanges();

            Console.WriteLine($"Билет #{ticket.Id} продан за {price.Amount} руб.");
        }

        static void RegisterVisit(ApplicationContext db)
        {
            Console.Write("ID билета: ");
            if (!int.TryParse(Console.ReadLine(), out int ticketId))
            {
                Console.WriteLine("Ошибка: неверный ID билета.");
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

            var ticket = db.Ticket.Find(ticketId);
            if (ticket == null)
            {
                Console.WriteLine("Билет не найден.");
                return;
            }

            db.Visit.Add(new Visit
            {
                Ticket_id = ticket.Id,
                Entry_Time = entry,
                Exit_Time = exit
            });
            db.SaveChanges();
            Console.WriteLine("Посещение зарегистрировано.");
        }

        static void ShowAllUsers(ApplicationContext db)
        {
            var users = db.Users.Include(u => u.Role).ToList();
            Console.WriteLine("=== ПОЛЬЗОВАТЕЛИ ===");
            foreach (var u in users)
                Console.WriteLine($"{u.Id} | {u.Email} | {u.Role?.Name} | {u.Date_Created}");
        }

        static void ShowAllTickets(ApplicationContext db)
        {
            var tickets = db.Ticket
                .Include(t => t.Profile)
                .Include(t => t.Price)
                .Include(t => t.Payment)
                .ToList();

            Console.WriteLine("=== БИЛЕТЫ ===");
            foreach (var t in tickets)
            {
                Console.WriteLine($"ID: {t.Id}");
                Console.WriteLine($"  Владелец: {t.Profile?.First_Name} {t.Profile?.Last_Name}");
                Console.WriteLine($"  Цена: {t.Price?.Amount} руб.");
                Console.WriteLine($"  Период: {t.Start_Time} - {t.End_Time}");
                Console.WriteLine($"  Оплачен: {t.Payment?.Payment_Date}");
                Console.WriteLine();
            }
        }

        static void ShowStatistics(ApplicationContext db)
        {
            Console.WriteLine("=== СТАТИСТИКА ===");
            Console.WriteLine($"Продано билетов: {db.Ticket.Count()}");
            Console.WriteLine($"Выручка: {db.Payment.Sum(p => p.Amount)} руб.");
            Console.WriteLine($"Посещений: {db.Visit.Count()}");

            var popular = db.RideVisit
                .GroupBy(rv => rv.Ride_id)
                .Select(g => new { RideId = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .FirstOrDefault();

            if (popular != null)
            {
                var ride = db.Ride.Find(popular.RideId);
                Console.WriteLine($"Популярный аттракцион: {ride?.Name} ({popular.Count} раз)");
            }
        }
    }
}