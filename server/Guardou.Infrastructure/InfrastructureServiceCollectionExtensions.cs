using Guardou.Application.Authentication;
using Guardou.Application.Files;
using Guardou.Application.Persistence;
using Guardou.Application.Preferences;
using Guardou.Application.Previews;
using Guardou.Application.Search;
using Guardou.Application.Settings;
using Guardou.Application.Shares;
using Guardou.Application.Tasks;
using Guardou.Application.Quotas;
using Guardou.Application.Uploads;
using Guardou.Application.Users;
using Guardou.Domain;
using Guardou.Domain.Authentication;
using Guardou.Domain.Users;
using Guardou.Infrastructure.Files;
using Guardou.Infrastructure.Identity;
using Guardou.Infrastructure.Identity.Oidc;
using Guardou.Infrastructure.Indexing;
using Guardou.Infrastructure.Persistence;
using Guardou.Infrastructure.Previews;
using Guardou.Infrastructure.Profiles;
using Guardou.Infrastructure.Storage;
using Guardou.Infrastructure.Uploads;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using tusdotnet.Stores;

namespace Guardou.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton(new StoragePaths(StoragePaths.DefaultRoot));
        services.AddDbContext<AppDbContext>((provider, options) =>
            options.UseSqlite(ConnectionString(provider.GetRequiredService<StoragePaths>().DatabaseFile)));
        services.AddDbContext<IndexDbContext>((provider, options) =>
            options.UseSqlite(ConnectionString(provider.GetRequiredService<StoragePaths>().IndexDatabaseFile)));

        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddUserValidator<UsernameValidator>()
            .AddUserValidator<DisplayNameValidator>();
        services.Configure<IdentityOptions>(identity =>
        {
            identity.User.RequireUniqueEmail = true;
            ApplyPasswordPolicy(identity.Password, PasswordPolicy.Default);
        });

        services.AddScoped<IUserAccountStore, IdentityUserAccountStore>();
        services.AddScoped<IDatabaseMigrator, DatabaseMigrator>();
        services.AddScoped<IItemStore, EfItemStore>();
        services.AddScoped<IWriteTransactions, EfWriteTransactions>();
        services.AddScoped<StorageUsageCounter>();
        services.AddScoped<IStorageQuotaStore, EfStorageQuotaStore>();
        services.AddScoped<ItemIndexWriter>();
        services.AddScoped<IItemIndexRebuilder>(provider => provider.GetRequiredService<ItemIndexWriter>());
        services.AddScoped<IItemIndex, EfItemIndex>();
        services.AddScoped<IShareStore, EfShareStore>();
        services.AddScoped<IDirectShareStore, EfDirectShareStore>();
        services.AddScoped<IShareLookup, EfShareLookup>();
        services.AddScoped<IItemOpenStore, EfItemOpenStore>();
        services.AddScoped<IUserNames, EfUserNames>();
        services.AddScoped<IUserDirectory, EfUserDirectory>();
        services.AddSingleton<LocalBlobStore>();
        services.AddSingleton<IBlobStore>(provider => provider.GetRequiredService<LocalBlobStore>());
        services.AddSingleton<IMimeTypeMap, MimeTypeMap>();
        services.AddSingleton(provider =>
            new TusDiskStore(Directory.CreateDirectory(provider.GetRequiredService<StoragePaths>().UploadsDirectory).FullName));
        services.AddSingleton<IUploadContent, TusUploadContent>();
        services.AddScoped<IUploadStore, EfUploadStore>();
        services.AddScoped<ITaskStore, EfTaskStore>();
        services.AddScoped<ITaskQueueSettingsStore, EfTaskQueueSettingsStore>();
        services.AddScoped<EfSettingsStore>();
        services.AddScoped<ISettingsStore>(provider => provider.GetRequiredService<EfSettingsStore>());
        services.AddSingleton<SettingsSecrets>();
        services.AddDataProtection().SetApplicationName(Product.Name).PersistKeysToDbContext<AppDbContext>();
        services.AddScoped<IPreferencesStore, EfPreferencesStore>();
        services.AddOptions<SettingsOverrides>().BindConfiguration(SettingsOverrides.Section);
        services.AddSingleton<OidcSettingsSnapshot>();
        services.AddSingleton<IOidcSettingsObserver>(provider => provider.GetRequiredService<OidcSettingsSnapshot>());
        services.AddSingleton<IImageSource, VipsImageSource>();
        services.AddSingleton<IImageSource, RawImageSource>();
        services.AddSingleton<IImageSource, HeicImageSource>();
        services.AddSingleton<IImageSource, PdfImageSource>();
        services.AddSingleton<IImageSource, VideoImageSource>();
        services.AddSingleton<IPreviewRenderer, VipsPreviewRenderer>();
        services.AddSingleton<IPreviewCache, FilePreviewCache>();
        services.AddHttpClient<ProfilePictureDownloader>(client => client.Timeout = ProfilePictureDownloader.Timeout)
            .ConfigurePrimaryHttpMessageHandler(PublicAddressConnector.CreateHandler);
        services.AddSingleton<IProfilePictureStore, FileProfilePictureStore>();
        return services;
    }

    public static IServiceCollection AddWebInfrastructure(this IServiceCollection services)
    {
        new IdentityBuilder(typeof(ApplicationUser), typeof(IdentityRole), services).AddSignInManager();
        AuthenticationBuilder authentication = services.AddAuthentication(IdentityConstants.ApplicationScheme);
        authentication.AddIdentityCookies();
        authentication.AddOpenIdConnect(OidcSettings.Provider, _ => { });
        services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, OidcOptionsSetup>();
        services.AddSingleton<IOptionsChangeTokenSource<OpenIdConnectOptions>>(provider => provider.GetRequiredService<OidcSettingsSnapshot>());
        services.AddScoped<OidcEvents>();
        services.AddScoped<OidcSignOut>();
        services.ConfigureApplicationCookie(SessionCookie.Configure);
        services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>().AddDbContextCheck<IndexDbContext>();
        services.AddScoped<ISessionService, IdentitySessionService>();
        return services;
    }

    private static string ConnectionString(string databaseFile)
    {
        return new SqliteConnectionStringBuilder { DataSource = databaseFile }.ToString();
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
