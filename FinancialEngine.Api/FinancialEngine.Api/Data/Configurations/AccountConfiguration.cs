using FinancialEngine.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialEngine.Api.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts", t =>
            t.HasCheckConstraint("ck_accounts_balance_non_negative", "\"Balance\" >= 0"));

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Owner).IsRequired().HasMaxLength(150);
        builder.Property(a => a.Balance).HasPrecision(18, 2);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.Version).IsRowVersion();
        builder.Property(a => a.Balance).HasPrecision(18, 2).UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}