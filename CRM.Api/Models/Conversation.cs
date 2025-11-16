using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Api.Models;

public class Conversation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ContactId { get; set; }

    [ForeignKey(nameof(ContactId))]
    public Contact Contact { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Platform { get; set; } = string.Empty; // Instagram, Facebook, WhatsApp

    [MaxLength(200)]
    public string? PlatformConversationId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastMessageAt { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Active"; // Active, Archived, Closed

    public int UnreadCount { get; set; } = 0;

    // Navigation properties
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

