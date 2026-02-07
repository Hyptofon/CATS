using Domain.Containers;
using Domain.ContainerTypes;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ContainerConfiguration : IEntityTypeConfiguration<Container>
{
    public void Configure(EntityTypeBuilder<Container> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Code)
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(x => x.Volume)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.ContainerTypeId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasColumnType("varchar(20)")
            .HasDefaultValue(ContainerStatus.Empty)
            .IsRequired();

        builder.Property(x => x.CurrentFillId)
            .IsRequired(false);

        builder.Property(x => x.CurrentProductId)
            .IsRequired(false);

        builder.Property(x => x.CurrentProductTypeId)
            .IsRequired(false);

        builder.Property(x => x.CurrentProductionDate)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired(false);

        builder.Property(x => x.CurrentQuantity)
            .HasColumnType("decimal(18,3)")
            .IsRequired(false);

        builder.Property(x => x.CurrentUnit)
            .HasColumnType("varchar(10)")
            .IsRequired(false);

        builder.Property(x => x.CurrentFilledAt)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired(false);

        builder.Property(x => x.CurrentExpirationDate)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired(false);

        builder.Property(x => x.LastProductId)
            .IsRequired(false);

        builder.Property(x => x.LastProductTypeId)
            .IsRequired(false);

        builder.Property(x => x.LastEmptiedAt)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired(false);

        builder.Property(x => x.Meta)
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .HasConversion(new DateTimeUtcConverter())
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired(false);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasOne(x => x.ContainerType)
            .WithMany()
            .HasForeignKey(x => x.ContainerTypeId)
            .HasConstraintName("fk_containers_container_types_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ContainerTypeId);
        builder.HasIndex(x => x.CurrentFillId).IsUnique();
        builder.HasIndex(x => x.CurrentProductTypeId);
        builder.HasIndex(x => x.CurrentProductId);
        builder.HasIndex(x => x.CurrentProductionDate);
        builder.HasIndex(x => x.LastProductTypeId);
        builder.HasIndex(x => x.LastProductId);
        builder.HasIndex(x => x.LastEmptiedAt);
        builder.HasIndex(x => x.IsDeleted);
    }
}