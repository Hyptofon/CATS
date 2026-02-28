using Domain.Containers;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ContainerFillConfiguration : IEntityTypeConfiguration<ContainerFill>
{
    public void Configure(EntityTypeBuilder<ContainerFill> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ContainerId)
            .IsRequired();

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasColumnType("decimal(18,3)")
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasColumnType("varchar(10)")
            .IsRequired();

        builder.Property(x => x.ProductionDate)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired();

        builder.Property(x => x.FilledDate)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired();

        builder.Property(x => x.ExpirationDate)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired();

        builder.Property(x => x.EmptiedDate)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired(false);

        builder.Property(x => x.FilledByUserId)
            .IsRequired();

        builder.Property(x => x.EmptiedByUserId)
            .IsRequired(false);

        builder.HasOne(x => x.Container)
            .WithMany()
            .HasForeignKey(x => x.ContainerId)
            .HasConstraintName("fk_container_fills_containers_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .HasConstraintName("fk_container_fills_products_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FilledByUser)
            .WithMany()
            .HasForeignKey(x => x.FilledByUserId)
            .HasConstraintName("fk_container_fills_users_filled_by_user_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EmptiedByUser)
            .WithMany()
            .HasForeignKey(x => x.EmptiedByUserId)
            .HasConstraintName("fk_container_fills_users_emptied_by_user_id")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ContainerId);
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.ProductionDate);
        builder.HasIndex(x => x.FilledDate);
        builder.HasIndex(x => x.ExpirationDate);
        builder.HasIndex(x => x.EmptiedDate);
    }
}
