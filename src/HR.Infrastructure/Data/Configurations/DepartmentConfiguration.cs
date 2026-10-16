using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Code).HasMaxLength(20).IsRequired();
        builder.Property(d => d.NameAr).HasMaxLength(150).IsRequired();
        builder.Property(d => d.NameEn).HasMaxLength(150).IsRequired();
        builder.Property(d => d.DescriptionAr).HasMaxLength(500);
        builder.Property(d => d.DescriptionEn).HasMaxLength(500);

        builder.HasIndex(d => d.Code).IsUnique();

        builder.HasOne(d => d.Manager)
            .WithMany()
            .HasForeignKey(d => d.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
