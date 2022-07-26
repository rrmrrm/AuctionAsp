using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auction.WebApi.Tests
{
    /// <summary>
    /// próbálkozás a signinmanager működésre bírására;, identity options osztály a bejelentkezési adatok megszabásához
    /// </summary>
    public class MyIdentityOptions : IOptions<IdentityOptions>
    {
        IdentityOptions iopt { get; set; }
        public MyIdentityOptions()
        {
            iopt = new IdentityOptions();
            // Jelszó komplexitására vonatkozó konfiguráció
            iopt.Password.RequiredLength = 6;
            iopt.Password.RequireNonAlphanumeric = false;
            iopt.Password.RequireLowercase = false;
            // Felhasználókezelésre vonatkozó konfiguráció
            iopt.User.RequireUniqueEmail = true;
        }

        IdentityOptions IOptions<IdentityOptions>.Value => iopt;
    }
}
