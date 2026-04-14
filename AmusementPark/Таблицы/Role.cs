using System;
using System.Collections.Generic;
using System.Text;

namespace AmusementPark.Таблицы
{
    public class Role
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public ICollection<RideVisit> RideVisits { get; set; } = new List<RideVisit>();
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
