namespace Guardou.Application.Settings;

public sealed record ServerSettingsView(string? ExternalUrl, int TrashRetentionDays, IReadOnlyList<LockedSetting> Locked);
