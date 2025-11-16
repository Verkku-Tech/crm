using CRM.Api.Data;
using CRM.Api.DTOs.Instagram;
using CRM.Api.Models;
using CRM.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/webhooks/instagram")]
public class InstagramWebhookController : ControllerBase
{
    private readonly ILogger<InstagramWebhookController> _logger;
    private readonly IConfiguration _configuration;
    private readonly CrmDbContext _context;
    private readonly IInstagramService _instagramService;

    public InstagramWebhookController(
        ILogger<InstagramWebhookController> logger,
        IConfiguration configuration,
        CrmDbContext context,
        IInstagramService instagramService)
    {
        _logger = logger;
        _configuration = configuration;
        _context = context;
        _instagramService = instagramService;
    }

    /// <summary>
    /// Webhook verification endpoint (GET)
    /// Instagram/Facebook will call this to verify the webhook
    /// </summary>
    [HttpGet]
    public IActionResult VerifyWebhook([FromQuery(Name = "hub.mode")] string mode,
                                       [FromQuery(Name = "hub.challenge")] string challenge,
                                       [FromQuery(Name = "hub.verify_token")] string verifyToken)
    {
        _logger.LogInformation("Webhook verification requested. Mode: {Mode}, Token: {Token}", mode, verifyToken);

        var expectedToken = _configuration["Instagram:VerifyToken"];

        if (mode == "subscribe" && verifyToken == expectedToken)
        {
            _logger.LogInformation("Webhook verified successfully");
            return Content(challenge, "text/plain");
        }

        _logger.LogWarning("Webhook verification failed");
        return Forbid();
    }

    /// <summary>
    /// Webhook endpoint for receiving messages (POST)
    /// Instagram/Facebook will send messages to this endpoint
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook([FromBody] InstagramWebhookDto webhook)
    {
        try
        {
            _logger.LogInformation("Received webhook: {Webhook}", JsonSerializer.Serialize(webhook));

            if (webhook.Object != "instagram" && webhook.Object != "page")
            {
                _logger.LogWarning("Unexpected webhook object type: {Type}", webhook.Object);
                return Ok(); // Return 200 to acknowledge receipt
            }

            foreach (var entry in webhook.Entry)
            {
                // Process messaging events (direct messages)
                if (entry.Messaging != null)
                {
                    foreach (var messaging in entry.Messaging)
                    {
                        await ProcessMessage(messaging);
                    }
                }

                // Process changes events (comments, mentions, etc.)
                if (entry.Changes != null)
                {
                    foreach (var change in entry.Changes)
                    {
                        _logger.LogInformation("Received change event: {Field}", change.Field);
                        
                        // Process messages from changes
                        if (change.Field == "messages" && change.Value != null)
                        {
                            await ProcessMessageFromChange(change.Value);
                        }
                        // Handle other types of changes (comments, mentions, etc.)
                    }
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return Ok(); // Still return 200 to avoid retries
        }
    }

    private async Task ProcessMessage(InstagramMessaging messaging)
    {
        try
        {
            // Skip echo messages (messages sent by us)
            if (messaging.Message?.IsEcho == true)
            {
                _logger.LogInformation("Skipping echo message");
                return;
            }

            // Check if message exists
            if (messaging.Message == null)
            {
                _logger.LogInformation("No message content found");
                return;
            }

            var senderId = messaging.Sender.Id;
            var messageId = messaging.Message.Mid;
            var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(messaging.Timestamp).UtcDateTime;

            // Check if message already exists
            var existingMessage = await _context.Messages
                .FirstOrDefaultAsync(m => m.PlatformMessageId == messageId);

            if (existingMessage != null)
            {
                _logger.LogInformation("Message already exists: {MessageId}", messageId);
                return;
            }

            // Get or create contact
            var contact = await GetOrCreateContact(senderId);

            // Get or create conversation
            var conversation = await GetOrCreateConversation(contact, senderId);

            // Determine message content and type
            string content = messaging.Message.Text ?? string.Empty;
            string messageType = "Text";
            string? mediaUrl = null;

            if (messaging.Message.Attachments != null && messaging.Message.Attachments.Any())
            {
                var attachment = messaging.Message.Attachments.First();
                messageType = attachment.Type switch
                {
                    "image" => "Image",
                    "video" => "Video",
                    "audio" => "Audio",
                    _ => "File"
                };
                mediaUrl = attachment.Payload.Url;
                content = $"[{messageType} attachment]";
            }

            // Create message record
            var message = new Message
            {
                ConversationId = conversation.Id,
                PlatformMessageId = messageId,
                Platform = "Instagram",
                Content = content,
                MessageType = messageType,
                Direction = "Inbound",
                Timestamp = timestamp,
                IsRead = false,
                MediaUrl = mediaUrl,
                Metadata = JsonSerializer.Serialize(messaging)
            };

            _context.Messages.Add(message);

            // Update conversation
            conversation.LastMessageAt = timestamp;
            conversation.UnreadCount++;

            // Update contact
            contact.LastContactedAt = timestamp;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Message saved successfully. Contact: {ContactId}, Conversation: {ConversationId}",
                contact.Id, conversation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
        }
    }

    /// <summary>
    /// Procesa mensajes que vienen en el formato changes (field = "messages")
    /// </summary>
    private async Task ProcessMessageFromChange(object changeValue)
    {
        try
        {
            // Deserializar el valor del change
            var changeValueJson = JsonSerializer.Serialize(changeValue);
            var messageChange = JsonSerializer.Deserialize<InstagramMessageChangeValue>(
                changeValueJson, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (messageChange == null || messageChange.Message == null)
            {
                _logger.LogWarning("Invalid message change value structure");
                return;
            }

            // Convertir el timestamp (puede venir como string o número)
            long timestampLong;
            if (long.TryParse(messageChange.Timestamp, out var parsedTimestamp))
            {
                timestampLong = parsedTimestamp;
            }
            else if (messageChange.Timestamp.All(char.IsDigit) && messageChange.Timestamp.Length > 0)
            {
                timestampLong = long.Parse(messageChange.Timestamp);
            }
            else
            {
                _logger.LogWarning("Invalid timestamp format: {Timestamp}", messageChange.Timestamp);
                timestampLong = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            var senderId = messageChange.Sender.Id;
            var messageId = messageChange.Message.Mid;
            var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestampLong).UtcDateTime;

            // Check if message already exists
            var existingMessage = await _context.Messages
                .FirstOrDefaultAsync(m => m.PlatformMessageId == messageId);

            if (existingMessage != null)
            {
                _logger.LogInformation("Message already exists: {MessageId}", messageId);
                return;
            }

            // Get or create contact
            var contact = await GetOrCreateContact(senderId);

            // Get or create conversation
            var conversation = await GetOrCreateConversation(contact, senderId);

            // Determine message content and type
            string content = messageChange.Message.Text ?? string.Empty;
            string messageType = "Text";
            string? mediaUrl = null;

            if (messageChange.Message.Attachments != null && messageChange.Message.Attachments.Any())
            {
                var attachment = messageChange.Message.Attachments.First();
                messageType = attachment.Type switch
                {
                    "image" => "Image",
                    "video" => "Video",
                    "audio" => "Audio",
                    _ => "File"
                };
                mediaUrl = attachment.Payload.Url;
                content = $"[{messageType} attachment]";
            }

            // Create message record
            var message = new Message
            {
                ConversationId = conversation.Id,
                PlatformMessageId = messageId,
                Platform = "Instagram",
                Content = content,
                MessageType = messageType,
                Direction = "Inbound",
                Timestamp = timestamp,
                IsRead = false,
                MediaUrl = mediaUrl,
                Metadata = changeValueJson
            };

            _context.Messages.Add(message);

            // Update conversation
            conversation.LastMessageAt = timestamp;
            conversation.UnreadCount++;

            // Update contact
            contact.LastContactedAt = timestamp;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Message from change saved successfully. Contact: {ContactId}, Conversation: {ConversationId}, MessageId: {MessageId}",
                contact.Id, conversation.Id, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message from change");
        }
    }

    private async Task<Contact> GetOrCreateContact(string instagramId)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.InstagramId == instagramId);

        if (contact == null)
        {
            // Fetch Instagram user info
            var userInfo = await _instagramService.GetUserInfo(instagramId);

            // Asegurar que no lanzamos excepción si el instagramId es más corto de 8 caracteres
            var shortInstagramId = instagramId.Length > 8
                ? instagramId[..8]
                : instagramId;

            contact = new Contact
            {
                InstagramId = instagramId,
                Name = userInfo?.Username ?? $"Instagram User {shortInstagramId}",
                InstagramUsername = userInfo?.Username,
                CreatedAt = DateTime.UtcNow
            };

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new contact: {ContactId} for Instagram ID: {InstagramId}",
                contact.Id, instagramId);
        }

        return contact;
    }

    private async Task<Conversation> GetOrCreateConversation(Contact contact, string platformConversationId)
    {
        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.ContactId == contact.Id
                                   && c.Platform == "Instagram"
                                   && c.PlatformConversationId == platformConversationId);

        if (conversation == null)
        {
            conversation = new Conversation
            {
                ContactId = contact.Id,
                Platform = "Instagram",
                PlatformConversationId = platformConversationId,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new conversation: {ConversationId} for contact: {ContactId}",
                conversation.Id, contact.Id);
        }

        return conversation;
    }
}

