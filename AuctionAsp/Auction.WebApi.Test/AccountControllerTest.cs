using Auction.Persistence;
using Auction.Persistence.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Auction.Webapi.Controllers;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Auction.Webapi.Tests;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Auction.WebApi.Tests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace Auction.Webapi.Tests
{
    public class AccountControllerTest : IDisposable
    {
        private readonly AuctionContext context;
        //private readonly AuctionContextM _contextM;
        private readonly ApiService _service;
        private  AccountController _controller;

        public AccountControllerTest()
        {

            /*
            var options = new DbContextOptionsBuilder<AuctionContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            var options = new DbContextOptionsBuilder<AuctionContextM>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            _contextM = new AuctionContextM(options);
            TestDbInitializer.Initialize(_contextM);
            
            var userManager = new UserManager<UserM>(
                new UserStore<UserM>(_contextM), null,
                new PasswordHasher<UserM>(), null, null, null, null, null, null);

            var user = new UserM { UserName = "testName", Id = "testId" };
            */

            
            var options = new DbContextOptionsBuilder<AuctionContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            context = new AuctionContext(options);
            TestDbInitializer.Initialize(context);


            //class UserStore<TUser> : UserStore<TUser, IdentityRole, DbContext, string> where TUser : IdentityUser<string>, new()
            
            var userManager = new UserManager<User>(
                store: new UserStore<User, IdentityRole<int>, AuctionContext, int>(context), null,
                new PasswordHasher<User>(), null, null, null, null, null, null);
           

            // Use only one of the below.
            // Create / update database based on migration classes.
            //context.Database.Migrate();
            // Create database if not exists based on current code-first model (no migrations!).
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            if (context.Items.Any())
            {

            }
            else
            {
                    var u1 = new User
                    {//hirdeto
                        Id = 1,
                        Name = "Neil Collings",
                        PhoneNumber = "06302225555",
                        UserName = "neil1234",
                        Email = "neil.collings@gmail.com",
                    };
                    var u2 = new User
                    {//hirdeto
                        Id = 2,
                        Name = "Bruce Castor",
                        PhoneNumber = "06301234567",
                        UserName = "bruce11",
                        Email = "brucecastor@gmail.com"
                    };
                    var u3 = new User
                    {
                        Id = 4,
                        Name = "Arthur Atkinson",
                        PhoneNumber = "06302225555",
                        UserName = "arthurkatkinson",
                        Email = "arthur@gmail.com"
                    };
                    var u4 = new User
                    {
                        Id = 5,
                        Name = "Boba Fett",
                        PhoneNumber = "06223344556",
                        UserName = "bobafett11",
                        Email = "bobafett@gmail.com"
                    };

                    /*
                    var result11 = _userManager.CreateAsync(u1, "Almafa123").Result;
                    var r1 = _userManager.CreateAsync(u2, "Password123").Result;
                    var r12 = _userManager.CreateAsync(u3, "Almafa123").Result;
                    var r2 = _userManager.CreateAsync(u4, "Password345").Result;
                    //var r4 = _userManager.CreateAsync(u4, "password4").Result;
                    context.SaveChanges();

                    // hirdetõk felvétele:
                    //az elsõ 2 felhasználó hirdetõ role-t kap, így õk hirdethetnek majd tárgyakat is
                    //var hirdetoRole = new IdentityRole<int>("hirdeto");
                    var res = _roleManager.CreateAsync(adminRole).Result;

                    var result1 = _userManager.AddToRoleAsync(u1, adminRole.Name).Result;
                    var result2 = _userManager.AddToRoleAsync(u2, adminRole.Name).Result;
                    context.SaveChanges();
                    */

                    ICollection<Category> defaultCategories = new List<Category> {
                        new Category{Name = "bútor"},
                        new Category{Name = "ingatlan"},
                        new Category{Name = "egyéb"}
                    };
                    foreach (var c in defaultCategories)
                    {
                        context.Categories.Add(c);
                    }
                    context.SaveChanges();

                    //List<User> users = _userManager.GetUsersInRoleAsync("hirdeto").Result.ToList();
                    ICollection<Item> defaultItems = new List<Item>
                    {
                    new Item{
                        Id = 1,
                        AuctionStartDate = new DateTime(2021,1,1),
                        Category = defaultCategories.ElementAt(0),
                        DateOfClosing = new DateTime(2021,3,1),
                        Name="IKEA schrössargbáur összecsukható szekrény",
                        Description="Ez egy nem létezõ szekrény.",
                        UserId=u1.Id,
                        StartingLicit=20000,
                        Picture =null,
                    },
                    new Item{
                        Id = 2,
                        AuctionStartDate = new DateTime(2021,3,2),
                        Category = defaultCategories.ElementAt(0),
                        DateOfClosing = new DateTime(2021,6,2),
                        Name="randomFotelNév fotel",
                        Description="Ez egy fotel.",
                        UserId=u1.Id,
                        StartingLicit=8000,
                        Picture = null,
                    },

                    new Item{
                        Id = 3,
                        AuctionStartDate = new DateTime(2021,6,2),
                        Category = defaultCategories.ElementAt(0),
                        DateOfClosing = new DateTime(2021,7,2),
                        Name="Boholmen mosdókagyló",
                        Description="Ez egy rozsdamemtes fém konyhai mosdókagyló.",
                        UserId=u2.Id,
                        StartingLicit=4000,
                        Picture = null//File.ReadAllBytes(Path.Combine(imageDirectory, "p3.png")),
                    },
                    new Item{
                        Id = 4,
                        AuctionStartDate = new DateTime(2021,1,2),
                        Category = defaultCategories.ElementAt(0),
                        DateOfClosing = new DateTime(2021,6,2),
                        Name="IKEA Einherjer ruhaszárító",
                        Description="Az Einherjer amúgy metálzenekar, nem szárító.",
                        UserId=u2.Id,
                        StartingLicit=4000,
                        Picture = null
                    },


                    new Item{
                        Id = 5,
                        AuctionStartDate = new DateTime(2021,1,2),
                        Category = defaultCategories.ElementAt(1),
                        DateOfClosing = new DateTime(2021,6,2),
                        Name="Eladó 3 szobás lakás Budapest belvárosban",
                        Description="részletek a https:ne.keresd linken",
                        UserId=u2.Id,
                        StartingLicit=90000000,
                        Picture = null
                    },
                    new Item{
                        Id = 6,
                        AuctionStartDate = new DateTime(2021,1,2),
                        Category = defaultCategories.ElementAt(1),
                        DateOfClosing = new DateTime(2021,6,2),
                        Name="neil valamilye",
                        Description="részletes leiras",
                        UserId=u1.Id,
                        StartingLicit=8000,
                        Picture = null
                    }

                };
                foreach (var item in defaultItems)
                {
                    context.Items.Add(item);
                }
                context.SaveChanges();

                    //List<User> defaultUsers = context.Users.ToList();
                    ICollection<Licit> defaultLicits = new List<Licit> {
                    new Licit{Item=defaultItems.ElementAt(0), Date=defaultItems.ElementAt(0).AuctionStartDate.AddDays(1), UserId=1, Value=20001},
                    new Licit{Item=defaultItems.ElementAt(0), Date=defaultItems.ElementAt(0).AuctionStartDate.AddDays(2), UserId=2, Value=20002},
                    new Licit{Item=defaultItems.ElementAt(0), Date=defaultItems.ElementAt(0).AuctionStartDate.AddDays(3), UserId=3, Value=20003},

                    new Licit{Item=defaultItems.ElementAt(0), Date=defaultItems.ElementAt(0).AuctionStartDate.AddDays(4), UserId=4, Value=20004},
                    new Licit{Item=defaultItems.ElementAt(0), Date=defaultItems.ElementAt(0).AuctionStartDate.AddDays(5), UserId=4, Value=20005},

                    new Licit{Item=defaultItems.ElementAt(1), Date=defaultItems.ElementAt(1).AuctionStartDate.AddDays(1), UserId=1, Value=8001},
                    new Licit{Item=defaultItems.ElementAt(1), Date=defaultItems.ElementAt(1).AuctionStartDate.AddDays(2), UserId=2, Value=8002},
                    new Licit{Item=defaultItems.ElementAt(1), Date=defaultItems.ElementAt(1).AuctionStartDate.AddDays(3), UserId=1, Value=8003},
                    new Licit{Item=defaultItems.ElementAt(1), Date=defaultItems.ElementAt(1).AuctionStartDate.AddDays(4), UserId=3, Value=8004},
                    new Licit{Item=defaultItems.ElementAt(1), Date=defaultItems.ElementAt(1).AuctionStartDate.AddDays(5), UserId=4, Value=8005},
                    new Licit{Item=defaultItems.ElementAt(1), Date=defaultItems.ElementAt(1).AuctionStartDate.AddDays(6), UserId=4, Value=8006}
                };
                    foreach (var l in defaultLicits)
                    {
                        context.Licits.Add(l);
                    }


                userManager.CreateAsync(u1, "testPassword").Wait();
                userManager.CreateAsync(u2, "testPassword").Wait();
                userManager.CreateAsync(u3, "testPassword").Wait();
                userManager.CreateAsync(u4, "testPassword").Wait();

                context.SaveChanges();
            }

            _service = new ApiService(context);
            HttpContextAccessor hacc = new HttpContextAccessor();
            IdentityOptions _iopt = new IdentityOptions();
            _iopt.User.RequireUniqueEmail = false;
            /*            services.Configure<IdentityOptions>(options =>
                        {
                            // Jelszó komplexitására vonatkozó konfiguráció
                            //options.Password.RequireDigit = true;
                            options.Password.RequiredLength = 6;
                            options.Password.RequireNonAlphanumeric = false;
                            //options.Password.RequireUppercase = true;
                            options.Password.RequireLowercase = false;
                            //options.Password.RequiredUniqueChars = 3;

                            //// Hibás bejelentkezés esetén az (ideiglenes) kizárásra vonatkozó konfiguráció
                            //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                            //options.Lockout.MaxFailedAccessAttempts = 10;
                            //options.Lockout.AllowedForNewUsers = true;

                            // Felhasználókezelésre vonatkozó konfiguráció
                            options.User.RequireUniqueEmail = true;
                        });*/
           
            


            //var user = new ApplicationUser { UserName = "testName", Id = "testId" };
            //userManager.CreateAsync(user, "testPassword").Wait();

            setControllerAndClaims(new List<Claim>{
                new Claim(ClaimTypes.Name, "neil1234"),
                new Claim(ClaimTypes.NameIdentifier, "testId"),
                new Claim(ClaimTypes.Role, "hirdeto"),
            });


            /*
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Name, "testName"),
                new Claim(ClaimTypes.NameIdentifier, "testId"),
            });
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
            */
        }
        
        private void setControllerAndClaims(List<Claim> claims)
        {
            var userManager = new UserManager<User>(
                  store: new UserStore<User, IdentityRole<int>, AuctionContext, int>(context), null,
                  new PasswordHasher<User>(), null,null, null, null, null, null);

            var claimsIdentity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            HttpContext httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            HttpContextAccessor h = new HttpContextAccessor { HttpContext = httpContext };

            MyIdentityOptions mi = new MyIdentityOptions();

            UserClaimsPrincipalFactory<User> fact = new Microsoft.AspNetCore.Identity.UserClaimsPrincipalFactory<User>(
                    userManager, mi
                );



            var serviceProvider = new ServiceCollection()
            .AddLogging(b => b.AddDebug())
            .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();

            var logger = factory.CreateLogger<SignInManager<User>>();



            /*var mock = new Mock<ILogger<SignInManager<User>>>();
            ILogger<SignInManager<User>> logger = mock.Object;

            //or use this short equivalent 
            logger = Mock.Of<ILogger<SignInManager<User>>>();
            //var controller = new BlogController(logger);*/

            //LoggerFactoryExtensions.CreateLogger<SignInManager<User>>();
            //var _loggerFactory = LoggerFactoryExtensions(); NullLoggerFactory.Instance;
            //var _logger = _loggerFactory.CreateLogger<SignInManager<User>>();
            //Logger<SignInManager<User>> logger = new Logger<SignInManager<User>>(new LoggerFactory());
            var signinManager = new SignInManager<User>(
                    userManager, 
                    h, 
                    fact,
                    mi,
                    logger,
                    null,
                    null
                );

            _controller = new AccountController(userManager, signinManager);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
        public void Dispose()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }

        //nem tudom a signinmanagert mûködésre bírni
        [Fact]
        public void LoginTest()
        {
            var objRes = Assert.IsAssignableFrom<OkResult>(_controller.Login(
                    new LoginDTO { UserName = "neil1234", Password = "Almafa" }
                ));
            int i = 0;
        }
    }
}