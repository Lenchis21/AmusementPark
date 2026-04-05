using System;
using System.Collections.Generic;
using System.Text;

namespace AmusementPark.Таблицы
{
    public class Ride
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Group_id { get; set; }
        public int Min_Age { get; set; }
        public int Duration_Minuts { get; set; }
        public RideGroup Group { get; set; }
        public ICollection<RideVisit> Rides { get; set; }
    }
}
