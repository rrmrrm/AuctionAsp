using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Auction.Persistence
{
    public class AuctionContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AuctionContext() // Mockoláshoz szükséges
        { }

        public AuctionContext(DbContextOptions<AuctionContext> options)
            : base(options)
        {
        }
        /// TODO: ha az idnetity kezelés megcsinálom, akkor a Guest helyett lehet, a 'User' entitást kell használnom itt is és máshol is(a Hirdeto pedig valszeg a Userbol kene hogy leszarmazzon)
        protected override void OnModelCreating(ModelBuilder builder)
        {
            ///TODO: megoldani, hogz ne kelljen mosositani a migracio utan ay adatbazismodellt.
            foreach (var relationship in builder.Model.GetEntityTypes(nameof(Item)).SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.ClientCascade;
            }
            ///most MANUÁLISAN minden migralas utan NoAction re kell allitani az Itembol Userbe mutato foreignKezre vonatkozo OnDelete metódust

            base.OnModelCreating(builder);

            builder.Entity("Auction.Persistence.Item", b =>
            {
                b.HasOne("Auction.Persistence.Category", "Category")
                    .WithMany()
                    .HasForeignKey("CategoryId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Auction.Persistence.User", "Hirdeto")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
            });

            builder.Entity<User>().ToTable("MyUsers");
            // A felhasználói tábla alapértelmezett neve AspNetUsers lenne az adatbázisban, de ezt felüldefiniálhatjuk.
        }
        //identity manager vezérli:  new public virtual DbSet<User> Users { get; set; } ///TODO: ELFEDI A AZ ÖRÖKÖLT IDENTITY OBJEKTUM 'Users' MEZŐJÉT. EZ VESZÉLYES!!!!
        //identity manager vezérli: public virtual DbSet<Hirdeto> Hirdetok { get; set; }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<Licit> Licits { get; set; }
/*
		 * A IdentityDbContext típustól további, az authentikációhoz és autorizációhoz kapcsolódó kollekciókat öröklünk, pl.:
		 * Users
		 * UserRoles
		 * stb.
		 */
    }
}
