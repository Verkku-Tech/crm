using CRM.Api.Data;
using CRM.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<ContactsController> _logger;

    public ContactsController(CrmDbContext context, ILogger<ContactsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all contacts
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Contact>>> GetContacts()
    {
        var contacts = await _context.Contacts
            .Include(c => c.Conversations)
            .OrderByDescending(c => c.LastContactedAt ?? c.CreatedAt)
            .ToListAsync();

        return Ok(contacts);
    }

    /// <summary>
    /// Get contact by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Contact>> GetContact(Guid id)
    {
        var contact = await _context.Contacts
            .Include(c => c.Conversations)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contact == null)
        {
            return NotFound();
        }

        return Ok(contact);
    }

    /// <summary>
    /// Create a new contact
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Contact>> CreateContact([FromBody] Contact contact)
    {
        contact.Id = Guid.NewGuid();
        contact.CreatedAt = DateTime.UtcNow;

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, contact);
    }

    /// <summary>
    /// Update contact
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateContact(Guid id, [FromBody] Contact contact)
    {
        if (id != contact.Id)
        {
            return BadRequest();
        }

        _context.Entry(contact).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ContactExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Delete contact
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteContact(Guid id)
    {
        var contact = await _context.Contacts.FindAsync(id);
        if (contact == null)
        {
            return NotFound();
        }

        _context.Contacts.Remove(contact);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> ContactExists(Guid id)
    {
        return await _context.Contacts.AnyAsync(e => e.Id == id);
    }
}

