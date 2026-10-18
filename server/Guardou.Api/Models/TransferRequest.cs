using System.ComponentModel.DataAnnotations;
using Guardou.Application.Files;

namespace Guardou.Api.Models;

public sealed record TransferRequest([Required] string DestinationId, ConflictPolicy Conflict = ConflictPolicy.Fail);
