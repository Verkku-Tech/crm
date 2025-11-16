using CRM.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Data;

public class CrmDbContext : DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
    {
    }

    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<InstagramAccount> InstagramAccounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Contact configuration
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasIndex(e => e.InstagramId);
            entity.HasIndex(e => e.FacebookId);
            entity.HasIndex(e => e.Email);
        });

        // Conversation configuration
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasIndex(e => new { e.ContactId, e.Platform });
            entity.HasIndex(e => e.PlatformConversationId);
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.Contact)
                .WithMany(e => e.Conversations)
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Message configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasIndex(e => e.PlatformMessageId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.ConversationId, e.Timestamp });

            entity.HasOne(e => e.Conversation)
                .WithMany(e => e.Messages)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // InstagramAccount configuration
        modelBuilder.Entity<InstagramAccount>(entity =>
        {
            entity.HasIndex(e => e.InstagramBusinessAccountId).IsUnique();
            entity.HasIndex(e => e.Username);
        });
    }
}

