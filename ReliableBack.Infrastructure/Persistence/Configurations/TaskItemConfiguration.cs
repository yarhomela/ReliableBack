using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.Type)
            .HasColumnName("type")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(t => t.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired()
            .HasConversion(
                v => v.RootElement.GetRawText(),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));
        
        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasColumnName("priority")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(t => t.RetryCount)
            .HasColumnName("retry_count")
            .HasDefaultValue(0);

        builder.Property(t => t.MaxRetries)
            .HasColumnName("max_retries")
            .HasDefaultValue(3);

        builder.Property(t => t.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.Property(t => t.ScheduledAt)
            .HasColumnName("scheduled_at");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
        
        builder.HasIndex(t => t.Status)
            .HasDatabaseName("ix_tasks_status");

        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("ix_tasks_created_at");

        builder.HasIndex(t => new { t.Status, t.Priority })
            .HasDatabaseName("ix_tasks_status_priority");
    }
}