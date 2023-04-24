using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Auction.Persistence
{
    //TODO: szerveroldalon is ellenőrizni kell a modelleket
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "A név megadása kötelező")]
        [StringLength(200, ErrorMessage = "A név legfeljebb 200 karakter lehet")]
        public String Name { get; set; }

        [Required(ErrorMessage = "A telefonszám megadása kötelező")]
        [Phone(ErrorMessage = "A telefonszám rossz formátumú")]
        public String Phone { get; set; }

        [Required(ErrorMessage = "A felhassználóév megadása kötelező")]
        [StringLength(200, ErrorMessage = "A név legfeljebb 200 karakter lehet")]
        public String UserName { get; set; }

        [Required(ErrorMessage = "A jelszó megadása kötelező")]
        [RegularExpression("^[a-zA-Z1-9 _0-]{6,50}$", ErrorMessage = "A jelszó formátuma nem megfelelő: minimum 6, maximum 50 karakter hosszú lehet. A jelszó csak az angol ábécé kis -és nagy- betűit, szóközt, számokat és a '-' és '_' karaktereket tartalmazhatja")]
        [DataType(DataType.Password)]
        public String Password { get; set; }

        [Required(ErrorMessage = "Megerősítő jelszó megadása kötelező")]
        [Compare(nameof(Password), ErrorMessage = "A két jelszó nem egyezik")]
        [DataType(DataType.Password)]
        public String ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Az e-mail cím megadása kötelező")]
        [EmailAddress(ErrorMessage = "az e-mail cím rossz formátumú")]
        [DataType(DataType.EmailAddress)]
        public String Email { get; set; }
    }
}
