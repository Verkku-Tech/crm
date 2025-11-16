using System.ComponentModel.DataAnnotations;

namespace CRM.Api.Models;

public class Contact
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    [MaxLength(200)]
    public string? InstagramUsername { get; set; }

    [MaxLength(200)]
    public string? InstagramId { get; set; }

    [MaxLength(200)]
    public string? FacebookId { get; set; }

    [MaxLength(200)]
    public string? WhatsAppNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastContactedAt { get; set; }

    public string? Notes { get; set; }

    // Navigation properties
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}

