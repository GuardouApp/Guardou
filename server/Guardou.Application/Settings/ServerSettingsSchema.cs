using System.Globalization;
using Guardou.Domain.Files;
using Guardou.Domain.Settings;

namespace Guardou.Application.Settings;

internal sealed class ServerSettingsSchema : ISettingsSchema<ServerSettings>
{
    public IReadOnlyList<SettingField<ServerSettings>> Fields { get; } =
    [
        new(
            nameof(ServerSettings.ExternalUrl),
            settings => settings.ExternalUrl,
            new(SettingsVariables.ExternalUrl, overrides => overrides.ExternalUrl is not null, (target, source) => target with { ExternalUrl = source.ExternalUrl })),
        new(
            nameof(ServerSettings.TrashRetentionDays),
            settings => settings.TrashRetentionDays.ToString(CultureInfo.InvariantCulture),
            new(SettingsVariables.TrashRetentionDays, overrides => overrides.TrashRetentionDays is not null, (target, source) => target with { TrashRetentionDays = source.TrashRetentionDays })),
        new(
            nameof(ServerSettings.UploadConcurrency),
            settings => settings.UploadConcurrency.ToString(CultureInfo.InvariantCulture),
            new(SettingsVariables.UploadConcurrency, overrides => overrides.UploadConcurrency is not null, (target, source) => target with { UploadConcurrency = source.UploadConcurrency })),
        new(
            nameof(ServerSettings.UploadChunkSizeMb),
            settings => settings.UploadChunkSizeMb.ToString(CultureInfo.InvariantCulture),
            new(SettingsVariables.UploadChunkSizeMb, overrides => overrides.UploadChunkSizeMb is not null, (target, source) => target with { UploadChunkSizeMb = source.UploadChunkSizeMb })),
    ];

    public static ServerSettings Normalize(ServerSettings settings)
    {
        if (!TrashPolicy.IsValid(settings.TrashRetentionDays))
        {
            throw new InvalidSettingsException(
                $"The trash retention must be from {TrashPolicy.MinimumRetentionDays} to {TrashPolicy.MaximumRetentionDays} days.");
        }

        if (!UploadPolicy.IsValidConcurrency(settings.UploadConcurrency))
        {
            throw new InvalidSettingsException(
                $"The upload concurrency must be from {UploadPolicy.MinimumConcurrency} to {UploadPolicy.MaximumConcurrency}.");
        }

        if (!UploadPolicy.IsValidChunkSize(settings.UploadChunkSizeMb))
        {
            throw new InvalidSettingsException(
                $"The upload chunk size must be from {UploadPolicy.MinimumChunkSizeMb} to {UploadPolicy.MaximumChunkSizeMb} MB.");
        }

        return settings with { ExternalUrl = SettingsUrl.Normalize(settings.ExternalUrl, "external URL") };
    }

    public ServerSettings FromEnvironment(SettingsOverrides overrides)
    {
        return Normalize(new ServerSettings(
            overrides.ExternalUrl,
            overrides.TrashRetentionDays ?? ServerSettings.Initial.TrashRetentionDays,
            overrides.UploadConcurrency ?? ServerSettings.Initial.UploadConcurrency,
            overrides.UploadChunkSizeMb ?? ServerSettings.Initial.UploadChunkSizeMb));
    }
}
