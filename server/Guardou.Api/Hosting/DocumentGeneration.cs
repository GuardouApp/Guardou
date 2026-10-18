using System.Reflection;

namespace Guardou.Api.Hosting;

internal static class DocumentGeneration
{
    private const string GeneratorAssemblyName = "GetDocument.Insider";

    private static readonly Assembly WebHostAssembly = typeof(WebHostBuilderExtensions).Assembly;

    public static bool IsActive { get; } = Assembly.GetEntryAssembly()?.GetName().Name == GeneratorAssemblyName;

    public static void RemoveBackgroundServices(IServiceCollection services)
    {
        ServiceDescriptor[] backgroundServices = services
            .Where(descriptor => descriptor.ServiceType == typeof(IHostedService)
                && descriptor.ImplementationType?.Assembly != WebHostAssembly)
            .ToArray();

        foreach (ServiceDescriptor backgroundService in backgroundServices)
        {
            services.Remove(backgroundService);
        }
    }
}
