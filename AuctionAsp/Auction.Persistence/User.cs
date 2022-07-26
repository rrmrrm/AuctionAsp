using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Security;
using Microsoft.AspNetCore.Identity;
//vhogz minden ami a .web projectbe volt atkerult es valami syar
namespace Auction.Persistence
{
    /// TODO: identity keyelés a példaprogik alapján Hirdeto es Userben is kell valszeg
    public class User : IdentityUser<int>
    {

        public String Name { get; set; }
        /* A korábban definiált tulajdonságok közül az IdentityUser<T> tartalmazza:
		 * T Id
		 * string UserName
		 * string PasswordHash (UserPassword helyett)
		 * string Email
		 * string PhoneNumber
		 * string SecurityStamp (UserChallenge helyett)
		 */
        /*
        virtual public ICollection<AssignedUser> assignedUsers { get; set; }

        public User()
        {
            assignedUsers = new HashSet<AssignedUser>();
        }
        */
    }
}
