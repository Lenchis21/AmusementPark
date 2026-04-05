using System;
using System.Collections.Generic;
using System.Text;

namespace AmusementPark.Таблицы
{
    public class RideGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Ride> Rides { get; set; }
    }

}
