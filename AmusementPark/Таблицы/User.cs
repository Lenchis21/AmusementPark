using System;
using System.Collections.Generic;
using System.Text;

namespace AmusementPark.Таблицы
{
    public class User
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public DateTime Date_Created { get; set; }
        public Role Role { get; set; } = null!;
        public int RoleId { get; set; }
        public Profile? Profile { get; set; }   // один-к-одному, может быть не сразу создан
    }

}
