using System;
using System.Collections.Generic;
using System.Text;

namespace AmusementPark
{
    public class Profile
    {
        public int Id {  get; set; }
        public int User_Id { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public DateOnly Birth_Date { get; set; }
        public string Phone { get; set; }
        
        public User User { get; set; }
        public ICollection<Ticket> Tickets { get; set; }

    }
}
