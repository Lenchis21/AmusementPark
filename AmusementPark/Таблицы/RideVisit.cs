using System;
using System.Collections.Generic;
using System.Text;

namespace AmusementPark.Таблицы
{
    public class RideVisit
    {
        public int Id { get; set; }
        public int Visit_Id { get; set; }
        public int Ride_id { get; set; }
        public DateTime Access_Time { get; set; }
        public DateTime Exit_Time { get; set; }
        public Visit Visit { get; set; } = null!;
        public Ride Ride { get; set; } = null!;
    }
}
