using System.Text.Json.Serialization;

namespace CRM.Api.DTOs.Instagram;

public class InstagramWebhookDto
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("entry")]
    public List<InstagramWebhookEntry> Entry { get; set; } = new();
}

public class InstagramWebhookEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("messaging")]
    public List<InstagramMessaging>? Messaging { get; set; }

    [JsonPropertyName("changes")]
    public List<InstagramChange>? Changes { get; set; }
}

public class InstagramMessaging
{
    [JsonPropertyName("sender")]
    public InstagramUser Sender { get; set; } = new();

    [JsonPropertyName("recipient")]
    public InstagramUser Recipient { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("message")]
    public InstagramMessageData? Message { get; set; }

    [JsonPropertyName("postback")]
    public InstagramPostback? Postback { get; set; }
}

public class InstagramUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

public class InstagramMessageData
{
    [JsonPropertyName("mid")]
    public string Mid { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("attachments")]
    public List<InstagramAttachment>? Attachments { get; set; }

    [JsonPropertyName("is_echo")]
    public bool IsEcho { get; set; }
}

public class InstagramAttachment
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // image, video, audio, file

    [JsonPropertyName("payload")]
    public InstagramAttachmentPayload Payload { get; set; } = new();
}

public class InstagramAttachmentPayload
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class InstagramPostback
{
    [JsonPropertyName("mid")]
    public string Mid { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public string Payload { get; set; } = string.Empty;
}

public class InstagramChange
{
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public object? Value { get; set; }
}

// DTO para el valor cuando field = "messages" en changes
public class InstagramMessageChangeValue
{
    [JsonPropertyName("sender")]
    public InstagramUser Sender { get; set; } = new();

    [JsonPropertyName("recipient")]
    public InstagramUser Recipient { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty; // Puede venir como string

    [JsonPropertyName("message")]
    public InstagramMessageData? Message { get; set; }
}

