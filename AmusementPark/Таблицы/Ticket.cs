using System;
using System.Collections.Generic;
using System.Text;

namespace AmusementPark.Таблицы
{
    public class Ticket
    {
        public int Id { get; set; }
        public int Profile_id { get; set; }
        public int Price_id { get; set; }
        public DateTime Start_Time { get; set; }
        public DateTime End_Time { get; set; }
        public Profile Profile { get; set; }
        public Price Price { get; set; }
        public Payment Payment { get; set; }
        public ICollection<Visit> Visits { get; set; }
    }
}
