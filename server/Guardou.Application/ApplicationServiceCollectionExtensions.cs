using System.Reflection;
using Guardou.Application.Authentication;
using Guardou.Application.Configuration;
using Guardou.Application.Files;
using Guardou.Application.Previews;
using Guardou.Application.Settings;
using Guardou.Application.Shares;
using Guardou.Application.Tasks;
using Guardou.Application.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Guardou.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<TaskSignal>();
        services.AddScoped<ITaskHandler, PreviewTaskHandler>();
        services.AddScoped<RuntimeConfigurationQuery>();
        services.AddScoped<UserAdministration>();
        services.AddScoped<UserAccounts>();
        services.AddScoped<ProfilePictures>();
        services.AddScoped<Registration>();
        services.AddScoped<PasswordLogin>();
        services.AddScoped<ExternalSignIn>();
        services.AddScoped<OidcSettingsAdministration>();
        services.AddScoped<ServerSettingsAdministration>();
        services.AddScoped<OwnedItems>();
        services.AddScoped<ItemEntryFactory>();
        services.AddScoped<ItemPlacement>();
        services.AddScoped<ItemTree>();
        services.AddScoped<ItemRemover>();
        services.AddScoped<ShareCleanup>();
        AddScopedWithInternalConstructor<ItemBrowser>(services);
        AddScopedWithInternalConstructor<ItemOrganizer>(services);
        AddScopedWithInternalConstructor<FileUploader>(services);
        AddScopedWithInternalConstructor<TrashService>(services);
        AddScopedWithInternalConstructor<ItemDownloads>(services);
        AddScopedWithInternalConstructor<PreviewTasks>(services);
        services.AddScoped<ITaskProducer>(provider => provider.GetRequiredService<PreviewTasks>());
        AddScopedWithInternalConstructor<ItemPreviews>(services);
        AddScopedWithInternalConstructor<ItemSharing>(services);
        AddScopedWithInternalConstructor<SharedItems>(services);
        AddScopedWithInternalConstructor<TaskRunner>(services);
        AddScopedWithInternalConstructor<QueueSettings>(services);
        AddScopedWithInternalConstructor<TaskAdministration>(services);
        return services;
    }

    private static void AddScopedWithInternalConstructor<TService>(IServiceCollection services)
        where TService : class
    {
        ConstructorInfo constructor = typeof(TService).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
        Type[] parameterTypes = constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
        services.AddScoped(provider => (TService)constructor.Invoke(parameterTypes.Select(provider.GetRequiredService).ToArray()));
    }
}
