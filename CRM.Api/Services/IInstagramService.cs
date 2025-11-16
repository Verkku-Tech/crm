using CRM.Api.DTOs.Instagram;

namespace CRM.Api.Services;

public interface IInstagramService
{
    Task<InstagramUserInfo?> GetUserInfo(string userId);
    Task<bool> SendMessage(string recipientId, string message, string? accessToken = null);
    Task<bool> SendMediaMessage(string recipientId, string mediaType, string mediaUrl, string? accessToken = null);
    Task<InstagramConversationResponse?> GetConversationHistory(string conversationId, string? accessToken = null);
}

public class InstagramUserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? ProfilePicUrl { get; set; }
}

public class InstagramConversationResponse
{
    public List<InstagramMessageResponse> Messages { get; set; } = new();
}

public class InstagramMessageResponse
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string CreatedTime { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
}

