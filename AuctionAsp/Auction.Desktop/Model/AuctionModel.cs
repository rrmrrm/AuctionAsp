
using Auction.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auction.Desktop.Model
{
    public class AuctionModel
    {
        private readonly AuctionApiService _service;

        public IReadOnlyList<ItemLicitDTO> Items
        { get; }

        /// <summary>
        /// Tartalmazza a kiválasztott trágy részleteit
        /// </summary>
        public ItemAllLicitsDTO ItemDetails
        { get; }

        public ItemDTO EditableItem
        { get; }

        public IReadOnlyList<CategoryDTO> Categories
        { get; }
        public Boolean IsUserLoggedIn { get; }
    }
}
