using System.ComponentModel.DataAnnotations;

namespace CRM.Api.Models;

public class InstagramAccount
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string InstagramBusinessAccountId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string AccessToken { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? PageId { get; set; }

    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

    public DateTime? TokenExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Metadata { get; set; } // JSON string for additional data
}

