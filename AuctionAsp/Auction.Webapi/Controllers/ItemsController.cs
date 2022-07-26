using Auction.Persistence;
using Auction.Persistence.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Auction.Webapi.Controllers
{
    /// <summary>
    /// A hirdetőknek szolgáltatja a meghirdetett tárgyaikat
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))] // OpenAPI konvenciók alkalmazása az akciók által visszaadható HTTP státusz kódokra
    public class ItemsController : Controller
    {

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }


        private readonly UserManager<User> userManager;
        private readonly ApiService serv;

        /// <summary>
        /// Vezérlő példányosítása.
        /// </summary>
        /// <param name="_serv">ApiService az adatbázis kezeléséhez.</param>
        /// <param name="_userManager">Bejelentkezett felhasználó lekérdezéséhez.</param>
        public ItemsController(UserManager<User> _userManager, ApiService _serv)
        {
            if (_serv == null)
                throw new ArgumentNullException(nameof(_serv));
            serv = _serv;

            if (_userManager == null)
                throw new ArgumentNullException(nameof(_userManager));
            userManager = _userManager;
        }

        /// <summary>
        /// a hirdető által hirdetett tárgyak listázása
        /// </summary>
        /// <returns></returns>
        [HttpGet("hirdetett")]
        [Authorize(Roles = "hirdeto")] // csak hirdetőknek
        public ActionResult<IEnumerable<ItemLicitDTO>> GetHirdetett()
        {
            try
            {
                String hirdetoName = User.Identity.Name;
                IList<ItemLicitDTO> ret = serv.GetHirdetettByHirdetoUName(hirdetoName).ToList();
                return Ok(ret);
            }
            catch
            {
                // Internal Server Error
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        /// <summary>
        ///  a felhasználó lekérheti az általa meghirdetett egyik tárgyat(kategóriával és képpel) azonosító alapján.
        ///  ha nem ő hirdette, akkor unauthorized státusszal térünk vissza
        /// </summary>
        /// <param name="id"></param>
        [HttpGet("{id}")]
        [Authorize(Roles = "hirdeto")] // csak hirdetőknek
        public ActionResult<ItemDTO> GetItem(Int32 id)
        {
            //ha nem létezik a tárgy, akkor 404-el térünk vissza:
            if (serv.GetItemById(id) == null)
            {
                return NotFound();
            }
            //a felhasználó csak az általa hirdetett tárgyhoz láthatja a licitálókat:
            if (!serv.IsUserHirdetoOf(User.Identity.Name, id))
            {
                return Unauthorized("Csak az on altal hirdetett targyat kerheti le");
            }

            try
            {
                ItemDTO itemDTO = serv.GetItemWithCategAndHirdeto(id);
                return Ok(itemDTO);
            }
            catch(DbUpdateException)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// kategóriák
        /// </summary>
        /// <param name="id"></param>
        [HttpGet("Categories")]
        public async Task< ActionResult<List<CategoryDTO>> > GetCategories()
        {
            try
            {
                List<CategoryDTO> categories = serv.GetCategories().ToList();
                return Ok(categories);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// a megadott ayonosítóval rendelkező tárgy részletes adatai képpel, és
        ///  az összes rá érkezett licittel(dátum, név, összeg) együtt.
        /// </summary>
        /// <param name="id"></param>
        [HttpGet("details/{id}")]
        [Authorize(Roles = "hirdeto")] // csak hirdetőknek
        public async Task< ActionResult<ItemAllLicitsDTO> > GetItemDetails(Int32 id)
        {
            //ha nem létezik a tárgy, akkor 404-el térünk vissza:
            if (serv.GetItemById(id) == null)
            {
                return NotFound();
            }
            //a felhasználó csak az általa hirdetett tárgyhoz láthatja a licitálókat:
            if (!serv.IsUserHirdetoOf(User.Identity.Name, id))
            {
                return Unauthorized("Csak az on altal hirdetett targyakathoz nezheti meg a szavazokat");
            }
            try
            {
                ItemAllLicitsDTO itemLicits = serv.GetItemWithAllLicits(id);
                return Ok(itemLicits);
            }
            catch (InvalidOperationException){//.Single() dobhatná br ezt már az if(serv.GetItemById(id) == null) -al lekezeltük, úgyhogy ez a catch() elméletileg törölhető
                return NotFound();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        // POST api/<ItemsController>
        /// <summary>
        /// uj targy felvitéle(tnev,kategoria,leiras,kezdlicit,lezarDatum, kep, +kategoria+hirdeto)
        /// </summary>
        /// <param name="itemDto"></param>
        [HttpPost]
        [Authorize(Roles = "hirdeto")] // csak hirdetőknek
        public async Task< ActionResult<ItemDTO> > PostItem([FromBody] ItemDTO itemDto)
        {
            ///TODO: validációs hibákat adni vissza (new new BadRequestObjectResult(modelstate)-al),
            ///és a túloldalon felhasználni ezeket
            bool goodState = true;
            if (itemDto.StartingLicit < 1)
            {
                ModelState.AddModelError(nameof(itemDto.StartingLicit), "a licit nem lehet 1-nél kisebb");
                goodState = false;
            }
            if (itemDto.DateOfClosing <= DateTime.Now.AddDays(1))
            {
                ModelState.AddModelError(nameof(itemDto.DateOfClosing), "a zárás dátuma legkorábban mosttól 24óra múlva kell, legyen");
                goodState = false;
            }
            if (itemDto.Category is null)
            {
                ModelState.AddModelError(nameof(itemDto.Category), "a licit nem lehet 1-nél kisebb");
                goodState = false;
            }
            if (!serv.IsCategoryExists(itemDto.Category.Id))
            {
                ModelState.AddModelError(nameof(itemDto.Category), $"nincs {itemDto.Category.Id} azonosítójú kategória");
                goodState = false;
            }
            if (!goodState) {
                ModelState.AddModelError("hibás a küldött tárgy", "");
                return new BadRequestObjectResult(ModelState);
            }
            //TODO: a category-t, picture-t, majd az adminisztrációs kliensnek ki kell vlasztania bele kell tennie az ItemDTO-ba
            try
            {
                //serv.c.Users.Find();
                ////MODIFIED
                User hirdeto = await userManager.FindByNameAsync(User.Identity.Name);
                Item newItem = new Item
                {
                    AuctionStartDate = DateTime.Now,
                    Name = itemDto.Name,
                    Picture = itemDto.Picture,
                    StartingLicit = itemDto.StartingLicit,
                    DateOfClosing = itemDto.DateOfClosing,
                    Description = itemDto.Description,
                    //Id = itemDto.Id,
                    Licits = new HashSet<Licit>(),
                    //Category            = itemDto.Category,
                    CategoryId = itemDto.Category.Id,
                    UserId = hirdeto.Id,
                };
                serv.addItem(newItem);//throws

                // itt nem a bejövő itemDTO-t adjuk vissza,
                // hanem az adatbázis által feltöltött idvel és itt feltöltött kezdődátummal ellátott newItem-et ItemDTO-vá konvertálva
                return CreatedAtAction(nameof(GetItem), new { id = newItem.Id }, (ItemDTO)newItem);
            }
            catch
            {
                // Internal Server Error
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }
        /// <summary>
        ///  a licit azonnali lezárására(csak ha valaki már licitált rá, ekkor az aktuális licitáló viszi a tárgyat)
        ///  Ha a tárgy más felhasználóé, vagy id-je nem található -> Unathorized("nem ön targya"),
        ///  Ha nem licitaltak ra -> Forbidden,"Hiba: nem licitaltak meg ra"
        ///  Ha adatbazis frissitese sikertelen -> 500internalserverError
        /// </summary>
        /// <param name="itemDTO"></param>
        [HttpPut("closeLicit")]
        [Authorize(Roles = "hirdeto")] // csak hirdetőknek
        public ActionResult<ItemAllLicitsDTO> CloseLicit(ItemAllLicitsDTO itemDTO)
        {
            //ha nem létezik a tárgy 404-et küldünk vissza:
            Item item = serv.GetItemById(itemDTO.item.Id);
            if (item is null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "Nem létezik ilyen tárgy");
            }
            //a felhasználó csak az általa hirdetett licitet zárhatja le:
            if (item.AuctionStartDate > DateTime.Now || item.DateOfClosing < DateTime.Now)
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed, "A licit még nem indult el, vagy már lezárult");
            }
            //a felhasználó csak a folyamatban levő licitálásokat zárhatja le:
            if (!serv.IsUserHirdetoOf(User.Identity.Name, itemDTO.item.Id))
            {
                return Unauthorized("Csak az on altal hirdetett licitalast yzrhatja le");
            }

            //ha nem licitáltak még a tárgyra,, akkor jelezzük ezt a kliens felé:
            if (serv.ItemsWithoutLicit().Select(il => il.ItemId).Contains(itemDTO.item.Id))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Hiba: nem licitaltak meg a targyra, ezert nem zarhato le");
            }
            try
            {
                //az item-ben kitöltjük a winner property-t:
                ItemAllLicitsDTO patchedItem = serv.CloseLicit(itemDTO.item.Id);
                return Ok(patchedItem);
            }

            catch (DbUpdateException e)// c.SaveChanges - sikertelen mentes az adatbazisban
            {
                // Internal Server Error
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch(Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}

/* kelleni fog:
    hirdeto bejelntkeyik, kijelntekezik,

    bejelentkezett hirdeto targyakat nezi{ld: ITEMS}

    uj targy felvitele(tnev,kategoria,leiras,kezdlicit,lezarDatum, KEP IS FELTOLTHETO!, +kategoria+hirdeto(.Web-el valo compatiblitashoz))
        ehhez a category-t majd az adminisztrációs kliensnek ki kell vlasztania bele kell tennie az ItemDTO-ba, és aztán ezt elküldenie ennek a kontrollernek

    ITEMS: tárgyak listazasa:[minden tárgy, lezarrasiIdo szrinti Sorrendben, AKTUALISLICIT], EMELLETT: LINK az alabbi funckiora{id alapján? kivalasztottItem}:
        minden info a {kivalasztottItem} targyrol, ÉS ÖSSZES {} LICIT (date,uName, összeg) LISTÁJA, + !ha valaki már licitált {kivTárgy} tárgyra!: LINK az alabbi funkció{lezarando targyId}:
            lehetőségünk van a licit azonnalilezárására (csak ha valaki már licitált rá, ekkor az aktuális licitáló viszi a tárgyat) 

*/