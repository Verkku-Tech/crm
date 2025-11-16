using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Api.Models;

public class Message
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ConversationId { get; set; }

    [ForeignKey(nameof(ConversationId))]
    public Conversation Conversation { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string PlatformMessageId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Platform { get; set; } = string.Empty; // Instagram, Facebook, WhatsApp

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(50)]
    public string MessageType { get; set; } = "Text"; // Text, Image, Video, Audio, File

    [Required]
    [MaxLength(50)]
    public string Direction { get; set; } = string.Empty; // Inbound, Outbound

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false;

    [MaxLength(500)]
    public string? MediaUrl { get; set; }

    public string? Metadata { get; set; } // JSON string for additional data
}

