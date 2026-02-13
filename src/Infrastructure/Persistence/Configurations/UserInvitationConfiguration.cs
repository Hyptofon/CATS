using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserInvitationConfiguration : IEntityTypeConfiguration<UserInvitation>
{
    public void Configure(EntityTypeBuilder<UserInvitation> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Email)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(t => t.Email);
        builder.Property(t => t.CreatedById).IsRequired(false);
    }
}
