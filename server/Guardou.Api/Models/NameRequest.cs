using System.ComponentModel.DataAnnotations;

namespace Guardou.Api.Models;

public sealed record NameRequest([Required] string Name);
