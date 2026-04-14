using System;
using System.Collections.Generic;
using System.Text;

namespace AmusementPark.Таблицы
{
    public class Profile
    {
        public int Id { get; set; }
        public int User_Id { get; set; }
        public required string First_Name { get; set; }
        public required string Last_Name { get; set; }
        public DateOnly Birth_Date { get; set; }
        public string Phone { get; set; } = string.Empty;   // необязательный
        public User User { get; set; } = null!;
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
