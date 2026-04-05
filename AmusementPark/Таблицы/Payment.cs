using System;
using System.Collections.Generic;
using System.Text;

namespace AmusementPark.Таблицы
{
    public class Payment
    {
        public int Id { get; set; }
        public int Ticket_Id { get; set; }
        public Decimal Amount { get; set; }
        public DateTime Payment_Date { get; set; }

        public Ticket Ticket { get; set; }
    }
}
