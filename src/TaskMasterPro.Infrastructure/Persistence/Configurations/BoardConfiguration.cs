using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskMasterPro.Domain.Entities;

namespace TaskMasterPro.Infrastructure.Persistence.Configurations;

public class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Title).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Description).HasMaxLength(500);

        // One-to-Many Relationship with User
        builder.HasOne(b => b.User)
            .WithMany(u => u.Boards)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Not strictly specified for user delete, but user is principal
    }
}
