using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options)
        : base(options)
    {
    }

    public DbSet<Participant> Participants => Set<Participant>();

    public DbSet<ChatGroup> ChatGroups => Set<ChatGroup>();

    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.ToTable("participants");

            entity.HasKey(e => e.ParticipantId);

            entity.Property(e => e.ParticipantId)
                .HasColumnName("participant_id");

            entity.Property(e => e.DisplayName)
                .HasColumnName("display_name")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()")
                .IsRequired();
        });

        modelBuilder.Entity<ChatGroup>(entity =>
        {
            entity.ToTable("chat_groups");

            entity.HasKey(e => e.GroupId);

            entity.Property(e => e.GroupId)
                .HasColumnName("group_id");

            entity.Property(e => e.GroupName)
                .HasColumnName("group_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description");

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            entity.HasOne(e => e.Creator)
                .WithMany(e => e.CreatedGroups)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.CreatedBy)
                .HasDatabaseName("idx_chat_groups_created_by");
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.ToTable("group_members");

            entity.HasKey(e => e.GroupMemberId);

            entity.Property(e => e.GroupMemberId)
                .HasColumnName("group_member_id");

            entity.Property(e => e.GroupId)
                .HasColumnName("group_id")
                .IsRequired();

            entity.Property(e => e.ParticipantId)
                .HasColumnName("participant_id")
                .IsRequired();

            entity.Property(e => e.JoinedAt)
                .HasColumnName("joined_at")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            entity.HasOne(e => e.Group)
                .WithMany(e => e.Members)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Participant)
                .WithMany(e => e.GroupMemberships)
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.GroupId, e.ParticipantId })
                .IsUnique();

            entity.HasIndex(e => e.GroupId)
                .HasDatabaseName("idx_group_members_group_id");

            entity.HasIndex(e => e.ParticipantId)
                .HasDatabaseName("idx_group_members_participant_id");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("messages");

            entity.HasKey(e => e.MessageId);

            entity.Property(e => e.MessageId)
                .HasColumnName("message_id");

            entity.Property(e => e.GroupId)
                .HasColumnName("group_id")
                .IsRequired();

            entity.Property(e => e.SenderId)
                .HasColumnName("sender_id")
                .IsRequired();

            entity.Property(e => e.MessageText)
                .HasColumnName("message_text");

            entity.Property(e => e.MessageType)
                .HasColumnName("message_type")
                .HasMaxLength(20)
                .HasDefaultValue("text")
                .IsRequired();

            entity.Property(e => e.SentAt)
                .HasColumnName("sent_at")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            entity.HasOne(e => e.Group)
                .WithMany(e => e.Messages)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Sender)
                .WithMany(e => e.SentMessages)
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.GroupId, e.SentAt })
                .HasDatabaseName("idx_messages_group_id_sent_at");

            entity.HasIndex(e => e.SenderId)
                .HasDatabaseName("idx_messages_sender_id");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("chk_message_type", "message_type IN ('text', 'image', 'file')");
                t.HasCheckConstraint("chk_message_content", "message_text IS NOT NULL OR message_type IN ('image', 'file')");
            });
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.ToTable("attachments");

            entity.HasKey(e => e.AttachmentId);

            entity.Property(e => e.AttachmentId)
                .HasColumnName("attachment_id");

            entity.Property(e => e.MessageId)
                .HasColumnName("message_id")
                .IsRequired();

            entity.Property(e => e.OriginalFileName)
                .HasColumnName("original_file_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.StoredFileName)
                .HasColumnName("stored_file_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.FilePath)
                .HasColumnName("file_path")
                .IsRequired();

            entity.Property(e => e.FileType)
                .HasColumnName("file_type")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.FileSize)
                .HasColumnName("file_size")
                .IsRequired();

            entity.Property(e => e.UploadedAt)
                .HasColumnName("uploaded_at")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            entity.HasOne(e => e.Message)
                .WithMany(e => e.Attachments)
                .HasForeignKey(e => e.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.MessageId)
                .HasDatabaseName("idx_attachments_message_id");
        });
    }
}
