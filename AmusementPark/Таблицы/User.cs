using System;
using System.Collections.Generic;
using System.Text;

namespace AmusementPark.Таблицы
{
    public class User
    {

        public int Id { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public DateTime Date_Created { get; set; }

        public Role Role;





    }

}
