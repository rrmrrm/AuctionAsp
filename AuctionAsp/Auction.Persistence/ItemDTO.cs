using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Auction.Persistence
{
    public class ItemDTO
    {
        public Int32 Id { get; set; }
        [Required]
        public String Name { get; set; }
        [Required]
        public String Description { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public Int32 StartingLicit { get; set; }
        [Required]
        public DateTime DateOfClosing { get; set; }

        public DateTime AuctionStartDate { get; set; }

        public Byte[] Picture{ get; set; }
        [Required]
        //public Int32 CategoryId { get; set; }
        public virtual CategoryDTO Category { get; set; }

        //public Int32 HirdetoId { get; set; }
        public  UserDTO Hirdeto { get; set; }// Hirdeto tipusu helyett atirtam User-re, mert mostmár a hirdetők nem külön entitások, csak User-ek akik "hirdeto" role-al rendelkeznek

        //public ICollection<LicitDTO> Licits { get; set; }

        public Boolean IsActive { get; set; }
        public static Boolean IsActiveFunc(ItemDTO dto) { return dto.AuctionStartDate <= DateTime.Now && DateTime.Now < dto.DateOfClosing; }


        public ItemDTO()
        {
            //Licits = new HashSet<LicitDTO>();
        }
        /*
        public static explicit operator Item(ItemDTO dto) => new Item
        {
            Id = dto.Id,
            AuctionStartDate = dto.AuctionStartDate,
            Category = (Category)dto.Category,
            CategoryId = dto.Category.Id,
            DateOfClosing = dto.DateOfClosing,
            Description = dto.Description,
            UserId = dto.Hirdeto.Id,
            Hirdeto = (User)dto.Hirdeto,
            //Licits = (Licit)dto.Licits,
            Name = dto.Name,
            Picture = dto.Picture,
            StartingLicit = dto.StartingLicit,
        };
        */
        public static explicit operator ItemDTO (Item ent)
        {
            if (ent is null)
            {
                throw new ArgumentNullException(nameof(ent));
            }
            if (ent.Hirdeto is null)
            {
                throw new ArgumentNullException(
                    nameof(ent.Hirdeto),
                    "ERROR: ItemDTO.operator(Item): Hirdeto is null. forgot the .Include(item=>item.Hirdeto) in the query?"
                );
            }
            if (ent.Category is null)
            {
                throw new ArgumentNullException(
                    nameof(ent.Category),
                    "ERROR: ItemDTO.operator(Item): Category is null. forgot the .Include(item=>item.Category) in the query?"
                );
            }

            return new ItemDTO
            {
                Id = ent.Id,
                AuctionStartDate = ent.AuctionStartDate,
                Category = (CategoryDTO)ent.Category,
                //CategoryId = ent.Category.Id,
                DateOfClosing = ent.DateOfClosing,
                Description = ent.Description,
                Hirdeto = (UserDTO)ent.Hirdeto,
                //Hirdeto = (UserDTO)ent.Hirdeto,
                //Licits = ent.Licits?.Select(l => (LicitDTO)l ).ToList(),
                IsActive = ent.IsActive(),
                Name = ent.Name,
                Picture = ent.Picture,
                StartingLicit = ent.StartingLicit,
            };
        }
    }
}
