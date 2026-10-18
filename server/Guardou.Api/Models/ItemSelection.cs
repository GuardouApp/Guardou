using System.ComponentModel.DataAnnotations;

namespace Guardou.Api.Models;

public sealed record ItemSelection([Required] IReadOnlyList<string> Ids);
