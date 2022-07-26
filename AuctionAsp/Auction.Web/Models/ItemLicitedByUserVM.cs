using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auction.Persistence
{
    public class ItemLicitedByUserVM: ItemLicitModel
    {
        //public Boolean IsActive { get; set; }
        public Boolean IsUserLeading { get; set; }
    }
}
