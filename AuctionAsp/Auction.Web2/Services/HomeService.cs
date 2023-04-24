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
    public class HomeService
    { 
        private readonly AuctionContext c;
        private readonly UserManager<User> _userManager;
        private readonly HttpContext httpContext;
        private readonly LicitValidator LicitValidator;

        public HomeService(AuctionContext _ac, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
            _userManager = userManager;
            this.LicitValidator = new LicitValidator(this);
            c = _ac;
        }
        
        public IQueryable<ItemLicitModel> FilterAndPageItemLicitVMs(IQueryable<ItemLicitModel> ItemLicitMs, PageFilterVM pf = null, Int32 pageSize = 20)
        {
            IQueryable<ItemLicitModel> filtered;
            IQueryable<ItemLicitModel> ordered;
            IQueryable<ItemLicitModel> paged;

            if (pf == null)
            {
                pf = new PageFilterVM { PageNum = null, NameFilter = null };
            }

            if (pf.PageNum == null)
            {
                pf.PageNum = 0;
            }
            if (pf.NameFilter == null)
            {
                pf.NameFilter = "";
            }
            if (pf.Categorical == null)
            {
                pf.Categorical = false;
            }
            if (pf.OnlyActive == null)
            {
                pf.OnlyActive = false;
            }

            Int32 pageStartInd = (Int32)pf.PageNum * pageSize;
            if (pageStartInd < 1)
            {
                pageStartInd = 1;
            }
            Int32 skipNum = pageStartInd - 1;
            Boolean OnlyActive = (Boolean)pf.OnlyActive;
            filtered = ItemLicitMs
                //úgy tűnik itt nem használhatom az IsActive() függvényt, "could not be translated" hibát dob a query futtatásakor, ha IsActive()-ot használom a ehelyett:
                .Where(o => !OnlyActive || (o.AuctionStartDate <= DateTime.Now && DateTime.Now < o.AuctionCloseDate) )
                .Where(l => l.ItemName.Contains(pf.NameFilter));

            
            if ((Boolean)pf.Categorical)
            {
                ordered = filtered
                    .Join(c.Categories, il => il.CategoryId, c => c.Id, (il, c) => new ItemLicitVM
                    {
                        ItemId = il.ItemId,
                        ItemName = il.ItemName,
                        AuctionStartDate = il.AuctionStartDate,
                        AuctionCloseDate = il.AuctionCloseDate,
                        ItemHirdetoName = il.ItemHirdetoName,
                        CategoryId = il.CategoryId,

                        CategoryName = c.Name,
                        ActiveLicit = il.ActiveLicit,
                    })
                    .OrderBy(l => l.CategoryName)
                    .ThenBy(l => l.AuctionStartDate);
            }
            else
            {
                ordered = filtered
                    .OrderByDescending(l => l.AuctionStartDate);
            }

            paged = ordered
                .Skip(skipNum)
                .Take(pageSize);
            return paged;
        }

        public IQueryable<ItemLicitModel> ItemsWithMaxLicit()
        {
            IQueryable<ItemLicitModel> ret = c.Licits
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
            .Select(g => new ItemLicitModel
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
        public IQueryable<ItemLicitModel> ItemsWithoutLicit()
        {
            IQueryable<ItemLicitModel> ret = c.Items
                .Include(i => i.Licits)
                .Include(i => i.Hirdeto)
                .Where(i => i.Licits.Count == 0)
                .Select(i => new ItemLicitModel
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

        public async Task<IQueryable<ItemLicitModel>> GetHirdetettByLoggedInHirdeto(String hirdetoUName)
        {
            ///TODO: itt nem guest-et, hanem Hirdetot Kell használni
            ///
            ///a példaprogramban(travAgerncyTest_13) így kéri le a bejelentkezett felhasználót:
            /// kommentben látható, ahogy egy felhasználóhoz kapcsolódó(navigation prop) objektumot mentenek az adatbázisba:
            // a felhasználót a név alapján betöltjük
            User user = await _userManager.FindByNameAsync(hirdetoUName);
            if (user == null){
                return new List<ItemLicitModel>().AsQueryable();
            }
            /*
            _context.Rents.Add(new Rent
            {
                ApartmentId = rent.Apartment.Id,
                UserId = guest.Id,
                StartDate = rent.RentStartDate,
                EndDate = rent.RentEndDate
            });
            */

            return GetHirdetettByHirdetoUName(hirdetoUName);
        }
        //megadott hirdető által hirdetett tárgyak a hirdető USERNEVE ALAPJÁN
        public IQueryable<ItemLicitModel> GetHirdetettByHirdetoUName(String hirdetoName)
        {
            return GetHirdetett2()
                .Where(i => i.ItemHirdetoName == hirdetoName);
        }
        public IQueryable<ItemLicitModel> GetHirdetettByHirdetoId(Int32 id)
        {
            String hirdetoUName = c.Users.FirstOrDefault(h => h.Id == id).UserName;// Hirdetok  helyett atirtam Users-re, mert mostmár a hirdetők nem külön entitások, csak User-ek akik "hirdeto" role-al rendelkeznek

            return GetHirdetettByHirdetoUName(hirdetoUName);
        }
        public IQueryable<ItemLicitModel> GetHirdetett2() 
        {
            IQueryable<ItemLicitModel> ret =
                ItemsWithMaxLicit()
                //a kezdőlicit vonatkozik aktuálisnak azoknál a tárgyaknál, amikre nem érkezett licit:
                .Union(
                    ItemsWithoutLicit()
                );
            return ret;
        }
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

        /*ez valamiért hibát dob:
        Licit getActualLicitFromItem(Int32 ItemId)
        {
            Licxit ret =

            c.Items.Licits.Aggregate(
                new Licit
                {
                    Value = i.StartingLicit,
                    Date = null,
                    Item = null,
                    User = null,
                    Id = -1,
                    ItemId = -1,
                    UserId = -1
                },
                (aggregate, next) => aggregate.Value > next.Value ? aggregate : next
            ).Value;
        }*/

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
    }
}
