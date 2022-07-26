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
using Auction.Webapi.Tests;
using System.Threading.Tasks;

namespace Auction.Webapi.Tests
{
    public class ItemsControllerTest : IDisposable
    {
        private readonly AuctionContext context;
        //private readonly AuctionContextM _contextM;
        private readonly ApiService _service;
        private  ItemsController _controller;

        public ItemsControllerTest()
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

            var userManager = new UserManager<User>(
                store: new UserStore<User, IdentityRole<int>, AuctionContext, int>(context), null,
                new PasswordHasher<User>(), null, null, null, null, null, null);

            TestDbInitializer.Initialize(context, userManager);

            _service = new ApiService(context); 

            _controller = new ItemsController(userManager, _service);


            //var user = new ApplicationUser { UserName = "testName", Id = "testId" };
            //userManager.CreateAsync(user, "testPassword").Wait();

            setClaims(new List<Claim>{
                new Claim(ClaimTypes.Name, "neil1234"),
                new Claim(ClaimTypes.NameIdentifier, "testId"),
                new Claim(ClaimTypes.Role, "hirdeto"),
            });
        }
        private void setClaims(List<Claim> claims)
        {
            var claimsIdentity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
        }
        public void Dispose()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }
        #region region GetHirdetettTests
        /// <summary>
        /// elv�rt: visszakapjuk neil1234 �ltal hirdetett 3 darab t�rgyat
        /// </summary>
        [Fact]
        public void GetHirdetettTest1()
        {
            // Act
            var result = _controller.GetHirdetett();

            // Assert
            var objRes = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
            var content = Assert.IsAssignableFrom<IEnumerable<ItemLicitDTO>>(objRes.Value);
            Assert.Equal(3, content.Count());
        }

        /// <summary>
        /// elv�rt: visszakapjuk bruce11 �ltal hirdetett 4 darab t�rgyat
        /// </summary>
        [Fact]
        public void GetHirdetettTest2()
        {
            //
            setClaims(new List<Claim>{
                new Claim(ClaimTypes.Name, "bruce11"),
                new Claim(ClaimTypes.NameIdentifier, "testId2"),
                new Claim(ClaimTypes.Role, "hirdeto"),
            });

            // Act
            var result = _controller.GetHirdetett();

            // Assert
            var objRes = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
            var content = Assert.IsAssignableFrom<IEnumerable<ItemLicitDTO>>(objRes.Value);
            Assert.Equal(4, content.Count());
        }

        /// <summary>
        /// elv�rt: a neil1234 �ltal meghirdetett 3 darab t�rgyhoz tartoz� aktu�lis licitet visszaadja a GetHirdetett(), amik rendre: 20005 8006 8000
        /// </summary>
        [Fact]
        public void GetHirdetettTest3()
        {

            // Act
            var result = _controller.GetHirdetett();

            // Assert
            var objRes = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
            var content = Assert.IsAssignableFrom<IEnumerable<ItemLicitDTO>>(objRes.Value);
            Assert.Equal(3, content.Count());
            Assert.Equal(20005, content.ElementAt(0).ActiveLicit);
            Assert.Equal(8006, content.ElementAt(1).ActiveLicit);
            Assert.Equal(8000, content.ElementAt(2).ActiveLicit);//erre a t�rgyra nem �rkezett licit, ez�rt a kezd� licit az aktu�lis
        }

        /*//elv�rt: bobafett11 nem hirdet�, ez�rt semennyit sem hirdetett;
        //a kontroller alapb�l unauthorised html k�ddal t�rne vissza, de mivel az authoriz�ci�s middleware nem m�k�dik tesztel�s k�zben, ez�rt lefut a kontroller akci�, �s �res list�t ad
        [Fact]
        public void GetHirdetettExperiment()
        {
            setClaims(new List<Claim>{
                new Claim(ClaimTypes.Name, "bobafett11"),
                new Claim(ClaimTypes.NameIdentifier, "testId4"),
                new Claim(ClaimTypes.Role, "nemhirdeto"),
            });

            // Act
            var result = _controller.GetHirdetett();

            // Assert
            var objRes = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
            var content = Assert.IsAssignableFrom<IEnumerable<ItemLicitDTO>>(objRes.Value);
            Assert.Empty(content);
        }*/
        #endregion

        #region region GetItemDetailsTests
        /// <summary>
        /// elv�rt: visszaadja az 1-es id-vel rendelkez� t�rgyat ItemAllLicitsDTO form�j�ban az �sszes r� �rkezett licittel egy�tt
        /// </summary>
        [Fact]
        public async void GetItemDetailsTest1Async()
        {
            // Act
            var result = await _controller.GetItemDetails(1);

            // Assert
            
            var objRes = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
            var content = Assert.IsAssignableFrom<ItemAllLicitsDTO>(objRes.Value);
            Assert.Equal(5, content.licits.Count());
        }

        /// <summary>
        /// elv�rt: visszaadja az 2-es id-vel rendelkez� t�rgyat ItemAllLicitsDTO form�j�ban a 6 darab r� �rkezett licittel egy�tt
        /// </summary>
        [Fact]
        public async void GetItemDetailsTest2()
        {
            // Act
            var result = await _controller.GetItemDetails(2);

            // Assert
            var objRes = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
            var content = Assert.IsAssignableFrom<ItemAllLicitsDTO>(objRes.Value);
            Assert.Equal(6, content.licits.Count());
        }
        /// <summary>
        /// mas hirdette, ezert unathorized-nak kell az eredmenznek lennie:
        /// </summary>
        [Fact]
        public async void GetItemDetailsTest3()
        {
            // Act
            var result = await _controller.GetItemDetails(3);

            // Assert
            var objRes = Assert.IsAssignableFrom<UnauthorizedObjectResult>(result.Result);
        }

        /// <summary>
        /// nem letezik, ezert az eredmenynek notfound-nak kell lennie:
        /// </summary>
        [Fact]
        public async void GetItemDetailsTest4()
        {
            // Act
            var result = await _controller.GetItemDetails(314159265);

            // Assert
            //var content = Assert.IsAssignableFrom<Microsoft.AspNetCore.Mvc.UnauthorizedResult>(result.Result);
            var objRes = Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }
        #endregion

        #region region PostItemTests
        /// <summary>
        /// elv�rt: sikeresen r�gz�ti a t�rgyat:
        /// </summary>
        [Fact]
        public async void PostItemTest1()
        {
            // Arrange
            var newItem = new ItemDTO
            {
                Name = "it",
                Category = new CategoryDTO { Id = 1 },
                DateOfClosing = DateTime.Now.AddDays(3),
                Picture = null,
                StartingLicit = 1,
                Description = "leiras"
            };
            var count = context.Items.Count();

            // Act
            var result = await _controller.PostItem(newItem);

            // Assert
            var objectResult = Assert.IsAssignableFrom<CreatedAtActionResult>(result.Result);
            var content = Assert.IsAssignableFrom<ItemDTO>(objectResult.Value);
            Assert.Equal(count + 1, context.Items.Count());
        }
        /// <summary>
        /// elv�rt: badrequest hib�s azonos�t�j� kateg�ria miatt:
        /// </summary>
        [Fact]
        public async void PostItemTest2()
        {
            // Arrange
            var newItem = new ItemDTO
            {
                Name = "it",
                Category = new CategoryDTO { Id = 65536 },
                DateOfClosing = DateTime.Now.AddDays(3),
                Picture = null,
                StartingLicit = 1,
                Description = "leiras"
            };
            var count = context.Items.Count();

            // Act
            var result = await _controller.PostItem(newItem);

            // Assert
            var objRes = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            Assert.Equal(400, objRes.StatusCode);
        }
        /// <summary>
        /// elv�rt: badrequest hibas keydo licit miatt
        /// </summary>
        [Fact]
        public async void PostItemTest3()
        {
            // Act
            //var result = _controller.GetItemDetails(3);

            // Assert
            //var content = Assert.IsAssignableFrom<Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult>(((Microsoft.AspNetCore.Mvc.ObjectResult)result.Result));

            // Arrange
            var newItem = new ItemDTO
            {
                Name = "it",
                Category = new CategoryDTO { Id = 1 },
                DateOfClosing = DateTime.Now.AddDays(3),
                Picture = null,
                StartingLicit = -2,
                Description = "leiras"
            };
            // Act
            var result = await _controller.PostItem(newItem);

            // Assert
            var objRes = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            Assert.Equal(400, objRes.StatusCode);
        }
        /// <summary>
        /// elv�rt: badrequest letelt zarodatum miatt
        /// </summary>
        [Fact]
        public async void PostItemTest4()
        {
            // Arrange
            var newItem = new ItemDTO
            {
                Name = "it",
                Category = new CategoryDTO { Id = 1 },
                DateOfClosing = DateTime.Now.AddDays(-5),
                Picture = null,
                StartingLicit = 100,
                Description = "leiras"
            };
            // Act
            var result = await _controller.PostItem(newItem);

            // Assert
            var objRes = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            Assert.Equal(400, objRes.StatusCode);
        }
        #endregion

        #region region CloseLicitTests
        /// <summary>
        /// elv�rt: sikeresen lez�rja a licitj�t, �s a legnagyobb licitet letev� user nyer:
        /// </summary>
        [Fact]
        public void CloseLicitTest1()
        {
            // Arrange
            var item = new ItemAllLicitsDTO
            {
                item = new ItemDTO { Id = 2 }
            };

            //bizony�t�k, hogy nem mag�t�l(z�r�si hat�rid� letel�se miatt) z�rul a licit:
            Assert.True(context.Items.Single(it => it.Id == 2).DateOfClosing > DateTime.Now.AddMinutes(3));
            // Act
            var result = _controller.CloseLicit(item);

            // Assert
            var objectResult = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
            var content = Assert.IsAssignableFrom<ItemAllLicitsDTO>(objectResult.Value);
            Assert.True(context.Items.Single(it => it.Id == 2).DateOfClosing < DateTime.Now);
            Assert.NotNull(content.winner);
            Assert.Equal("bobafett11", content.winner.UserName);
        }
        /// <summary>
        /// elv�rt: nem zarhatja le, mert nincs ra licit
        /// </summary>
        [Fact]
        public void CloseLicitTest2()
        {
            // Arrange
            var item = new ItemAllLicitsDTO
            {
                item = new ItemDTO { Id = 6 }
            };

            // Act
            var result = _controller.CloseLicit(item);

            // Assert
            var objRes = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            Assert.Equal(403, objRes.StatusCode);
        }
        /// <summary>
        /// elv�rt: unatorized, mert mar leyarult a licit
        /// </summary>
        [Fact]
        public void CloseLicitTest3()
        {
            // Arrange
            var item = new ItemAllLicitsDTO
            {
                item = new ItemDTO { Id = 1 }
            };

            // Act
            var result = _controller.CloseLicit(item);

            // Assert
            var objRes = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            Assert.Equal(405, objRes.StatusCode);
        }
        /// <summary>
        /// elv�rt: unatorized, mert nem zarhat le m�s �ltal hirdetett licitet
        /// </summary>
        [Fact]
        public void CloseLicitTest4()
        {
            // Arrange
            var item = new ItemAllLicitsDTO
            {
                item = new ItemDTO { Id = 3 }
            };

            // Act
            var result = _controller.CloseLicit(item);

            // Assert
            var objRes = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            Assert.Equal(401, objRes.StatusCode);
        }
        #endregion

        #region region GetItemTests
        /// <summary>
        /// elv�rt: sikeresen visszaaadja a k�rt azonos�t�j� t�rgyat
        /// </summary>
        [Fact]
        public void GetItemTest1Async()
        {
            // Act
            var result = _controller.GetItem(1);

            // Assert

            var objRes = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
            var content = Assert.IsAssignableFrom<ItemDTO>(objRes.Value);
            Assert.Equal("Ez egy nem l�tez� szekr�ny.", content.Description);
        }
        /// <summary>
        /// elv�rt: nem az � t�rgya, ez�rt unauthorized az eredm�ny:
        /// </summary>
        [Fact]
        public void GetItemTest2()
        {
            // Act
            var result = _controller.GetItem(3);

            // Assert
            //var content = Assert.IsAssignableFrom<Microsoft.AspNetCore.Mvc.UnauthorizedResult>(result.Result);
            var objRes = Assert.IsAssignableFrom<UnauthorizedObjectResult>(result.Result);
        }
        /// <summary>
        /// nem letezik, ezert az eredmenynek notfound-nak kell lennie:
        /// </summary>
        [Fact]
        public void GetItemTest3()
        {
            // Act
            var result = _controller.GetItem(314159265);

            // Assert
            //var content = Assert.IsAssignableFrom<Microsoft.AspNetCore.Mvc.UnauthorizedResult>(result.Result);
            var objRes = Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }
        #endregion

        #region region GetCategoriesTests
        /// <summary>
        /// elv�rt: a GetCategories() visszat�r a 3 kateg�ria�t tartalmaz� list�val
        /// </summary>
        [Fact]
        public async void GetCategoriesTest1()
        {
            var result = await _controller.GetCategories();

            var objRes = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
            var content = Assert.IsAssignableFrom<List<CategoryDTO>>(objRes.Value);
            Assert.Equal(3, content.Count());
        }
        /// <summary>
        /// elv�rt: mivel f�lvesz�nk a tesztel�s elej�n egy �j kateg�ri�t, a GetCategories() az �j kateg�ri�t is visszak�ldi a list�ban
        /// </summary>
        [Fact]
        public async void GetCategoriesTest2()
        {
            context.Categories.Add(new Category { Id = 4, Name = "Ez egy �j kateg�ria" });
            context.SaveChanges();
            var result = await _controller.GetCategories();

            var objRes = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
            var content = Assert.IsAssignableFrom<List<CategoryDTO>>(objRes.Value);
            Assert.Equal(4, content.Count());
            Assert.True( content.Where(c => c.Id == 4).FirstOrDefault() != null);
            Assert.Equal("Ez egy �j kateg�ria", content.Where(c => c.Id == 4).FirstOrDefault().Name);
        }
        #endregion
    }
}