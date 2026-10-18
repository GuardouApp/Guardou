namespace Guardou.Application.Users;

public sealed record RegistrationResult(RegistrationStatus Status, IReadOnlyList<string> Errors)
{
    public static RegistrationResult Registered { get; } = new(RegistrationStatus.Registered, []);

    public static RegistrationResult Closed { get; } = new(RegistrationStatus.Closed, []);

    public static RegistrationResult Rejected(IReadOnlyList<string> errors)
    {
        return new RegistrationResult(RegistrationStatus.Rejected, errors);
    }
}
