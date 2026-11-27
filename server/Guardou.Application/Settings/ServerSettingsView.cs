namespace Guardou.Application.Settings;

public sealed record ServerSettingsView(
    string? ExternalUrl,
    int TrashRetentionDays,
    int UploadConcurrency,
    int UploadChunkSizeMb,
    bool Signup,
    bool UpdateCheck,
    IReadOnlyList<LockedSetting> Locked);
