using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Auth.Identity.Data;
using Auth.Identity.Services;
using Auth.Infra.Data;

namespace Auth.Infra.Extensions
{
    public static class StartupExtensions
    {
        public static async void ApplyMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateAsyncScope();

            var identityContext = scope.ServiceProvider.GetService<IdentityDataContext>();
            var dataContext = scope.ServiceProvider.GetService<DataContext>();

            await identityContext.Database.MigrateAsync();
            await dataContext.Database.MigrateAsync();
        }

        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseSqlServer(configuration.GetConnectionString("DataDB"));
            });

            services.AddDbContext<IdentityDataContext>(opt =>
            {
                opt.UseSqlServer(configuration.GetConnectionString("IdentityDB"));
            });

            services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<IdentityDataContext>()
                    .AddDefaultTokenProviders();

            services.AddScoped<IdentityService>();
        }
    }
}
