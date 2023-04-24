using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auction.Persistence
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public String Name { get; set; }

        public static explicit operator CategoryDTO(Category ent)
        {
            if (ent is null)
            {
                throw new ArgumentNullException(nameof(ent));
            }

            return new CategoryDTO
            {
                Id = ent.Id,
                Name = ent.Name
            };
        }

        /*public static explicit operator Category(CategoryDTO dto)
        {
            if (dto is null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            return new Category
            {
                Id = dto.Id,
                Name = dto.Name
            };
        }*/
    }
}

