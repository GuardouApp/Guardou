using Guardou.Application.Authentication;
using Guardou.Application.Files;
using Guardou.Application.Persistence;
using Guardou.Application.Previews;
using Guardou.Application.Tasks;
using Guardou.Application.Users;
using Guardou.Domain;
using Guardou.Domain.Users;
using Guardou.Infrastructure.Files;
using Guardou.Infrastructure.Identity;
using Guardou.Infrastructure.Persistence;
using Guardou.Infrastructure.Previews;
using Guardou.Infrastructure.Storage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Guardou.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton(new StoragePaths(StoragePaths.DefaultFilesRoot, StoragePaths.DefaultDatabaseRoot, StoragePaths.DefaultCacheRoot));
        services.AddDbContext<AppDbContext>((provider, options) =>
            options.UseSqlite(ConnectionString(provider.GetRequiredService<StoragePaths>())));

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
        services.AddSingleton<LocalBlobStore>();
        services.AddSingleton<IBlobStore>(provider => provider.GetRequiredService<LocalBlobStore>());
        services.AddSingleton<IMimeTypeMap, MimeTypeMap>();
        services.AddScoped<ITaskStore, EfTaskStore>();
        services.AddScoped<ITaskQueueSettingsStore, EfTaskQueueSettingsStore>();
        services.AddSingleton<IImageSource, VipsImageSource>();
        services.AddSingleton<IImageSource, RawImageSource>();
        services.AddSingleton<IImageSource, HeicImageSource>();
        services.AddSingleton<IImageSource, PdfImageSource>();
        services.AddSingleton<IImageSource, VideoImageSource>();
        services.AddSingleton<IPreviewRenderer, VipsPreviewRenderer>();
        services.AddSingleton<IPreviewCache, FilePreviewCache>();
        return services;
    }

    public static IServiceCollection AddWebInfrastructure(this IServiceCollection services)
    {
        new IdentityBuilder(typeof(IdentityUser), typeof(IdentityRole), services).AddSignInManager();
        services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
        services.ConfigureApplicationCookie(SessionCookie.Configure);
        services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);

        services.AddDataProtection().SetApplicationName(Product.Name).PersistKeysToDbContext<AppDbContext>();

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
        services.AddScoped<ISessionService, IdentitySessionService>();
        return services;
    }

    private static string ConnectionString(StoragePaths paths)
    {
        return new SqliteConnectionStringBuilder { DataSource = paths.DatabaseFile }.ToString();
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
