using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace Auction.Persistence
{
    public class Licit
    {
        [Key]
        public Int32 Id { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public Int32 Value { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [DisplayName("Item")]
        public Int32 ItemId { get; set; }
        public virtual Item Item { get; set; }

        [Required]
        [DisplayName("User")]
        public Int32 UserId { get; set; }
        public virtual User User { get; set; }
    }
}
