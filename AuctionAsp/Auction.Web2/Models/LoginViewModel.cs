using System;
using System.ComponentModel.DataAnnotations;

namespace Auction.Persistence
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "A felhassználóév megadása kötelező")]
        [StringLength(200, ErrorMessage = "A név legfeljebb 200 karakter lehet")]
        public String UserName { get; set; }

        [Required(ErrorMessage = "A jelszó megadása kötelező")]
        [RegularExpression("^[a-zA-Z1-9 _0-]{6,50}$", ErrorMessage = "A jelszó formátuma nem megfelelő: minimum 6, maximum 50 karakter hosszú lehet. A jelszó csak az angol ábécé kis -és nagy- betűit, szóközt, számokat és a '-' és '_' karaktereket tartalmazhatja")]
        [DataType(DataType.Password)]
        public String Password { get; set; }
        /// <summary>
        /// Bejelentkezés megjegyzése.
        /// </summary>
        public Boolean RememberLogin { get; set; }
	}
}