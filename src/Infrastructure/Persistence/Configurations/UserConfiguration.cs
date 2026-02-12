using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Email)
            .HasMaxLength(200)
            .IsRequired();
            
        // Робимо індекс по Email, щоб швидко шукати при логіні
        builder.HasIndex(t => t.Email).IsUnique();

        builder.Property(t => t.FirstName).HasMaxLength(100);
        builder.Property(t => t.LastName).HasMaxLength(100);
    }
}