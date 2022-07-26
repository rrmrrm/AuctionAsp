using Auction.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auction.Persistence.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;

namespace Auction.Webapi
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


            services.AddTransient<ApiService>();
            //identity manager vezerli ezt: services.AddTransient<AccountService>();

            //services.AddSingleton<ApplicationState>();
            //services.AddSingleton<ApplicationState>();
            // Dependency injection a IHttpContextAccessor-hoz
            //services.AddHttpContextAccessor();

            services.AddControllers();
            // Swagger generator regisztrálása
            services.AddSwaggerGen(c =>
            {
                // (név, OpenApiInfo) párok megadása szükséges
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auction API", Version = "v1" });

                // XML API dokumentáció felhasználása a Swaggerben
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Swagger használata (JSON végpontok)
            app.UseSwagger();
            //swagger ui eleresi utvonalai ez lsey:
            // domain>/swagger/index.html es 
            //<domain nev>.swaffer/v1/swagger.json
            // Swagger UI engedélyezése (böngészhetõ HTML végpontok)
            app.UseSwaggerUI(c =>
            {
                // a JSON végpont megadása (és engedélyezése szükséges)
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auction API V1");
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
