namespace Guardou.Application.Settings;

public sealed record ServerSettingsView(
    string? ExternalUrl,
    int TrashRetentionDays,
    int UploadConcurrency,
    int UploadChunkSizeMb,
    IReadOnlyList<LockedSetting> Locked);
