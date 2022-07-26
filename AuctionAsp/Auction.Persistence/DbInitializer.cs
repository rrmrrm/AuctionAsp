using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security;
using System.Security.Cryptography;
using System.IO;
using Microsoft.AspNetCore.Identity;

namespace Auction.Persistence
{
    public static class DbInitializer
    {
        private static UserManager<User> _userManager;
        private static RoleManager<IdentityRole<int>> _roleManager;
        private static AuctionContext vC;
        public static void Initialize(IServiceProvider provider, string imageDirectory)
        {
            vC = provider.GetRequiredService<AuctionContext>();
            _userManager = provider.GetRequiredService<UserManager<User>>();
            _roleManager = provider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            /*
            Adatbázis létrehozása (amennyiben nem létezik), illetve a migrációk alapján a frissítése.
            Amennyiben teljesen el szeretnénk vetni a jelenlegi adatbázisunk, akkor a
            törléshez az Sql Server Object Explorer ablak használható a Visual Studioban.
            Itt SQL Server > (localdb)\\MSSQLLocalDB útvonalon találjuk az adatbázisainkat.
            */
            vC.Database.EnsureDeleted();
            vC.Database.EnsureCreated();
            //vC.Database.Migrate();

            // Felhasználók inicializálása
            if (!vC.Users.Any())
            {
                SeedUsers(imageDirectory);
            }
            // és csak aezután jöhet a többi dolog inicializáslása pillanatnyilag(a user es hirdeto tablakat meg hasznalom, és a seedusers inicializalja ezeket mostmég)
            if (!vC.Items.Any())
            {
                seedStuff( imageDirectory);
            };



        }


        private static void seedStuff(string imageDirectory)
        {
            ICollection<Category> defaultCategories = new List<Category> {
                    new Category{Name = "bútor"},
                    new Category{Name = "ingatlan"},
                    new Category{Name = "egyéb"}
                };
            foreach (var c in defaultCategories)
            {
                vC.Categories.Add(c);
            }
            vC.SaveChanges();

            List<User> users = _userManager.GetUsersInRoleAsync("hirdeto").Result.ToList();
            ICollection<Item> defaultItems = new List<Item>
            {
                new Item{
                    AuctionStartDate = new DateTime(2021,1,1),
                    Category = defaultCategories.ElementAt(0),
                    DateOfClosing = new DateTime(2021,3,1),
                    Name="IKEA schrössargbáur összecsukható szekrény",
                    Description="Ez egy nem létező szekrény.",
                    Hirdeto=users.ElementAt(0),
                    StartingLicit=20000,
                    Picture = File.ReadAllBytes(Path.Combine(imageDirectory, "p1.png")),
                },
                new Item{
                    AuctionStartDate = new DateTime(2021,3,2),
                    Category = defaultCategories.ElementAt(0),
                    DateOfClosing = new DateTime(2021,6,2),
                    Name="randomFotelNév fotel",
                    Description="Ez egy fotel.",
                    Hirdeto=users.ElementAt(0),
                    StartingLicit=8000,
                    Picture = File.ReadAllBytes(Path.Combine(imageDirectory, "p2.png")),
                },

                new Item{
                    AuctionStartDate = new DateTime(2021,6,2),
                    Category = defaultCategories.ElementAt(0),
                    DateOfClosing = new DateTime(2021,7,2),
                    Name="Boholmen mosdókagyló",
                    Description="Ez egy rozsdamemtes fém konyhai mosdókagyló.",
                    Hirdeto=users.ElementAt(1),
                    StartingLicit=4000,
                    Picture = File.ReadAllBytes(Path.Combine(imageDirectory, "p3.png")),
                },
                new Item{
                    AuctionStartDate = new DateTime(2021,1,2),
                    Category = defaultCategories.ElementAt(0),
                    DateOfClosing = new DateTime(2021,6,2),
                    Name="IKEA Einherjer ruhaszárító",
                    Description="Az Einherjer amúgy metálzenekar, nem szárító.",
                    Hirdeto=users.ElementAt(1),
                    StartingLicit=4000,
                    Picture = File.ReadAllBytes(Path.Combine(imageDirectory, "p4.png")),
                },


                new Item{
                    AuctionStartDate = new DateTime(2021,1,2),
                    Category = defaultCategories.ElementAt(1),
                    DateOfClosing = new DateTime(2021,6,2),
                    Name="Eladó 3 szobás lakás Budapest belvárosban",
                    Description="részletek a https:ne.keresd linken",
                    Hirdeto=users.ElementAt(1),
                    StartingLicit=90000000,
                    Picture = File.ReadAllBytes(Path.Combine(imageDirectory, "p5.png")),
                }

            };
            //sok targy kell azért, hogy látsszon a tárgyakat megjelenírő weboldalon hogy egy oldalon csak 20 tárgy jelenik meg:
            for (int i = 0; i < 50; i++)
            {
                defaultItems.Add(
                    new Item
                    {
                        AuctionStartDate = new DateTime(1900 + i, 4, 14),//ez alapján lesz rendezve a főoldalon
                        Category = defaultCategories.ElementAt(2),
                        DateOfClosing = new DateTime(2021 + i, 6, 2),
                        Name = "test item_" + i.ToString(),
                        Description = "test item description_" + i.ToString(),
                        Hirdeto = users.ElementAt(1),

                        StartingLicit = i * 1000,
                    }
                );
            }
            foreach (var item in defaultItems)
            {
                vC.Items.Add(item);
            }
            vC.SaveChanges();

            List<User> defaultUsers = vC.Users.ToList();
            ICollection<Licit> defaultLicits = new List<Licit> {
                new Licit{Item=defaultItems.ElementAt(0), Date=defaultItems.ElementAt(0).AuctionStartDate.AddDays(1), User=defaultUsers.ElementAt(0), Value=20001},
                new Licit{Item=defaultItems.ElementAt(0), Date=defaultItems.ElementAt(0).AuctionStartDate.AddDays(2), User=defaultUsers.ElementAt(1), Value=20002},
                new Licit{Item=defaultItems.ElementAt(0), Date=defaultItems.ElementAt(0).AuctionStartDate.AddDays(3), User=defaultUsers.ElementAt(2), Value=20003},

                new Licit{Item=defaultItems.ElementAt(0), Date=defaultItems.ElementAt(0).AuctionStartDate.AddDays(4), User=defaultUsers.ElementAt(3), Value=20004},
                new Licit{Item=defaultItems.ElementAt(0), Date=defaultItems.ElementAt(0).AuctionStartDate.AddDays(5), User=defaultUsers.ElementAt(3), Value=20005},

                new Licit{Item=defaultItems.ElementAt(1), Date=defaultItems.ElementAt(1).AuctionStartDate.AddDays(1), User=defaultUsers.ElementAt(0), Value=8001},
                new Licit{Item=defaultItems.ElementAt(1), Date=defaultItems.ElementAt(1).AuctionStartDate.AddDays(2), User=defaultUsers.ElementAt(1), Value=8002},
                new Licit{Item=defaultItems.ElementAt(1), Date=defaultItems.ElementAt(1).AuctionStartDate.AddDays(3), User=defaultUsers.ElementAt(0), Value=8003},
                new Licit{Item=defaultItems.ElementAt(1), Date=defaultItems.ElementAt(1).AuctionStartDate.AddDays(4), User=defaultUsers.ElementAt(2), Value=8004},
                new Licit{Item=defaultItems.ElementAt(1), Date=defaultItems.ElementAt(1).AuctionStartDate.AddDays(5), User=defaultUsers.ElementAt(3), Value=8005}
            };
            foreach (var l in defaultLicits)
            {
                vC.Licits.Add(l);
            }
            vC.SaveChanges();
        }

        /// <summary>
        /// Adminisztrátorok inicializálása.
        /// </summary>
        private static void SeedUsers(string imageDirectory)
        {
            var adminUser = new User
            {
                UserName = "admin",
                Name = "Adminisztrátor",
                Email = "admin@example.com",
                PhoneNumber = "+36123456789",
            };
            var adminUser2 = new User
            {
                UserName = "admin2",
                Name = "Adminisztrátor2",
                Email = "admin2@example.com",
                PhoneNumber = "+36123456782",
            };
            var adminPassword = "Almafa123";
            var adminRole = new IdentityRole<int>("hirdeto");

            /*   
            var result11 = _userManager.CreateAsync(adminUser, adminPassword).Result;
            vC.SaveChanges();
            var result22 = _roleManager.CreateAsync(adminRole).Result;
            var result33 = _userManager.AddToRoleAsync(adminUser, adminRole.Name).Result;
            vC.SaveChanges();
            */


            var u1 = new User
            {
                Name = "Neil Collings",
                PhoneNumber = "06302225555",
                UserName = "neil1234",
                Email = "neil.collings@gmail.com",
            };
            var u2 = new User { Name = "Bruce Castor", PhoneNumber = "06301234567", UserName = "bruce11", Email = "brucecastor@gmail.com"
            };
            var u3 = new User { Name = "Arthur Atkinson", PhoneNumber = "06302225555", UserName = "arthurkatkinson", Email = "arthur@gmail.com"
            };
            var u4 = new User
            {
                Name = "Boba Fett",
                PhoneNumber = "06223344556",
                UserName = "bobafett11",
                Email = "bobafett@gmail.com"
            };

            var result11 =  _userManager.CreateAsync(u1, "Almafa123").Result;
            var r1 =        _userManager.CreateAsync(u2, "Password123").Result;
            var r12 =       _userManager.CreateAsync(u3, "Almafa123").Result;
            var r2 =        _userManager.CreateAsync(u4, "Password345").Result;
            //var r4 = _userManager.CreateAsync(u4, "password4").Result;
            vC.SaveChanges();

            // hirdetők felvétele:
            //az első 2 felhasználó hirdető role-t kap, így ők hirdethetnek majd tárgyakat is
            //var hirdetoRole = new IdentityRole<int>("hirdeto");
            var res = _roleManager.CreateAsync(adminRole).Result;

            var result1 = _userManager.AddToRoleAsync(u1, adminRole.Name).Result;
            var result2 = _userManager.AddToRoleAsync(u2, adminRole.Name).Result;
            vC.SaveChanges();

            /*
            var hirdeto = new User
            {
                UserName = "admin",
                Name = "Hirdeto1",
            };
            var adminPassword = "Almafa123";
            var result1 = _userManager.CreateAsync(hirdeto).Result;
            var result2 = _roleManager.CreateAsync(adminRole).Result;
            var result3 = _userManager.AddToRoleAsync(hirdeto, adminRole.Name).Result;
            */
            /*
            //UserChallenge=?
            ICollection<User> defaultUsers;
            using (SHA512CryptoServiceProvider crypto = new SHA512CryptoServiceProvider())
            {
                defaultUsers = new List<User>{
                        new User {Name="Neil Collings",     Phone="06302225555", UserName="neil1234", Email = "neil.collings@gmail.com",           Password = crypto.ComputeHash(Encoding.UTF8.GetBytes("asdfgh")), UserChallenge=Guid.NewGuid().ToString()},
                        new User {Name="Bruce Castor",      Phone="06301234567", UserName="bruce11",    Email = "brucecastor@gmail.com",             Password = crypto.ComputeHash(Encoding.UTF8.GetBytes("123456")), UserChallenge=Guid.NewGuid().ToString()},
                        new User {Name="Arthur Atkinson",   Phone="06302225555", UserName="arthurkatkinson", Email = "arthurkatkinson@gmail.com",         Password = crypto.ComputeHash(Encoding.UTF8.GetBytes("123456")), UserChallenge=Guid.NewGuid().ToString()},
                        new User {Name="Boba Fett",         Phone="06223344556", UserName="bobafett11", Email = "bobafett@gmail.com", Password = crypto.ComputeHash(Encoding.UTF8.GetBytes("bobafett")), UserChallenge=Guid.NewGuid().ToString()},

                        new User {Name="u1",     Phone="06302225555", UserName="bobdole", Email = "bobdole@gmail.com",                 Password = crypto.ComputeHash(Encoding.UTF8.GetBytes("asdfgh")), UserChallenge=Guid.NewGuid().ToString() },
                        new User {Name="u2",     Phone="06302225555", UserName="ivan222", Email = "IvanMaslennikov@gmail.com",         Password = crypto.ComputeHash(Encoding.UTF8.GetBytes("asdfgh")), UserChallenge=Guid.NewGuid().ToString() },
                        new User {Name="u3",     Phone="06702225555", UserName="valentina444", Email = "ValentinSavitsky@gmail.com",        Password = crypto.ComputeHash(Encoding.UTF8.GetBytes("asdfgh")), UserChallenge=Guid.NewGuid().ToString() },
                        new User {Name="u4",     Phone="06702225555", UserName="vasily1234", Email = "VasilyArkhipov@gmail.com",          Password = crypto.ComputeHash(Encoding.UTF8.GetBytes("____Thx")), UserChallenge=Guid.NewGuid().ToString() },

                        new User {Name="u5",     Phone="06702225555", UserName="testtest111", Email = "testUser1@alma.com",                Password = crypto.ComputeHash(Encoding.UTF8.GetBytes("123456")), UserChallenge=Guid.NewGuid().ToString() },
                        new User {Name="u6",     Phone="06202225555", UserName="testtest222", Email = "testUser2@alma.com",                Password = crypto.ComputeHash(Encoding.UTF8.GetBytes("123456")), UserChallenge=Guid.NewGuid().ToString() },
                        new User {Name="u7",     Phone="06202225555", UserName="testtest333", Email = "testUser3@alma.com",                Password = crypto.ComputeHash(Encoding.UTF8.GetBytes("123456")), UserChallenge=Guid.NewGuid().ToString() },
                    };
            }
            foreach (var user in defaultUsers)
            {
                vC.Users.Add(user);
            }
            vC.SaveChanges();
            */
            /*
            byte[] testPicture = File.ReadAllBytes(Path.Combine(imageDirectory, "p5.png"));
            ICollection<Hirdeto> defaultHirdetok = new List<Hirdeto> {
                    new User{
                        Name=defaultUsers.ElementAt(0).Name,
                        UserName=defaultUsers.ElementAt(0).UserName,
                        Password=defaultUsers.ElementAt(0).Password,
                    },
                    new Hirdeto{
                        Name=defaultUsers.ElementAt(1).Name,
                        UserName=defaultUsers.ElementAt(1).UserName,
                        Password=defaultUsers.ElementAt(1).Password,
                    },
                    new Hirdeto{
                        Name=defaultUsers.ElementAt(2).Name,
                        UserName=defaultUsers.ElementAt(2).UserName,
                        Password=defaultUsers.ElementAt(2).Password,
                    },
                    new Hirdeto{
                        Name=defaultUsers.ElementAt(3).Name,
                        UserName=defaultUsers.ElementAt(3).UserName,
                        Password=defaultUsers.ElementAt(3).Password,
                    }
                };
            foreach (var h in defaultHirdetok)
            {
                vC.Hirdetok.Add(h);
            }
            */
        }
    }
}
