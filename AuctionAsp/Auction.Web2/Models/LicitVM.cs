using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Auction.Persistence
{
    public class LicitVM
    {
        //public Int32 ItemId { get; set; }
        //public String ItemName { get; set; }

        [Required(ErrorMessage = "A licitösszeg megadása kötelező")] 
        [Range(0, int.MaxValue, ErrorMessage = "Nemnegatív értéket adjon meg!")]
        [DataType(DataType.Currency)]
        public Int32? Value { get; set; }


        public Item Item { get; set; }
        public Int32? ActualLicit { get; set; }

    }
}
