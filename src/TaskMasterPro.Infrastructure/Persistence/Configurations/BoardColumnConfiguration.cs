using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskMasterPro.Domain.Entities;

namespace TaskMasterPro.Infrastructure.Persistence.Configurations;

public class BoardColumnConfiguration : IEntityTypeConfiguration<BoardColumn>
{
    public void Configure(EntityTypeBuilder<BoardColumn> builder)
    {
        // Spec 4: Entity adı Column DEĞİL, BoardColumn olmalıdır. Tablo adı: BoardColumns.
        builder.ToTable("BoardColumns");
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Title).IsRequired().HasMaxLength(100);
        builder.Property(c => c.OrderIndex).IsRequired();

        // One-to-Many Relationship with Board (Cascade delete)
        builder.HasOne(c => c.Board)
            .WithMany(b => b.Columns)
            .HasForeignKey(c => c.BoardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
