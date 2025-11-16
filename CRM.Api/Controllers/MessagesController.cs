using CRM.Api.Data;
using CRM.Api.Models;
using CRM.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly IInstagramService _instagramService;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(
        CrmDbContext context,
        IInstagramService instagramService,
        ILogger<MessagesController> logger)
    {
        _context = context;
        _instagramService = instagramService;
        _logger = logger;
    }

    /// <summary>
    /// Get all conversations
    /// </summary>
    [HttpGet("conversations")]
    public async Task<ActionResult<IEnumerable<Conversation>>> GetConversations()
    {
        var conversations = await _context.Conversations
            .Include(c => c.Contact)
            .Include(c => c.Messages)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync();

        return Ok(conversations);
    }

    /// <summary>
    /// Get conversation by ID with messages
    /// </summary>
    [HttpGet("conversations/{id}")]
    public async Task<ActionResult<Conversation>> GetConversation(Guid id)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Contact)
            .Include(c => c.Messages.OrderBy(m => m.Timestamp))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (conversation == null)
        {
            return NotFound();
        }

        return Ok(conversation);
    }

    /// <summary>
    /// Get messages for a conversation
    /// </summary>
    [HttpGet("conversations/{conversationId}/messages")]
    public async Task<ActionResult<IEnumerable<Message>>> GetMessages(Guid conversationId)
    {
        var messages = await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        return Ok(messages);
    }

    /// <summary>
    /// Send a message
    /// </summary>
    [HttpPost("send")]
    public async Task<ActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Contact)
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId);

        if (conversation == null)
        {
            return NotFound("Conversation not found");
        }

        bool success = false;

        if (conversation.Platform == "Instagram")
        {
            if (string.IsNullOrEmpty(conversation.Contact.InstagramId))
            {
                return BadRequest("Contact does not have Instagram ID");
            }

            success = await _instagramService.SendMessage(conversation.Contact.InstagramId, request.Message);
        }
        else
        {
            return BadRequest($"Platform {conversation.Platform} is not supported yet");
        }

        if (!success)
        {
            return StatusCode(500, "Failed to send message");
        }

        // Save outbound message to database
        var message = new Message
        {
            ConversationId = conversation.Id,
            PlatformMessageId = Guid.NewGuid().ToString(), // Temporary ID
            Platform = conversation.Platform,
            Content = request.Message,
            MessageType = "Text",
            Direction = "Outbound",
            Timestamp = DateTime.UtcNow,
            IsRead = true
        };

        _context.Messages.Add(message);
        conversation.LastMessageAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(message);
    }

    /// <summary>
    /// Mark messages as read
    /// </summary>
    [HttpPost("conversations/{conversationId}/read")]
    public async Task<ActionResult> MarkAsRead(Guid conversationId)
    {
        var messages = await _context.Messages
            .Where(m => m.ConversationId == conversationId && !m.IsRead)
            .ToListAsync();

        foreach (var message in messages)
        {
            message.IsRead = true;
        }

        var conversation = await _context.Conversations.FindAsync(conversationId);
        if (conversation != null)
        {
            conversation.UnreadCount = 0;
        }

        await _context.SaveChangesAsync();

        return Ok();
    }
}

public class SendMessageRequest
{
    public Guid ConversationId { get; set; }
    public string Message { get; set; } = string.Empty;
}

