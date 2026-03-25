using System.ComponentModel.DataAnnotations;

namespace CaManagement.Api.DTOs.Audit;

public sealed class PersistAuditReportRequest
{
    [Required, MinLength(5), MaxLength(200)]
    public required string Title { get; init; }
}
