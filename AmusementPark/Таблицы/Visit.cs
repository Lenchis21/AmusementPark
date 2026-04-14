using System;
using System.Collections.Generic;
using System.Text;

namespace AmusementPark.Таблицы
{
    public class Visit
    {
        public int Id { get; set; }
        public int Ticket_id { get; set; }
        public DateTime Entry_Time { get; set; }
        public DateTime Exit_Time { get; set; }
        public Ticket Ticket { get; set; } = null!;
        public ICollection<RideVisit> RideVisits { get; set; } = new List<RideVisit>();
    }
}
