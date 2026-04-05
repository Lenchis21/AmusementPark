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
        }
    }
}