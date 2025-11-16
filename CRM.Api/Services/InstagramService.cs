using CRM.Api.Data;
using CRM.Api.DTOs.Instagram;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CRM.Api.Services;

public class InstagramService : IInstagramService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InstagramService> _logger;
    private readonly CrmDbContext _context;
    private const string GraphApiBaseUrl = "https://graph.facebook.com/v18.0";

    public InstagramService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<InstagramService> logger,
        CrmDbContext context)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    public async Task<InstagramUserInfo?> GetUserInfo(string userId)
    {
        try
        {
            var accessToken = await GetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("No access token available for Instagram");
                return null;
            }

            var url = $"{GraphApiBaseUrl}/{userId}?fields=id,username,name,profile_pic&access_token={accessToken}";
            
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<InstagramUserInfo>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
                
                return userInfo;
            }
            
            _logger.LogWarning("Failed to get user info for {UserId}. Status: {StatusCode}", 
                userId, response.StatusCode);
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Instagram user info for {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> SendMessage(string recipientId, string message, string? accessToken = null)
    {
        try
        {
            accessToken ??= await GetAccessToken();
            
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token available for sending message");
                return false;
            }

            var messageDto = new SendMessageDto
            {
                Recipient = new RecipientDto { Id = recipientId },
                Message = new MessageContentDto { Text = message }
            };

            var url = $"{GraphApiBaseUrl}/me/messages?access_token={accessToken}";
            
            var jsonContent = JsonSerializer.Serialize(messageDto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Message sent successfully to {RecipientId}", recipientId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send message. Status: {StatusCode}, Error: {Error}", 
                response.StatusCode, errorContent);
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Instagram message to {RecipientId}", recipientId);
            return false;
        }
    }

    public async Task<bool> SendMediaMessage(string recipientId, string mediaType, string mediaUrl, string? accessToken = null)
    {
        try
        {
            accessToken ??= await GetAccessToken();
            
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token available for sending media message");
                return false;
            }

            var messageDto = new SendMessageDto
            {
                Recipient = new RecipientDto { Id = recipientId },
                Message = new MessageContentDto 
                { 
                    Attachment = new AttachmentDto 
                    { 
                        Type = mediaType.ToLower(),
                        Payload = new AttachmentPayloadDto { Url = mediaUrl }
                    }
                }
            };

            var url = $"{GraphApiBaseUrl}/me/messages?access_token={accessToken}";
            
            var jsonContent = JsonSerializer.Serialize(messageDto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Media message sent successfully to {RecipientId}", recipientId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send media message. Status: {StatusCode}, Error: {Error}", 
                response.StatusCode, errorContent);
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Instagram media message to {RecipientId}", recipientId);
            return false;
        }
    }

    public async Task<InstagramConversationResponse?> GetConversationHistory(string conversationId, string? accessToken = null)
    {
        try
        {
            accessToken ??= await GetAccessToken();
            
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("No access token available for fetching conversation");
                return null;
            }

            var url = $"{GraphApiBaseUrl}/{conversationId}/messages?fields=id,message,created_time,from&access_token={accessToken}";
            
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var conversationResponse = JsonSerializer.Deserialize<InstagramConversationResponse>(content, 
                    new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                
                return conversationResponse;
            }

            _logger.LogWarning("Failed to get conversation history for {ConversationId}. Status: {StatusCode}", 
                conversationId, response.StatusCode);
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Instagram conversation history for {ConversationId}", conversationId);
            return null;
        }
    }

    private async Task<string?> GetAccessToken()
    {
        // First try to get from configuration
        var configToken = _configuration["Instagram:PageAccessToken"];
        if (!string.IsNullOrEmpty(configToken))
        {
            return configToken;
        }

        // Otherwise, get from the first active Instagram account in database
        var account = await _context.InstagramAccounts
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.ConnectedAt)
            .FirstOrDefaultAsync();

        return account?.AccessToken;
    }
}

