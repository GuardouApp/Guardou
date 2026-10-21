namespace Guardou.Application.Users;

public sealed record ProfilePicture(Stream Content, DateTimeOffset Modified);
