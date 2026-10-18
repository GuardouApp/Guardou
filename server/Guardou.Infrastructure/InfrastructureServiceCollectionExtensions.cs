using Guardou.Application.Authentication;
using Guardou.Application.Files;
using Guardou.Application.Persistence;
using Guardou.Application.Users;
using Guardou.Domain;
using Guardou.Domain.Users;
using Guardou.Infrastructure.Files;
using Guardou.Infrastructure.Identity;
using Guardou.Infrastructure.Persistence;
using Guardou.Infrastructure.Storage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Guardou.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton(new DataDirectory(DataDirectory.DefaultRoot));
        services.AddDbContext<AppDbContext>((provider, options) =>
            options.UseSqlite(ConnectionString(provider.GetRequiredService<DataDirectory>())));

        services.AddIdentityCore<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddUserValidator<UsernameValidator>();
        services.Configure<IdentityOptions>(identity =>
        {
            identity.User.RequireUniqueEmail = true;
            ApplyPasswordPolicy(identity.Password, PasswordPolicy.Default);
        });

        services.AddScoped<IUserAccountStore, IdentityUserAccountStore>();
        services.AddScoped<IDatabaseMigrator, DatabaseMigrator>();
        services.AddScoped<IItemStore, EfItemStore>();
        services.AddSingleton<IBlobStore, LocalBlobStore>();
        services.AddSingleton<IMimeTypeMap, MimeTypeMap>();
        return services;
    }

    public static IServiceCollection AddWebInfrastructure(this IServiceCollection services)
    {
        new IdentityBuilder(typeof(IdentityUser), typeof(IdentityRole), services).AddSignInManager();
        services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
        services.ConfigureApplicationCookie(SessionCookie.Configure);
        services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);

        services.AddDataProtection().SetApplicationName(Product.Name);
        services.AddOptions<KeyManagementOptions>()
            .Configure<DataDirectory, ILoggerFactory>((options, dataDirectory, loggerFactory) =>
                options.XmlRepository = new FileSystemXmlRepository(new DirectoryInfo(dataDirectory.KeysDirectory), loggerFactory));

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
        services.AddScoped<ISessionService, IdentitySessionService>();
        return services;
    }

    private static string ConnectionString(DataDirectory dataDirectory)
    {
        return new SqliteConnectionStringBuilder { DataSource = dataDirectory.DatabaseFile }.ToString();
    }

    private static void ApplyPasswordPolicy(PasswordOptions password, PasswordPolicy policy)
    {
        password.RequiredLength = policy.MinimumLength;
        password.RequireDigit = policy.RequireDigit;
        password.RequireLowercase = policy.RequireLowercase;
        password.RequireUppercase = policy.RequireUppercase;
        password.RequireNonAlphanumeric = policy.RequireNonAlphanumeric;
    }
}
