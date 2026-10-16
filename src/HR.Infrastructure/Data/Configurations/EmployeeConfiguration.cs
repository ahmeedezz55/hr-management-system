using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EmployeeNumber).HasMaxLength(20).IsRequired();
        builder.Property(e => e.FirstNameAr).HasMaxLength(80).IsRequired();
        builder.Property(e => e.LastNameAr).HasMaxLength(80).IsRequired();
        builder.Property(e => e.FirstNameEn).HasMaxLength(80).IsRequired();
        builder.Property(e => e.LastNameEn).HasMaxLength(80).IsRequired();
        builder.Property(e => e.NationalId).HasMaxLength(10);
        builder.Property(e => e.IqamaNumber).HasMaxLength(15);
        builder.Property(e => e.Nationality).HasMaxLength(80).IsRequired();
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.Email).HasMaxLength(150);
        builder.Property(e => e.Address).HasMaxLength(250);
        builder.Property(e => e.City).HasMaxLength(80);
        builder.Property(e => e.JobTitleAr).HasMaxLength(120).IsRequired();
        builder.Property(e => e.JobTitleEn).HasMaxLength(120).IsRequired();
        builder.Property(e => e.BasicSalary).HasPrecision(18, 2);
        builder.Property(e => e.GosiNumber).HasMaxLength(20);
        builder.Property(e => e.BankIban).HasMaxLength(34);
        builder.Property(e => e.UserId).HasMaxLength(450);

        builder.HasIndex(e => e.EmployeeNumber).IsUnique();
        builder.HasIndex(e => e.NationalId);
        builder.HasIndex(e => e.IqamaNumber);
        builder.HasIndex(e => e.Email);

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.FullNameAr);
        builder.Ignore(e => e.FullNameEn);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
