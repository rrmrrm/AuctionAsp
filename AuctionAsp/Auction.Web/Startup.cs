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

            ///m�dos�t�sok a travelagency p�ldaprogi alapj�n, h�tha �gy m�k�dni fog a scaffolding meg majd a program, �s �gy nem rugnak ki az egyetemr�l:
            // Dependency injection be�ll�t�sa az adatb�zis kontextushoz
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
               .AddEntityFrameworkStores<AuctionContext>() // EF haszn�lata az AuctionContext entit�s kontextussal
               .AddDefaultTokenProviders(); // Alap�rtelmezett token gener�tor haszn�lata (pl. SecurityStamp-hez)

            services.Configure<IdentityOptions>(options =>
            {
                // Jelsz� komplexit�s�ra vonatkoz� konfigur�ci�
                //options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                //options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                //options.Password.RequiredUniqueChars = 3;

                //// Hib�s bejelentkez�s eset�n az (ideiglenes) kiz�r�sra vonatkoz� konfigur�ci�
                //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                //options.Lockout.MaxFailedAccessAttempts = 10;
                //options.Lockout.AllowedForNewUsers = true;

                // Felhaszn�l�kezel�sre vonatkoz� konfigur�ci�
                options.User.RequireUniqueEmail = true;
            });


            services.AddTransient<HomeService>();
            //identity managementet hasznal unk ehlzette: services.AddTransient<AccountService>();

            //services.AddSingleton<ApplicationState>();
            //services.AddSingleton<ApplicationState>();
            // Dependency injection a IHttpContextAccessor-hoz
            services.AddHttpContextAccessor();

            services.AddControllersWithViews();

            // Munkamenetkezel�s be�ll�t�sa
            //services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(15); // max. 15 percig �l a munkamenet
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

            /// UseAuthentication, a travelagency p�ldaprogi alapj�n:
            app.UseAuthentication();
            app.UseAuthorization();

            // Munkamentek haszn�lata
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            //adatb inicializ�l�sa:
            DbInitializer.Initialize(serviceProvider, Configuration.GetValue<string>("ImageStore"));
        }
    }
}
