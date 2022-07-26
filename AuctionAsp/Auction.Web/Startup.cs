using Auction.Persistence;
using Auction.Persistence.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auction
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            DbType dbType = Configuration.GetValue<DbType>("DbType");

            //DbInitializer.Initialize(context);
            //VoterService service = new VoterService(context);

            ///módosítások a travelagency példaprogi alapján, hátha így mûködni fog a scaffolding meg majd a program, és így nem rugnak ki az egyetemrõl:
            // Dependency injection beállítása az adatbázis kontextushoz
            services.AddDbContext<AuctionContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection"),
                    x => x.MigrationsAssembly("Auction.Persistence")));
            /*
            switch (dbType)
            {
                    // Need Microsoft.EntityFrameworkCore.SqlServer package for this
                case DbType.SqlServer:
                    services.AddDbContext<AuctionContext>(options =>
                        options.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection"))
                        );
                    break;
                // Need Microsoft.EntityFrameworkCore.Sqlite package for this
                // Using Microsoft.EntityFrameworkCore.Sqlite.Core causes error with update-database
                case DbType.Sqlite:
                    services.AddDbContext<AuctionContext>(o =>
                        o.UseSqlite(Configuration.GetConnectionString("SqliteConnection"))
                        );
                    break;
            }
            */
            services.AddIdentity<User, IdentityRole<int>>()
               .AddEntityFrameworkStores<AuctionContext>() // EF használata az AuctionContext entitás kontextussal
               .AddDefaultTokenProviders(); // Alapértelmezett token generátor használata (pl. SecurityStamp-hez)

            services.Configure<IdentityOptions>(options =>
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
            });


            services.AddTransient<HomeService>();
            //identity managementet hasznal unk ehlzette: services.AddTransient<AccountService>();

            //services.AddSingleton<ApplicationState>();
            //services.AddSingleton<ApplicationState>();
            // Dependency injection a IHttpContextAccessor-hoz
            services.AddHttpContextAccessor();

            services.AddControllersWithViews();

            // Munkamenetkezelés beállítása
            //services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(15); // max. 15 percig él a munkamenet
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            /// UseAuthentication, a travelagency példaprogi alapján:
            app.UseAuthentication();
            app.UseAuthorization();

            // Munkamentek használata
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            //adatb inicializálása:
            DbInitializer.Initialize(serviceProvider, Configuration.GetValue<string>("ImageStore"));
        }
    }
}
