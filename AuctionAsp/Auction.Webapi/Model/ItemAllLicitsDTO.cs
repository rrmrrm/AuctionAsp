using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Auction.Persistence
{
    public class ItemAllLicitsDTO
    {
        [Required]
        public ItemDTO item { get; set; }
        [Required]
        public IList<LicitDTO> licits { get; set; }
        [Required]
        public UserDTO hirdeto { get; set; }

        public UserDTO winner { get; set; } // alapból null, viszont az api apiService-ében closeLicit-ben feltöltjük a winner-t az itteni FromItem függvény használatával

        /// <summary>
        /// Item -> ItemAllLicitsDTO
        /// </summary>
        /// <param name="ent">item</param>
        /// <param name="_winner">nyertes felhasznalo. default: null</param>
        /// <returns></returns>
        public static ItemAllLicitsDTO FromItem(Item ent, User _winner=null)
        {
            if (ent is null) 
            {
                throw new ArgumentNullException(nameof(ent));
            }
            if (ent.Licits is null)
            {
                throw new ArgumentNullException(
                    nameof(ent.Licits),
                    "ERROR: ItemAllLicitsDTO.FromItem(Item): Licits is null. forgot the .Include(item=>item.Licits) in the query?"
                );
            }
            if (ent.Hirdeto is null)
            {
                throw new ArgumentNullException(
                    nameof(ent.Hirdeto),
                    "ERROR: ItemAllLicitsDTO.FromItem(Item): Hirdeto is null. forgot the .Include(item=>item.Hirdeto) in the query?"
                );
            }
            return new ItemAllLicitsDTO
            {
                item = (ItemDTO)ent,
                licits = ent.Licits.Select(l => (LicitDTO)l).ToList(),
                hirdeto = (UserDTO)ent.Hirdeto,
                winner = _winner==null ? null : (UserDTO)_winner
            };
        }
    }
}
