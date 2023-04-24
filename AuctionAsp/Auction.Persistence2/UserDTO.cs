using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Security;
namespace Auction.Persistence
{
    /// TODO: identity keyelés a példaprogik alapján Hirdeto es Userben is kell valszeg
    public class UserDTO
    {
        public Int32 Id { get; set; }

        public String Name { get; set; }

        public String PhoneNumber { get; set; }

        public String UserName { get; set; }


        //public String PasswordHash { get; set; }

        public String Email { get; set; }

        //public string SecurityStamp { get; set; }

        public static explicit operator User(UserDTO dto)
        {
            if (dto is null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            return new User
            {
                Id = dto.Id,
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                UserName = dto.UserName,
                //SecurityStamp = dto.SecurityStamp,
                //PasswordHash = dto.PasswordHash,
            };
        }

        public static explicit operator UserDTO(User ent)
        {
            if (ent is null)
            {
                throw new ArgumentNullException(nameof(ent));
            }

            return new UserDTO
            {
                Id = ent.Id,
                Name = ent.Name,
                Email = ent.Email,
                PhoneNumber = ent.PhoneNumber,
                UserName = ent.UserName,
                //SecurityStamp = ent.SecurityStamp,
                //PasswordHash = ent.PasswordHash,
            };
        }
    }
}
