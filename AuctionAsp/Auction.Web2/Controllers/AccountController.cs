using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Auction.Persistence.Services;
using Auction.Persistence;
using Microsoft.AspNetCore.Identity;

namespace Auction.Controllers
{
    public class AccountController : BaseCtrl
    {

        private readonly SignInManager<User> sManager;
        private readonly UserManager<User> uManager;
        public AccountController(HomeService _homeService,
            SignInManager<User> _sManager,
            //signin -es usermanagert hasznalok ehelyett mar: AccountService _accountService,
            UserManager<User> _uManager)
            : base(_homeService/*, _accountService*/)
        {
            sManager = _sManager;
            uManager = _uManager;
       }
        /*
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            //lehet ilyesmire nekem is szükségem lesz: ViewBag.vmi = vmiService.vmi.ToArray();
        }
        */

        [HttpGet]
        public IActionResult Login()
        {
            return View("Login");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel user)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", user);
            }
            var result = await sManager.PasswordSignInAsync(user.UserName, user.Password, user.RememberLogin, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Hibás felhasználónév vagy jelszó");
                return View("Login", user);
            }
            //a sManagere helzett-nk a munkamenetbe felvesszi felhasználót:
            //HttpContext.Session.SetString("user", user.UserName);

            ViewBag.email = user.UserName;
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View("Register");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationViewModel userM)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", userM);
            }
            User user= new User
            {
                UserName = userM.UserName,
                Email = userM.Email,
                Name = userM.Name,
                PhoneNumber = userM.Phone,
            };
            var result = await uManager.CreateAsync(user, userM.Password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err.Description);
                //ezt csereltem le az elozo sorokra: lecserModelState.AddModelError("UserName", "A megadott felhasználónév már használatban van.");
                return View("Register", userM);
            }

            /*signinM vegzi mostmar a ki es bejelentkeztetest:
            //ha már be volt jelentkezve egy felhasználó, akkor kijelentkeztetjük:
            if(HttpContext.Session.GetString("user") != null)
            {
                HttpContext.Session.Remove("user");
            }
            */
            // be is jelentkeztetjük egyből a felhasználót:
            await sManager.SignInAsync(user, false);
            return RedirectToAction("Login");
        }
        public async Task<IActionResult> Logout()
        {
            //userM vvegzi el:
            //ha be volt jelentkezve
            //accService.Logout();
            await sManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
