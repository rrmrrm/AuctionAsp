using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auction.Persistence
{
    public class ItemLicitVM : ItemLicitModel
    {
        //public Boolean IsActive { get; set; }


        public String CategoryName { get; set; }
        /*
        public ItemLicitVM():base()
        {
            IsActive = DateTime.Now < base.AuctionCloseDate;
            this.CategoryName = CategoryName;
        }
        public ItemLicitVM(ItemLicitModel ilm, String CategoryName) 
            : base()
        {
            IsActive = DateTime.Now < ilm.AuctionCloseDate;
            this.CategoryName = CategoryName;

            ItemId = ilm.ItemId;
            ItemName = ilm.ItemName;
            ItemHirdetoName = ilm.ItemHirdetoName;
            ActiveLicit = ilm.ActiveLicit;
            AuctionStartDate = ilm.AuctionStartDate;
        }
        */
    }
}
