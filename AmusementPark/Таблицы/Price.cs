using System;
using System.Collections.Generic;
using System.Text;

namespace AmusementPark.Таблицы
{
    public class Price
    {
        public int Id { get; set; }
        public int Status_Id { get; set; }
        public int Category_Id { get; set; }
        public decimal Amount { get; set; }
        public Status Status { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
