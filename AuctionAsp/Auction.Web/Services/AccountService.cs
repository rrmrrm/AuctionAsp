using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Auction.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

/// TODO: a mintabedando megmutatja, hogyan lehet szepen szamontartani a bejelentkezett user emailcímét és azt megjeleníteni minden oldalon(bár megjeleniteni az emailcimet asszem asszem nem elvárt
namespace Auction.Persistence.Services
{
    public class AccountService
    {
        private readonly AuctionContext context;
        private readonly HttpContext httpContext;
        //private readonly ApplicationState applicationState;
        public AccountService(AuctionContext c, IHttpContextAccessor httpContextAccessor/*, ApplicationState _applicationState*/)
        {
            context = c;
            httpContext = httpContextAccessor.HttpContext;

            // ha a felhasználónak van sütije, de még nincs bejelentkezve, bejelentkeztetjük
            if (httpContext.Request.Cookies.ContainsKey("user_challenge") &&
                !httpContext.Session.Keys.Contains("user"))
            {
                User guest = context.Users.FirstOrDefault(
                    g => g.UserChallenge == httpContext.Request.Cookies["user_challenge"]);
                // kikeressük a felhasználót
                if (guest != null)
                {
                    httpContext.Session.SetString("user", guest.UserName);
                    // felvesszük a felhasználó nevét a munkamenetbe

                    //UserCount++; // növeljük a felhasználószámot
                }
            }
            //applicationState = _applicationState;

        }
        public Boolean Login(LoginViewModel user)
        {
            if(user == null)
            {
                return false;
            }
            //validáció:

            //data annotációk ellenőrzése:
            if (!Validator.TryValidateObject(user, new ValidationContext(user, null, null), null))
            {
                return false;
            }

            //autentikáció:

            //megnézzük regisztrálva van-e az e-mail cím:
            User foundUser = context.Users.FirstOrDefault(c => c.UserName == user.UserName);
            if(foundUser == null)
            {
                return false;
            }
            Byte[] passwordBytes = null;
            using (SHA512CryptoServiceProvider provider = new SHA512CryptoServiceProvider())
            {
                passwordBytes = provider.ComputeHash(Encoding.UTF8.GetBytes(user.Password));
            }
            if(passwordBytes == null)
            {
                return false;
            }

            if (!passwordBytes.SequenceEqual(foundUser.Password))
            {
                return false;
            }
            httpContext.Session.SetString("user", user.UserName);
            
            if (user.RememberLogin)
            {
                httpContext.Response.Cookies.Append("user_challenge", foundUser.UserChallenge,
                    new CookieOptions
                    {
                        Expires = DateTime.Today.AddDays(365),
                        HttpOnly = true
                    });
            }
            //UserCount++;
            return true;
        }
/*
public Int32 UserCount
{
    get => (Int32)applicationState.UserCount;
    set => applicationState.UserCount = value;
}
*/
        public String CurrentUserName => httpContext.Session.GetString("user");
        public Boolean Logout()
        {
            if (!httpContext.Session.Keys.Contains("user"))
            {
                return false;
            }
            httpContext.Session.Remove("user");
            httpContext.Response.Cookies.Delete("user_challenge");
            //UserCount--;
            return true;
        }
        public Boolean Register(RegistrationViewModel user)
        {
            if(user == null)
            {
                return false;
            }
            if (!Validator.TryValidateObject(user, new ValidationContext(user, null, null), null))
            {
                return false;
            }
            if(context.Users.Count(c => c.UserName == user.UserName) != 0)//már regisztrálva van a UserName
            {
                return false;
            }
            Byte[] passwordBytes = null;

            using (SHA512CryptoServiceProvider provider = new SHA512CryptoServiceProvider())
            {
                passwordBytes = provider.ComputeHash(Encoding.UTF8.GetBytes(user.Password));
            }
            if (passwordBytes == null)
            {
                return false;
            }
            context.Users.Add( new User 
            {
                Phone = user.Phone,
                Name = user.Name,
                UserName = user.UserName,
                Email = user.Email,
                Password = passwordBytes,
                UserChallenge = Guid.NewGuid().ToString()
            });
            try
            {
                context.SaveChanges();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
