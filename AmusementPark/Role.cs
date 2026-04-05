using System;
using System.Collections.Generic;
using System.Text;

namespace AmusementPark
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }

        ICollection<User> Users;

    }
}
