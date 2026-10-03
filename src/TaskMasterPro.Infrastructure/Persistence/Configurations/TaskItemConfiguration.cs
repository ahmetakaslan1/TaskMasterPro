using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskMasterPro.Domain.Entities;

namespace TaskMasterPro.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(2000);
        builder.Property(t => t.Priority).IsRequired(); // Enum mapped as int by default in EF Core
        builder.Property(t => t.OrderIndex).IsRequired();

        // One-to-Many Relationship with BoardColumn (Cascade delete)
        builder.HasOne(t => t.BoardColumn)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.BoardColumnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
