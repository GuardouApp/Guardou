using System.Reflection;
using Guardou.Application.Configuration;
using Guardou.Application.Files;
using Guardou.Application.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Guardou.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<RuntimeConfigurationQuery>();
        services.AddScoped<UserAdministration>();
        services.AddScoped<UserAccounts>();
        services.AddScoped<Registration>();
        services.AddScoped<OwnedItems>();
        services.AddScoped<ItemEntryFactory>();
        services.AddScoped<ItemPlacement>();
        services.AddScoped<ItemTree>();
        services.AddScoped<ItemRemover>();
        AddScopedWithInternalConstructor<ItemBrowser>(services);
        AddScopedWithInternalConstructor<ItemOrganizer>(services);
        AddScopedWithInternalConstructor<FileUploader>(services);
        AddScopedWithInternalConstructor<TrashService>(services);
        AddScopedWithInternalConstructor<ItemDownloads>(services);
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
