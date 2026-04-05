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
    }
}