using System.Text.Json.Serialization;

namespace CRM.Api.DTOs.Instagram;

public class SendMessageDto
{
    [JsonPropertyName("recipient")]
    public RecipientDto Recipient { get; set; } = new();

    [JsonPropertyName("message")]
    public MessageContentDto Message { get; set; } = new();
}

public class RecipientDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

public class MessageContentDto
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("attachment")]
    public AttachmentDto? Attachment { get; set; }
}

public class AttachmentDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public AttachmentPayloadDto Payload { get; set; } = new();
}

public class AttachmentPayloadDto
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

