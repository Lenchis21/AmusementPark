using System;
using System.Collections.Generic;
using System.Text;

namespace AmusementPark
{
    public class Price
    {
        public int Id { get; set; }
        
        public int Status_Id { get; set; }
        public int Category_Id { get; set; }
        public Decimal Amount { get; set; }
        public  Status  Status { get; set; }
        public Category Category { get; set; }

        public ICollection<Ticket> Tickets;
    }
}
