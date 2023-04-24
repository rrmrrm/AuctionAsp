 using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auction.Persistence;
using Auction.Persistence.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Auction.Controllers
{
    public class BaseCtrl : Controller
    {
        protected readonly HomeService homeService;
        //protected readonly AccountService accService;
        public BaseCtrl(HomeService _homeService/*, AccountService _accService*/)
        {
            homeService = _homeService;
            //accService = _accService;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            ViewBag.CurrentUserName = String.IsNullOrEmpty(User.Identity.Name) ? null : User.Identity.Name;
        }
    }
}
