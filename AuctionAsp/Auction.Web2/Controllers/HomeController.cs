using Auction.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Auction.Persistence.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace Auction.Controllers
{
    public class HomeController : BaseCtrl
    {
        public HomeController(HomeService _homeService/*, AccountService _accountService*/)
            : base(_homeService/*, _accountService*/)
        { }

        [HttpGet]
        public IActionResult Index(Boolean Categorical, Boolean OnlyActive, Int32? PageNum = null, String NameFilter = null)
        {
            Int32 pageSize = 20;
            PageFilterVM pf = new PageFilterVM { PageNum = PageNum, NameFilter = NameFilter, Categorical= Categorical, OnlyActive= OnlyActive };
            if (!TryValidateModel(pf))
            {
                //pf = new PageFilterVM { PageNum = 0, NameFilter = "", Categorical = false, OnlyActive = true };
                ModelState.AddModelError("", "Hibás szűrőfeltétel");
                return View("Index", pf);
            }
            //ezt is használhatom mert átláthatóbb mint a getHirdetett2 implementáció,
            //de ennél meg utólag kell megállapítani az aktuális licitet.
            //List<ItemLicitModel> last20_2 = homeService.getTopHirdetett(pf, 20).ToList();


            List<ItemLicitVM> last20 = homeService.ILMs_To_ILVMs(
                        homeService.FilterAndPageItemLicitVMs(
                            homeService.GetHirdetett2(),
                            pf,
                            pageSize
                        )
                    )
                    .ToList();

            /*List<Item> last20 = homeService.getHirdetett( 
                        homeService.FilterAndPageItems(pf, pageSize)
                    ).ToList();
            */
            ViewBag.Items = last20;
            //kérdés: hogyan csinálok textFieldet, ami a route-ba teszi a NameFiltert?
            return View("Index", pf);
        }



        //talán ha a get verziót a post verzió meghívja(redirect-eli redirectToAction-el) egy FilterObjektum alapján(amit a post verzió megkapott egy html form segítségévela klienstől),
        //            akkor a get verzió a viewbagben átadná a nézetnek a szűrőparamétert, és emelett a szűrt cuccokat jelenítené meg(modositani kell a HomeService-t ehhez)
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Index(PageFilterVM pf)
        {
            if (!TryValidateModel(pf))
            {
                ModelState.AddModelError("", "Hibás szűrőfeltétel");
                return View("Index", pf);
            }
            return RedirectToAction(nameof(Index), new { pf.Categorical, pf.OnlyActive, pf.PageNum, pf.NameFilter });
            //return RedirectToAction(nameof(Index), new { pageNum = pf.PageNum, nameFilter = pf.NameFilter });
        }

        [HttpGet]
        public IActionResult Details(Int32? ItemId)
        {
            Item item = homeService.getItem(ItemId);
            if (item == null)
            {
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ActualLicit = homeService.GetActualLicitValueForItem((Int32)ItemId, out _);
            return View("Details", item);
        }

        [HttpGet]
        [Authorize]//csak bejelentkezett felhasználóknak
        public IActionResult StartLicit(Int32? ItemId)
        {
            //if (!User.Identity.IsAuthenticated/* HttpContext.Session.GetString("user") == null*/)
            //{
            //    //ModelState.AddModelError("", "A licitáláshoz előbb jelentkezzen be!");
            //    return RedirectToAction(nameof(Index));
            //    //return RedirectToAction(nameof(StartLicit), ItemId);
            //}
            Item item = homeService.getItem(ItemId);
            if (item == null)
            {
                return RedirectToAction(nameof(Index));
            }
            //a legkisebb elfogadható licitre állitjuk be létrehozandó modell licitértékét:
            Boolean isStartingLicit;
            Int32 ActiveLicit = homeService.GetActualLicitValueForItem(item.Id, out isStartingLicit);

            Int32 nextAvaliableLicit = ActiveLicit;
            if (!isStartingLicit)
                nextAvaliableLicit++;

            LicitVM lvm = new LicitVM { Item = item, /*ItemId = item.Id, ItemName = item.Name, */Value = nextAvaliableLicit , ActualLicit = ActiveLicit };
            /*
            if (!TryValidateModel(lvm))
            {
                ModelState.AddModelError("", "Hibás kitöltés");
                return View("StartLicit", lvm);
            }
            */
            //ViewBag.ItemId = ItemId;
            return View("StartLicit", lvm);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize]
        public IActionResult CommitLicit(Int32? ItemId, LicitVM lvm)
        {
            if (ItemId == null || lvm == null)
            {
                return RedirectToAction(nameof(Index));
            }

            lvm.Item= homeService.getItem(ItemId);
            if (lvm.Item == null)
            {
                return RedirectToAction(nameof(Index));
            }
            lvm.ActualLicit = homeService.GetActualLicitValueForItem((Int32)ItemId, out _);
            //lvm.Item = (Int32)ItemId;
            //lvm.ItemName = ItemName;

            if(lvm.Item == null)
                return RedirectToAction(nameof(Index));

            if (!TryValidateModel(lvm))
            {
                ModelState.AddModelError("", "Hibás kitöltés");
                return View("StartLicit", lvm);
            }
            switch (homeService.ValidateLicit(lvm))
            {
                case LicitError.ItemIdInvalid:
                    ModelState.AddModelError("", "nem létezik ilyen azonosítójú tárgy(az elküldött objektum hibás)");
                    break;
                case LicitError.LicitIsOver:
                    ModelState.AddModelError("", "Már lezárult a tárgyra a licitálás");
                    break;
                case LicitError.LicitSmallThanPrevious:
                    ModelState.AddModelError("Value", "a megadott licit kisebb, mint a kezdőlicit!");
                    break;
                case LicitError.LicitSmallThanInitial:
                    ModelState.AddModelError("Value", "a licit nagyobb kell legyen az előző licitnél!");
                    break;
            }
            /*
            if (HttpContext.Session.GetString("user") == null)
            {
                ModelState.AddModelError("", "A licitáláshoz előbb jelentkezzen be!");
                return View("StartLicit", lvm);
                //return RedirectToAction(nameof(StartLicit), ItemId);
            }
            */
            if (!ModelState.IsValid)
                return View("StartLicit", lvm);
            String loggedinUserName = User.Identity.Name;
            if (!homeService.Licit(lvm, loggedinUserName))
            {
                ModelState.AddModelError("", "A Licitálás sikertelen, kérem próbálja újra!");
            }

            ViewBag.Message = "A Licitet sikeresen rögzítettük!";
            return View("LicitSuccess");
        }
        [HttpGet]
        [Authorize]
        public IActionResult ListLicitedByUser()
        {
            /*
            if(HttpContext.Session.GetString("user") == null)
            {
                return RedirectToAction(nameof(Index));
            }
            */
            String loggedinUserName = User.Identity.Name;
            List<ItemLicitedByUserVM> ItemsLicited = homeService.ItemsLicitedByUser(loggedinUserName).ToList();
            return View("ItemsLicitedByUser", ItemsLicited);
        }
        public FileResult ImageForItem(Int32? itId)
        {
            // lekérjük a megadott azonosítóval rendelkező képet
            Byte[] picture= homeService.GetItemImage(itId);

            if (picture == null) // amennyiben nem sikerült betölteni, egy alapértelmezett képet adunk vissza
                return File("~/images/NoImage.png", "image/png");

            return File(picture, "image/png");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
