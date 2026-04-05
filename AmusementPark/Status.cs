using System;
using System.Collections.Generic;
using System.Text;

namespace AmusementPark
{
    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Price> Prices;

    }
}
