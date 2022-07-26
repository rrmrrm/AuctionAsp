using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace Auction.Persistence
{
    public class LicitDTO
    {

        public Int32 Id { get; set; }

        public Int32 Value { get; set; }

        public DateTime Date { get; set; }

        //public Int32 ItemId { get; set; }
        //public virtual ItemDTO Item { get; set; }

        //public Int32 UserId { get; set; }
        public virtual String UserName { get; set; }
        /*
        public static explicit operator Licit(LicitDTO dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            return new Licit
            {
                Id = dto.Id,
                Value = dto.Value,
                Date = dto.Date,
                //nem kerül bele az item a konvertált Licitbe-ba(körbehivatkozás elkerülése érdekében), ha kell a licithez tartozo item, akkor az Items-ből kell majd queryt
                //ItemId = dto.Item.Id, 
                //Item = dto.Item,
                UserId = dto.User.Id,
                User = (User)dto.User,
            };
        }
        */
        public static explicit operator LicitDTO(Licit ent)
        {
            if (ent is null)
            {
                throw new ArgumentNullException(nameof(ent));
            }
            if (ent.User is null)
            {
                throw new ArgumentNullException(
                    nameof(ent.User),
                    "ERROR: LicitDTO operator(Licit): User is null. forgot the .Include(licit=>licit.User) in the query?"
                );
            }

            return new LicitDTO
            {
                Id = ent.Id,
                Value = ent.Value,
                Date = ent.Date,
                UserName = ((UserDTO)ent.User).UserName
            };
        }
    }
}
