using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Auction.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auction.Persistence.Services
{
    public class ApiService
    { 
        public readonly AuctionContext c;
        //private readonly UserManager<User> _userManager;///////////////////////////////////
        //private readonly HttpContext httpContext;
        //private readonly LicitValidator LicitValidator;

        public ApiService(AuctionContext _ac /*,UserManager<User> userManager, IHttpContextAccessor httpContextAccessor////////////////////////////////////*/)
        {
            //httpContext = httpContextAccessor.HttpContext;
            //_userManager = userManager;/////////////////////////////////
            //this.LicitValidator = new LicitValidator(this);
            c = _ac;
        }
        IQueryable<ItemLicitDTO> ItemsWithMaxLicit()
        {
            IQueryable<ItemLicitDTO> ret = c.Licits
            .Include(l => l.Item)
            .ThenInclude(i => i.Hirdeto)

            .GroupBy(l => new
            {
                l.Item.Id,
                l.Item.Name,
                HirdetoName = l.Item.Hirdeto.UserName,
                CategoryId = l.Item.CategoryId,

                AuctionStartDate = l.Item.AuctionStartDate,
                AuctionCloseDate = l.Item.DateOfClosing
            })
            .Select(g => new ItemLicitDTO
            {
                ItemId = g.Key.Id,
                ItemName = g.Key.Name,
                AuctionStartDate = g.Key.AuctionStartDate,
                AuctionCloseDate = g.Key.AuctionCloseDate,

                ItemHirdetoName = g.Key.HirdetoName,
                CategoryId = g.Key.CategoryId,

                ActiveLicit = g.Max(k => k.Value)//a legnagyobb licit az aktuális
            });
            return ret;
        }
        /// <summary>
        /// Tárgyak DTO-ként, amikre nem szavaztak. 
        /// Minden visszaadott ItemLicitDTO  ActiveLicit property-j az Item Kezdőlicitje(StartingLicit) lesz
        /// </summary>
        /// <returns></returns>
        public IQueryable<ItemLicitDTO> ItemsWithoutLicit()
        {
            IQueryable<ItemLicitDTO> ret = c.Items
                .Include(i => i.Licits)
                .Include(i => i.Hirdeto)
                .Where(i => i.Licits.Count == 0)
                .Select(i => new ItemLicitDTO
                {
                    ItemId = i.Id,
                    ItemName = i.Name,
                    AuctionStartDate = i.AuctionStartDate,
                    AuctionCloseDate = i.DateOfClosing,

                    ItemHirdetoName = i.Hirdeto.UserName,
                    CategoryId = i.CategoryId,

                    ActiveLicit = i.StartingLicit//a legnagyobb licit az aktuális

                });
            return ret;
        }

        /// <summary>
        /// hirdető által hirdetett tárgyak aktuális licittel
        /// </summary>
        /// <param name="hirdetoName"></param>
        /// <returns></returns>
        public IQueryable<ItemLicitDTO> GetHirdetettByHirdetoUName(String hirdetoName)
        {
            return GetHirdetett2()
                .Where(i => i.ItemHirdetoName == hirdetoName)
                .OrderBy(dto => dto.AuctionCloseDate);
        }
        /// <summary>
        /// meghirdetett tárgyak az aktuális licittel
        /// </summary>
        /// <returns></returns>
        IQueryable<ItemLicitDTO> GetHirdetett2() 
        {
            IQueryable<ItemLicitDTO> ret =
                ItemsWithMaxLicit()
                //a kezdőlicit vonatkozik aktuálisnak azoknál a tárgyaknál, amikre nem érkezett licit:
                .Union(
                    ItemsWithoutLicit()
                );
            return ret;
        }

        /// <summary>
        /// add item t Items Dbset
        /// </summary>
        /// <param name="item"></param>
        public void addItem(Item item) /*throws DbUpdateException, DbConcurrencyException*/
        {
            c.Items.Add(item);
            c.SaveChanges();
        }
        /// <summary>
        /// targy kategorival együtt
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ItemDTO GetItemWithCategAndHirdeto(Int32 id)
        {
            Item item = c.Items
                .Where(it => it.Id == id)
                .Include(i => i.Category)
                .Include(i => i.Hirdeto)
                .Single();
            return (ItemDTO)item;
        }
        /// <summary>
        /// a kategóriák CategoryDTO-vá konvertálva
        /// </summary>
        /// <returns></returns>
        public IQueryable<CategoryDTO> GetCategories()
        {
            IQueryable<CategoryDTO> ret = c.Categories
                .Select(c => new CategoryDTO { Id = c.Id, Name = c.Name });
            return ret;
        }
        /// <summary>
        /// ha nincs ilyen azonositójú tárgy, vagy a tárgyat nem a megadott felhasználónevű user hirdette, akkor hamisat ad vissz,
        /// amúgy igazat
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Boolean IsUserHirdetoOf(String username, Int32 itemId)
        {
            Item item = c.Items
                .Where(it => it.Id == itemId)
                .Include(i => i.Hirdeto)
                .Where(it => it.Hirdeto.UserName == username)
                .FirstOrDefault();
            return (item != null);
        }
        /// <summary>
        /// kikeresi a megadott azonosítójú tárgyat. 
        /// null-t ad vissza, ha nincs ilyen
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Item GetItemById(Int32 id)
        {
            Item item = c.Items.Where(it => it.Id == id)
                .FirstOrDefault();
            return item;
        }

        /// <summary>
        /// Megnézi, hogy létezik-e  a megadott azonosítóval rendelkező kategória
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsCategoryExists(int id)
        {
            return c.Categories.FirstOrDefault(c => c.Id == id) != null;
        }
        /// <summary>
        /// az id azonosítójú tárgyat visszaadja ItemAllLicitsDTO-ként, miután feltöltötte a tárgy licitjeinek listáját
        /// hibát dob, ha nincs ilyen azonosítójú tárgy
        ///
        /// tárgyat kiválasztva megkapjuk az összes információt képpelegyütt,
        /// valamint az összes addigi licitet (dátum, név, összeg). 
        /// Lehetőségünk van a licit azonnalilezárására(csak ha valaki már licitált rá, ekkor az aktuális licitáló viszi a tárgyat)
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ItemAllLicitsDTO GetItemWithAllLicits(Int32 id)
        {
            Item item = c.Items.Where(it => it.Id == id)
                .Include(i => i.Licits)
                .ThenInclude(l => l.User)
                .Include(i => i.Category)
                .Include(i => i.Hirdeto)
                .Single();
            return ItemAllLicitsDTO.FromItem(item, null);
        }

        public ItemLicitDTO GetItemWitActualLicit(Int32 id)
        {
            ItemLicitDTO ret = GetHirdetett2().Single(i => i.ItemId == id);
            return ret;
        }
        /// <summary>
        /// ha  nem találja a tárgyat, akkor InvalidOperaationt dob(.Single());
        /// megkeresi az id azonosítójú tárgyat, feltölti a navigational property-jeit,
        /// Ezután megkeresi a licitet vezető felhasználót(itt feltételezzük, hogy licitáltak a tárgyra, ezt a controllerben ellenőrízni kell)
        /// lezárja a szavazást(item.DateOfClosing beállításával);
        /// A tárgy sikeres frissítése esetén ItemAllLicitDTO típusban küldjük vissza a lezárt licitálást és a nyertes felhaasználó adatait,
        ///  ha nem sikeres, akkor DbUpdateException-t dob
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ItemAllLicitsDTO CloseLicit(int Id)
        {
            ///todo ez jól mukodik?? tesytelni
            Item item = c.Items
                .Where(i => i.Id == Id)
                .Include(i => i.Licits)
                .ThenInclude(l => l.User)
                .Include(i => i.Hirdeto)
                .Include(i => i.Category)
                .Single();
            int maxVal = 0; Licit maxLicit = null;
            
            //maximális értékű licit megkeresese:
            foreach(Licit l in item.Licits.ToList()){
                if(l.Value>maxVal) { maxVal = l.Value; maxLicit = l; }
            }
            
            //licit lezárása:
            
            item.DateOfClosing = DateTime.Now;
            c.SaveChanges();

            //ha a saveChanges nem dobott hibat, visszaadjuk a lezart Itemet, és a nyertest egy ItemAllLicitsDTO-ban:
            User winner = maxLicit.User;
            return ItemAllLicitsDTO.FromItem(item,winner);
            /*
            c.Licits
                .Where(l => l.ItemId == Id)
                .Include(l => l.User)
                .Include()
            */
        }
        /*
        public IQueryable<ItemLicitVM> ILMs_To_ILVMs(IQueryable<ItemLicitModel> ilm)
        {
            IQueryable<ItemLicitVM> itemLicitVMs = ilm.Select(ilm => new ItemLicitVM { 
                ItemId =ilm.ItemId,
                ItemName = ilm.ItemName,
                AuctionStartDate = ilm.AuctionStartDate,
                AuctionCloseDate = ilm.AuctionCloseDate,
                ItemHirdetoName = ilm.ItemHirdetoName,
                CategoryId= ilm.CategoryId,
                ActiveLicit = ilm.ActiveLicit,

                CategoryName = c.Items
                        .Include(i => i.Category)
                        .First(i => i.Id == ilm.ItemId)
                        .Category.Name
            });

            return itemLicitVMs;
        }



        public Int32 GetActualLicitValueForItem(Int32 ItemId, out Boolean returnedIsStartingLicit)
        {
            ItemLicitModel il = ItemsWithMaxLicit().FirstOrDefault(il => il.ItemId == ItemId);
            if(il == null)
            {
                //ha nem érkezett még licit:
                Int32 ret = c.Items.First(i => i.Id == ItemId).StartingLicit;
                returnedIsStartingLicit = true;
                return ret;
            }
            else
            {
                returnedIsStartingLicit = false;
                return il.ActiveLicit;
            }
        }

        public Item getItem(Int32? ItemId){
            if(ItemId == null)
            {
                return null;
            }
            Item item = c.Items.
                FirstOrDefault(i => i.Id == ItemId);
            return item;
        }
        public Byte[] GetItemImage(Int32? itemId)
        {
            if (itemId == null)
                return null;

            // lekérjük az épület első tárolt képjét (kicsiben)
            byte[] picture = c.Items
                .Where(i => i.Id == itemId)
                .Select(i => i.Picture)
                .FirstOrDefault();
            return picture;
        }
        public Boolean IsItemActive(Int32 ItemId)
        {
            Item item = getItem(ItemId);
            if(item == null)
            {
                throw new System.Exception("IsItemActive(ItemId): A megadott 'ItemId'-hoz nem tartozik tárgy");
            }
            return (item.IsActive());
        }
        //public String CurrentUserName => httpContext.Session.GetString("user");
        public User GetUserByUsername(String username)
        {
            User ret = c.Users.FirstOrDefault(u => u.UserName == username);
            return ret;
        }
        public LicitError ValidateLicit(LicitVM lvm)
        {
            return LicitValidator.Validate(lvm);
        }
        public Boolean Licit(LicitVM lvm, String loggedinUserName)
        {
            // ellenőrizzük az annotációkat
            if (!Validator.TryValidateObject(lvm, new ValidationContext(lvm, null, null), null))
                return false;

            if (LicitValidator.Validate(lvm) != LicitError.None)
                return false;

            c.Licits.Add(new Licit
            {
                Date = DateTime.Now,
                ItemId = lvm.Item.Id,
                Value = (Int32)lvm.Value,
                UserId = GetUserByUsername(loggedinUserName).Id
            });
            try
            {
                c.SaveChanges();
            }
            catch (Exception)
            {
                // mentéskor lehet hiba
                return false;
            }
            //nincs hiba
            return true;
        }


        class HelperRec
        {
            public Int32 ItemId { get; set; }
            public String ItemName { get; set; }
            public DateTime AuctionStartDate { get; set; }
            public DateTime AuctionCloseDate { get; set; }

            public String ItemHirdetoName { get; set; }
            public Int32 ItemCategoryId { get; set; }

            public Int32 ActiveLicit { get; set; }

            public Int32 MaxLicitByUser { get; set; }
            public Boolean IsUserLeading { get; set; }
            //public String ItemName { get; set; }
            //public String ItemHirdetoName { get; set; }
        }
        public IQueryable<ItemLicitedByUserVM> ItemsLicitedByUser(String loggedinUserName)
        {
            String UserName = loggedinUserName;
            if (UserName == null)
            {
                return (IQueryable<ItemLicitedByUserVM>)new List<ItemLicitedByUserVM>();
            }
            IQueryable<HelperRec> baseQuery = c.Licits
            .Include(l => l.User)// eltérés ItemsWithMaxLicit-től
            .Where(l => l.User.UserName == UserName)//eltérés ItemsWithMaxLicit-től
            .Include(l => l.Item)
            .ThenInclude(i => i.Hirdeto)

            .GroupBy(l => new
            {
                l.Item.Id,
                l.Item.Name,
                hirdetoName = l.Item.Hirdeto.UserName,
                //AuctionStartDate = l.Item.AuctionStartDate,
                //AuctionCloseDate = l.Item.DateOfClosing
            })
            .Select(g => new
            {
                ItemId = g.Key.Id,
                //ItemName = g.Key.Name,
                MaxLicitByUser = g.Max(lu => lu.Value),//a USER ÁLTAL leadott legnagyobb licit USER ÁLTAL leadott legnagyobb licit(a fentebbi Where() feltételben szűrtünk a USER licitjeire)
                //ItemHirdetoName = g.Key.hirdetoName,
                //AuctionStartDate = g.Key.AuctionStartDate,
                //AuctionCloseDate = g.Key.AuctionCloseDate
            })
            .Join(ItemsWithMaxLicit(), ilbuVM => ilbuVM.ItemId, ilm => ilm.ItemId, (ilbu, ilm) => new HelperRec
            {
                ItemId = ilbu.ItemId,
                ItemName = ilm.ItemName,
                AuctionCloseDate = ilm.AuctionCloseDate,
                AuctionStartDate = ilm.AuctionStartDate,

                ItemHirdetoName = ilm.ItemHirdetoName,
                ItemCategoryId = ilm.CategoryId,

                ActiveLicit = ilm.ActiveLicit,
                MaxLicitByUser = ilbu.MaxLicitByUser
            });

            IQueryable<ItemLicitedByUserVM> ret = baseQuery
            //.Join(c.Categories, il => il.CategoryId, c => c.Id, (il, c) => new ItemLicitVM { });
            // ha egy rekordban maxLicit==maxLicitByUser, akkor a user-é A maximális licit az adott tárgyra
            .Where(o => o.ActiveLicit == o.MaxLicitByUser)
            .Select(i => new ItemLicitedByUserVM
            {
                IsUserLeading = true,
                ItemId = i.ItemId,
                ActiveLicit = i.ActiveLicit,
                ItemName = i.ItemName,
                ItemHirdetoName = i.ItemHirdetoName,
                AuctionCloseDate = i.AuctionCloseDate,
                AuctionStartDate = i.AuctionStartDate,
                CategoryId = i.ItemCategoryId,
            })
            .Union(
                baseQuery
                .Where(o => o.ActiveLicit != o.MaxLicitByUser)//itt tulajdonképpen azon (item,licit) párokat vesszük, ahol nem a USER vezeti a licitet
                .Select(i => new ItemLicitedByUserVM
                {
                    IsUserLeading = false,
                    ItemId = i.ItemId,
                    ActiveLicit = i.ActiveLicit,
                    ItemName = i.ItemName,
                    ItemHirdetoName = i.ItemHirdetoName,
                    AuctionCloseDate = i.AuctionCloseDate,
                    AuctionStartDate = i.AuctionStartDate,
                    CategoryId = i.ItemCategoryId,
                })
            );
            return ret;
        }
        */
    }
}
