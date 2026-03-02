using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResturantReserve.Models
{
    public class CardData
    {
        public string Type { get; set; } = "Number";
        public int Value { get; set; } = 0;
        public int Index { get; set; } = 0;

        public CardData() { }
    }
}
