using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auction.Persistence
{
    public class ItemLicitDTO
    {
        public Int32 ItemId { get; set; }
        public String ItemName { get; set; }
        public DateTime AuctionStartDate { get; set; }
        public DateTime AuctionCloseDate { get; set; }

        public String ItemHirdetoName { get; set; }
        public Int32 CategoryId { get; set; }

        public Int32 ActiveLicit { get; set; }

        public Boolean IsActive() { return AuctionStartDate <= DateTime.Now && DateTime.Now < AuctionCloseDate; }

        public ItemLicitDTO() { }
        /*
        public ItemLicitDTO(Int32 ItemId, String ItemName, DateTime AuctionStartDate, DateTime AuctionCloseDate,
                           String ItemHirdetoName, Int32 CategoryId, Int32 ActiveLicit)
        {
            this.ItemId = ItemId;
            this.ItemName       = ItemName;
            this.AuctionStartDate   = AuctionStartDate;
            this.AuctionCloseDate = AuctionCloseDate;
            this.ItemHirdetoName = ItemHirdetoName ;
            this.CategoryId         = CategoryId;
            this.ActiveLicit     = ActiveLicit;
        }
        */
    }
}