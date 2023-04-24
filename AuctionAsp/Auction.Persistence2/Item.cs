using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Auction.Persistence
{
    public class Item
    {
        [Key]
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
        [Required]
        public DateTime AuctionStartDate { get; set; }

        public Byte[]? Picture{ get; set; }

        [Required]
        [DisplayName("Category")]
        public Int32 CategoryId { get; set; }
        public virtual Category Category{ get; set; }

        //[Required] //szándékosan nullable pillanatnyilag ez a navigational property, lásd: "todo" feljebb
        [DisplayName("Hirdeto")]
        ///TODO: meg kéne oldani az alábbi problémát: (attól függően hogy mükszik-e a program :nagy probléma, vagy Rekordtörlést megnehezítő probléma, ami a beadandó specifikációnak megfelelését nem rontja el())
        ///nullble foreign kezt allitok be, mert azadatb modellben 'multipleCascadePath' hibat kapok, latszolag azert mert a User torlesekor(~OnDelete()) nem tudja az adatbaziskezelo,
        ///hogy a Licit fele folytassa a torlest(Licit.User), vagy az Item felé(Item.Hirdeto)
        ///ez a probléma a (régen külön entitást képező ) User és Hirdeto osztály egybeolvasztásakor lépett fel
        ///ami pedig azert klepett fel, mert Identitz managert kell hasznalni, amivel nehéz 2 fajta Flehasználó entitscsoportot kezelni egyszerre(valahogy lehet, de bonyolultnak tunik)
        public int? UserId { get; set; }//szándékosan nullable pillanatnyilag ez a navigational property, lásd: "todo" feljebb
        public virtual User Hirdeto { get; set; }// Hirdeto tipusu helyett atirtam User-re, mert mostmár a hirdetők nem külön entitások, csak User-ek akik "hirdeto" role-al rendelkeznek

        public ICollection<Licit> Licits { get; set; }
        public Boolean IsActive() { return AuctionStartDate <= DateTime.Now && DateTime.Now < DateOfClosing; }

        public Item()
        {
            Licits = new HashSet<Licit>();
        }
    }
}
