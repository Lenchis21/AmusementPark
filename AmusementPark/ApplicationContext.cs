using AmusementPark.Таблицы;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmusementPark
{

    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Category{ get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<Price> Price { get; set; }
        public DbSet<Profile> Profile { get; set; }
        public DbSet<Ride> Ride { get; set; }
        public DbSet<RideGroup> RideGroup { get; set; }
        public DbSet<RideVisit> RideVisit { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<Visit> Visit { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<Profile>()
                .HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<Profile>(p => p.User_Id);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Profile)
                .WithMany(p => p.Tickets)
                .HasForeignKey(t => t.Profile_id);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Price)
                .WithMany(p => p.Tickets)
                .HasForeignKey(t => t.Price_id);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Payment)
                .WithOne(p => p.Ticket)
                .HasForeignKey<Payment>(p => p.Ticket_Id);

            modelBuilder.Entity<Visit>()
                .HasOne(v => v.Ticket)
                .WithMany(t => t.Visits)
                .HasForeignKey(v => v.Ticket_id);
            modelBuilder.Entity<RideVisit>()
                .HasOne(rv => rv.Visit)
                .WithMany(v => v.RideVisits)
                .HasForeignKey(rv => rv.Visit_Id);

            modelBuilder.Entity<RideVisit>()
                .HasOne(rv => rv.Ride)
                .WithMany(r => r.Rides)                // ← исправлено
                .HasForeignKey(rv => rv.Ride_id);

            modelBuilder.Entity<Ride>()
                .HasOne(r => r.Group)
                .WithMany(rg => rg.Rides)
                .HasForeignKey(r => r.Group_id);

            modelBuilder.Entity<Price>()
                .HasOne(p => p.Status)
                .WithMany(s => s.Prices)
                .HasForeignKey(p => p.Status_Id);

            modelBuilder.Entity<Price>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Prices)
                .HasForeignKey(p => p.Category_Id);
        }
    }
}