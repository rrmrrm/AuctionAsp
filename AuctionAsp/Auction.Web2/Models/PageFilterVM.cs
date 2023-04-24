using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Auction.Persistence
{
    public class PageFilterVM
    {
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a non-negative value!")]
        public Int32? PageNum  { get; set; }

        [MaxLength(100, ErrorMessage = "Filter string cannot be longer than 100 characters!")]
        public String? NameFilter { get; set; }

        public Boolean Categorical { get; set; }

        public Boolean OnlyActive { get; set; }


    }
}
