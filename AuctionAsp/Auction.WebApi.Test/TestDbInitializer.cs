using Auction.Persistence;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Auction.Webapi.Tests
{
    public static class TestDbInitializer
    {
        public static void Initialize(AuctionContext context, UserManager<User> userManager)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
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
                Id = 3,
                Name = "Arthur Atkinson",
                PhoneNumber = "06302225555",
                UserName = "arthurkatkinson",
                Email = "arthur@gmail.com"
            };
            var u4 = new User
            {
                Id = 4,
                Name = "Boba Fett",
                PhoneNumber = "06223344556",
                UserName = "bobafett11",
                Email = "bobafett@gmail.com"
            };

            ICollection<Category> defaultCategories = new List<Category> {
                    new Category{Id =1, Name = "bútor"},
                    new Category{Id =2, Name = "ingatlan"},
                    new Category{Id =3, Name = "egyéb"}
                };
            foreach (var c in defaultCategories)
            {
                context.Categories.Add(c);
            }
            context.SaveChanges();

            ICollection<Item> defaultItems = new List<Item>
                {
                new Item{
                    Id = 1,
                    AuctionStartDate = new DateTime(2021,1,1),
                    Category = defaultCategories.ElementAt(0),
                    DateOfClosing = new DateTime(2021,3,1),
                    Name="IKEA schrössargbáur összecsukható szekrény",
                    Description="Ez egy nem létező szekrény.",
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
                    AuctionStartDate = new DateTime(2021,2,2),
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
                },
                new Item{
                    Id = 7,
                    AuctionStartDate = new DateTime(2021,1,2),
                    Category = defaultCategories.ElementAt(1),
                    DateOfClosing = new DateTime(2021,6,2),
                    Name="ez a 4. amit nem neil1234 hirdetet",
                    Description="ez meg egy leiras",
                    UserId=u2.Id,
                    StartingLicit=9876,
                    Picture = null
                }

            };
            foreach (var item in defaultItems)
            {
                context.Items.Add(item);
            }
            context.SaveChanges();

            ICollection<Licit> defaultLicits = new List<Licit> {
                //neil1234 hirdette
                new Licit{Item=defaultItems.ElementAt(0), Date=defaultItems.ElementAt(0).AuctionStartDate.AddDays(1), UserId=1, Value=20001},
                new Licit{Item=defaultItems.ElementAt(0), Date=defaultItems.ElementAt(0).AuctionStartDate.AddDays(2), UserId=2, Value=20002},
                new Licit{Item=defaultItems.ElementAt(0), Date=defaultItems.ElementAt(0).AuctionStartDate.AddDays(3), UserId=3, Value=20003},

                new Licit{Item=defaultItems.ElementAt(0), Date=defaultItems.ElementAt(0).AuctionStartDate.AddDays(4), UserId=4, Value=20004},
                new Licit{Item=defaultItems.ElementAt(0), Date=defaultItems.ElementAt(0).AuctionStartDate.AddDays(5), UserId=4, Value=20005},

                //bruce11 hirdette
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
    }
}
