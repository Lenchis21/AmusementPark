using AmusementPark.Таблицы;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmusementPark.Service
{
    public class DatabaseService
    {
        private readonly IDbContextFactory<ApplicationContext> _contextFactory;

        public DatabaseService(IDbContextFactory<ApplicationContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // Для операций, которые изменяют данные, всегда создаём новый контекст
        private ApplicationContext CreateContext() => _contextFactory.CreateDbContext();

        // ========== Роли ==========
        public List<Role> GetRoles()
        {
            using var db = CreateContext();
            return db.Role.ToList();
        }

        public Role? GetRole(int id)
        {
            using var db = CreateContext();
            return db.Role.Find(id);
        }

        public void AddRole(Role role)
        {
            using var db = CreateContext();
            db.Role.Add(role);
            db.SaveChanges();
        }

        public bool UpdateRole(int id, string newName)
        {
            using var db = CreateContext();
            var role = db.Role.Find(id);
            if (role == null) return false;
            role.Name = newName;
            db.SaveChanges();
            return true;
        }

        public bool DeleteRole(int id)
        {
            using var db = CreateContext();
            var role = db.Role.Find(id);
            if (role == null) return false;
            if (db.Users.Any(u => u.RoleId == id))
                throw new InvalidOperationException("Есть пользователи с этой ролью.");
            db.Role.Remove(role);
            db.SaveChanges();
            return true;
        }

        // ========== Пользователи ==========
        public List<User> GetUsers()
        {
            using var db = CreateContext();
            return db.Users.Include(u => u.Role).ToList();
        }

        public User? GetUser(int id)
        {
            using var db = CreateContext();
            return db.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == id);
        }

        public void AddUser(User user)
        {
            using var db = CreateContext();
            if (db.Role.Find(user.RoleId) == null)
                throw new ArgumentException("Роль не найдена.");
            user.Date_Created = DateTime.Now;
            db.Users.Add(user);
            db.SaveChanges();
        }

        public bool UpdateUser(int id, string? email, string? password, int? roleId)
        {
            using var db = CreateContext();
            var user = db.Users.Find(id);
            if (user == null) return false;

            if (!string.IsNullOrEmpty(email)) user.Email = email;
            if (!string.IsNullOrEmpty(password)) user.Password = password;
            if (roleId.HasValue && db.Role.Find(roleId.Value) != null)
                user.RoleId = roleId.Value;

            db.SaveChanges();
            return true;
        }

        public bool DeleteUser(int id)
        {
            using var db = CreateContext();
            var user = db.Users.Find(id);
            if (user == null) return false;
            if (db.Profile.Any(p => p.User_Id == id))
                throw new InvalidOperationException("У пользователя есть профиль.");
            db.Users.Remove(user);
            db.SaveChanges();
            return true;
        }

        // ========== Профили ==========
        public List<Profile> GetProfiles()
        {
            using var db = CreateContext();
            return db.Profile.Include(p => p.User).ToList();
        }

        public Profile? GetProfile(int id)
        {
            using var db = CreateContext();
            return db.Profile.Include(p => p.User).FirstOrDefault(p => p.Id == id);
        }

        public void AddProfile(Profile profile)
        {
            using var db = CreateContext();
            if (db.Users.Find(profile.User_Id) == null)
                throw new ArgumentException("Пользователь не найден.");
            db.Profile.Add(profile);
            db.SaveChanges();
        }

        public bool UpdateProfile(int id, string? firstName, string? lastName, DateOnly? birthDate, string? phone)
        {
            using var db = CreateContext();
            var profile = db.Profile.Find(id);
            if (profile == null) return false;

            if (!string.IsNullOrEmpty(firstName)) profile.First_Name = firstName;
            if (!string.IsNullOrEmpty(lastName)) profile.Last_Name = lastName;
            if (birthDate.HasValue) profile.Birth_Date = birthDate.Value;
            if (!string.IsNullOrEmpty(phone)) profile.Phone = phone;

            db.SaveChanges();
            return true;
        }

        public bool DeleteProfile(int id)
        {
            using var db = CreateContext();
            var profile = db.Profile.Find(id);
            if (profile == null) return false;
            if (db.Ticket.Any(t => t.Profile_id == id))
                throw new InvalidOperationException("У профиля есть билеты.");
            db.Profile.Remove(profile);
            db.SaveChanges();
            return true;
        }

        // ========== Билеты ==========
        public List<Ticket> GetTickets()
        {
            using var db = CreateContext();
            return db.Ticket
                .Include(t => t.Profile)
                .Include(t => t.Price)
                .Include(t => t.Payment)
                .ToList();
        }

        public Ticket? GetTicket(int id)
        {
            using var db = CreateContext();
            return db.Ticket
                .Include(t => t.Profile)
                .Include(t => t.Price)
                .Include(t => t.Payment)
                .FirstOrDefault(t => t.Id == id);
        }

        public Ticket AddTicket(int profileId, int priceId, DateTime start, DateTime end)
        {
            using var db = CreateContext();
            if (db.Profile.Find(profileId) == null)
                throw new ArgumentException("Профиль не найден.");
            var price = db.Price.Find(priceId) ?? throw new ArgumentException("Цена не найдена.");

            var ticket = new Ticket
            {
                Profile_id = profileId,
                Price_id = priceId,
                Start_Time = start,
                End_Time = end
            };
            db.Ticket.Add(ticket);
            db.SaveChanges();

            // Автоматическое создание платежа
            db.Payment.Add(new Payment
            {
                Ticket_Id = ticket.Id,
                Amount = price.Amount,
                Payment_Date = DateTime.Now
            });
            db.SaveChanges();

            return ticket;
        }

        public bool UpdateTicket(int id, DateTime? start, DateTime? end)
        {
            using var db = CreateContext();
            var ticket = db.Ticket.Find(id);
            if (ticket == null) return false;

            if (start.HasValue) ticket.Start_Time = start.Value;
            if (end.HasValue) ticket.End_Time = end.Value;

            db.SaveChanges();
            return true;
        }

        public bool DeleteTicket(int id)
        {
            using var db = CreateContext();
            var ticket = db.Ticket.Find(id);
            if (ticket == null) return false;

            // Каскадное удаление связанных данных (можно и через FK в БД, но здесь для ясности)
            db.Payment.RemoveRange(db.Payment.Where(p => p.Ticket_Id == id));
            db.Visit.RemoveRange(db.Visit.Where(v => v.Ticket_id == id));
            db.Ticket.Remove(ticket);
            db.SaveChanges();
            return true;
        }

        // ========== Платежи ==========
        public List<Payment> GetPayments()
        {
            using var db = CreateContext();
            return db.Payment.Include(p => p.Ticket).ToList();
        }

        public void AddPayment(Payment payment)
        {
            using var db = CreateContext();
            if (db.Ticket.Find(payment.Ticket_Id) == null)
                throw new ArgumentException("Билет не найден.");
            db.Payment.Add(payment);
            db.SaveChanges();
        }

        public bool UpdatePayment(int id, decimal? amount, DateTime? date)
        {
            using var db = CreateContext();
            var payment = db.Payment.Find(id);
            if (payment == null) return false;

            if (amount.HasValue) payment.Amount = amount.Value;
            if (date.HasValue) payment.Payment_Date = date.Value;

            db.SaveChanges();
            return true;
        }

        public bool DeletePayment(int id)
        {
            using var db = CreateContext();
            var payment = db.Payment.Find(id);
            if (payment == null) return false;
            db.Payment.Remove(payment);
            db.SaveChanges();
            return true;
        }

        // ========== Посещения парка ==========
        public List<Visit> GetVisits()
        {
            using var db = CreateContext();
            return db.Visit.Include(v => v.Ticket).ToList();
        }

        public void AddVisit(Visit visit)
        {
            using var db = CreateContext();
            if (db.Ticket.Find(visit.Ticket_id) == null)
                throw new ArgumentException("Билет не найден.");
            db.Visit.Add(visit);
            db.SaveChanges();
        }

        public bool UpdateVisit(int id, DateTime? entry, DateTime? exit)
        {
            using var db = CreateContext();
            var visit = db.Visit.Find(id);
            if (visit == null) return false;

            if (entry.HasValue) visit.Entry_Time = entry.Value;
            if (exit.HasValue) visit.Exit_Time = exit.Value;

            db.SaveChanges();
            return true;
        }

        public bool DeleteVisit(int id)
        {
            using var db = CreateContext();
            var visit = db.Visit.Find(id);
            if (visit == null) return false;
            db.Visit.Remove(visit);
            db.SaveChanges();
            return true;
        }

        // ========== Аттракционы ==========
        public List<Ride> GetRides()
        {
            using var db = CreateContext();
            return db.Ride.Include(r => r.Group).ToList();
        }

        public void AddRide(Ride ride)
        {
            using var db = CreateContext();
            if (db.RideGroup.Find(ride.Group_id) == null)
                throw new ArgumentException("Группа не найдена.");
            db.Ride.Add(ride);
            db.SaveChanges();
        }

        public bool UpdateRide(int id, string? name, int? groupId, int? minAge, int? duration)
        {
            using var db = CreateContext();
            var ride = db.Ride.Find(id);
            if (ride == null) return false;

            if (!string.IsNullOrEmpty(name)) ride.Name = name;
            if (groupId.HasValue && db.RideGroup.Find(groupId.Value) != null)
                ride.Group_id = groupId.Value;
            if (minAge.HasValue) ride.Min_Age = minAge.Value;
            if (duration.HasValue) ride.Duration_Minuts = duration.Value;

            db.SaveChanges();
            return true;
        }

        public bool DeleteRide(int id)
        {
            using var db = CreateContext();
            var ride = db.Ride.Find(id);
            if (ride == null) return false;
            if (db.RideVisit.Any(rv => rv.Ride_id == id))
                throw new InvalidOperationException("Есть посещения этого аттракциона.");
            db.Ride.Remove(ride);
            db.SaveChanges();
            return true;
        }

        // ========== Группы аттракционов ==========
        public List<RideGroup> GetRideGroups()
        {
            using var db = CreateContext();
            return db.RideGroup.ToList();
        }

        public void AddRideGroup(RideGroup group)
        {
            using var db = CreateContext();
            db.RideGroup.Add(group);
            db.SaveChanges();
        }

        public bool UpdateRideGroup(int id, string newName)
        {
            using var db = CreateContext();
            var group = db.RideGroup.Find(id);
            if (group == null) return false;
            group.Name = newName;
            db.SaveChanges();
            return true;
        }

        public bool DeleteRideGroup(int id)
        {
            using var db = CreateContext();
            var group = db.RideGroup.Find(id);
            if (group == null) return false;
            if (db.Ride.Any(r => r.Group_id == id))
                throw new InvalidOperationException("В группе есть аттракционы.");
            db.RideGroup.Remove(group);
            db.SaveChanges();
            return true;
        }

        // ========== Посещения аттракционов ==========
        public List<RideVisit> GetRideVisits()
        {
            using var db = CreateContext();
            return db.RideVisit.Include(rv => rv.Visit).Include(rv => rv.Ride).ToList();
        }

        public void AddRideVisit(RideVisit rideVisit)
        {
            using var db = CreateContext();
            if (db.Visit.Find(rideVisit.Visit_Id) == null)
                throw new ArgumentException("Посещение не найдено.");
            if (db.Ride.Find(rideVisit.Ride_id) == null)
                throw new ArgumentException("Аттракцион не найден.");
            db.RideVisit.Add(rideVisit);
            db.SaveChanges();
        }

        public bool UpdateRideVisit(int id, int? visitId, int? rideId, DateTime? access, DateTime? exit)
        {
            using var db = CreateContext();
            var rv = db.RideVisit.Find(id);
            if (rv == null) return false;

            if (visitId.HasValue && db.Visit.Find(visitId.Value) != null)
                rv.Visit_Id = visitId.Value;
            if (rideId.HasValue && db.Ride.Find(rideId.Value) != null)
                rv.Ride_id = rideId.Value;
            if (access.HasValue) rv.Access_Time = access.Value;
            if (exit.HasValue) rv.Exit_Time = exit.Value;

            db.SaveChanges();
            return true;
        }

        public bool DeleteRideVisit(int id)
        {
            using var db = CreateContext();
            var rv = db.RideVisit.Find(id);
            if (rv == null) return false;
            db.RideVisit.Remove(rv);
            db.SaveChanges();
            return true;
        }

        // ========== Цены ==========
        public List<Price> GetPrices()
        {
            using var db = CreateContext();
            return db.Price.Include(p => p.Status).Include(p => p.Category).ToList();
        }

        public void AddPrice(Price price)
        {
            using var db = CreateContext();
            if (db.Status.Find(price.Status_Id) == null)
                throw new ArgumentException("Статус не найден.");
            if (db.Category.Find(price.Category_Id) == null)
                throw new ArgumentException("Категория не найдена.");
            db.Price.Add(price);
            db.SaveChanges();
        }

        public bool UpdatePrice(int id, int? statusId, int? categoryId, decimal? amount)
        {
            using var db = CreateContext();
            var price = db.Price.Find(id);
            if (price == null) return false;

            if (statusId.HasValue && db.Status.Find(statusId.Value) != null)
                price.Status_Id = statusId.Value;
            if (categoryId.HasValue && db.Category.Find(categoryId.Value) != null)
                price.Category_Id = categoryId.Value;
            if (amount.HasValue) price.Amount = amount.Value;

            db.SaveChanges();
            return true;
        }

        public bool DeletePrice(int id)
        {
            using var db = CreateContext();
            var price = db.Price.Find(id);
            if (price == null) return false;
            if (db.Ticket.Any(t => t.Price_id == id))
                throw new InvalidOperationException("Есть билеты с этой ценой.");
            db.Price.Remove(price);
            db.SaveChanges();
            return true;
        }

        // ========== Категории ==========
        public List<Category> GetCategories()
        {
            using var db = CreateContext();
            return db.Category.ToList();
        }

        public void AddCategory(Category category)
        {
            using var db = CreateContext();
            db.Category.Add(category);
            db.SaveChanges();
        }

        public bool UpdateCategory(int id, string newName)
        {
            using var db = CreateContext();
            var category = db.Category.Find(id);
            if (category == null) return false;
            category.Name = newName;
            db.SaveChanges();
            return true;
        }

        public bool DeleteCategory(int id)
        {
            using var db = CreateContext();
            var category = db.Category.Find(id);
            if (category == null) return false;
            if (db.Price.Any(p => p.Category_Id == id))
                throw new InvalidOperationException("Есть цены с этой категорией.");
            db.Category.Remove(category);
            db.SaveChanges();
            return true;
        }

        // ========== Статусы ==========
        public List<Status> GetStatuses()
        {
            using var db = CreateContext();
            return db.Status.ToList();
        }

        public void AddStatus(Status status)
        {
            using var db = CreateContext();
            db.Status.Add(status);
            db.SaveChanges();
        }

        public bool UpdateStatus(int id, string newName)
        {
            using var db = CreateContext();
            var status = db.Status.Find(id);
            if (status == null) return false;
            status.Name = newName;
            db.SaveChanges();
            return true;
        }

        public bool DeleteStatus(int id)
        {
            using var db = CreateContext();
            var status = db.Status.Find(id);
            if (status == null) return false;
            if (db.Price.Any(p => p.Status_Id == id))
                throw new InvalidOperationException("Есть цены с этим статусом.");
            db.Status.Remove(status);
            db.SaveChanges();
            return true;
        }
    }
}