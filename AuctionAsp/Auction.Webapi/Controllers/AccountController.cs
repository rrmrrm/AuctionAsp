
using Auction.Persistence;
using System;
using System.Threading.Tasks;
using Auction.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Auction.Webapi.Controllers
{
	/// <summary>
	/// Felhasználókezelést biztosító vezérlő.
	/// </summary>
	[Route("api/[controller]")]
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class AccountController : Controller
    {

        /// <summary>
        /// Authentikációs szolgáltatás.
        /// </summary>
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }

        /// <summary>
        /// Vezérlő példányosítása.
        /// </summary>
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Bejelentkezés.
        /// </summary>
        /// <param name="loginDTO">Bejelentkezési adatok.</param>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                // bejelentkeztetjük a felhasználót
                var result = await _signInManager.PasswordSignInAsync(loginDTO.UserName, loginDTO.Password, false, false);
	            if (!result.Succeeded) // ha nem sikerült, akkor nincs bejelentkeztetés
		            return Unauthorized();
                //User.Identity./////////////////////////////////////////////////////////
				// ha sikeres volt az ellenőrzés
                return Ok();
            }
            catch(Exception e)
            {
				// Internal Server Error
	            return StatusCode(StatusCodes.Status500InternalServerError);
			}
        }
        
        /// <summary>
        /// Kijelentkezés.
        /// </summary>
        [HttpGet("Logout")]////MODIFIED
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
				// kijelentkeztetjük az aktuális felhasználót
				await _signInManager.SignOutAsync();
				return Ok();
            }
            catch
            {
				// Internal Server Error
	            return StatusCode(StatusCodes.Status500InternalServerError);
			}
        }
    }
}
