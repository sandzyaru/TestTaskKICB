using Microsoft.EntityFrameworkCore;
using TestTaskKICB.Infrastructure.Models;

namespace TestTaskKICB.Infrastructure.DB;

public class BankDbContext : DbContext
{
    public BankDbContext(
        DbContextOptions<BankDbContext> options
        ) : base(options)
    {
    }

    public DbSet<CustomerRow> Customers => Set<CustomerRow>();
    public DbSet<PhoneRow> Phones => Set<PhoneRow>();
    public DbSet<AccountRow> Accounts => Set<AccountRow>();
    public DbSet<CardRow> Cards => Set<CardRow>();
    public DbSet<TransferRow> Transfers => Set<TransferRow>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<CustomerRow>(entity =>
        {
            entity.ToTable("customers", "customers");
            entity.Property(x => x.Id)
                .HasColumnName("id");
            entity.Property(x => x.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100);
            entity.Property(x => x.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100);
            entity.Property(x => x.Patronymic)
                .HasColumnName("patronymic")
                .HasMaxLength(100);
            entity.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(254);
            entity.Property(x => x.NormalizedEmail)
                .HasColumnName("normalized_email")
                .HasMaxLength(254);
            entity.Property(x => x.DateOfBirth)
                .HasColumnName("date_of_birth");
            entity.HasIndex(x => x.NormalizedEmail)
                .IsUnique()
                .HasDatabaseName("ux_customers_normalized_email");
        });

        model.Entity<PhoneRow>(entity =>
        {
            entity.ToTable("phones", "customers", table =>
                table.HasCheckConstraint("ck_phones_type", "type IN ('Mobile', 'Home', 'Work')"));
            entity.Property(x => x.Id)
                .HasColumnName("id");
            entity.Property(x => x.CustomerId)
                .HasColumnName("customer_id");
            entity.Property(x => x.Number)
                .HasColumnName("number")
                .HasMaxLength(16);
            entity.Property(x => x.Type)
                .HasColumnName("type")
                .HasConversion<string>()
                .HasMaxLength(6);
            entity.HasIndex(x => new { x.CustomerId, x.Number })
                .IsUnique()
                .HasDatabaseName("ux_phones_customer_number");
            entity.HasOne<CustomerRow>()
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_phones_customers");
        });

        model.Entity<AccountRow>(entity =>
        {
            entity.ToTable("accounts", "accounts", table =>
            {
                table.HasCheckConstraint(
                    "ck_accounts_currency",
                    "currency IN ('KGS', 'USD', 'EUR')");
                table.HasCheckConstraint(
                    "ck_accounts_balance",
                    "balance >= 0 AND balance <> 'NaN'::numeric");
                table.HasCheckConstraint(
                    "ck_accounts_closed_balance",
                    "closed_at IS NULL OR balance = 0");
            });
            entity.Property(
                x => x.Id).HasColumnName("id");
            entity.Property(
                x => x.CustomerId).HasColumnName("customer_id");
            entity.Property(
                x => x.Number)
                .HasColumnName("number").HasMaxLength(34);
            entity.Property(
                x => x.Currency).HasColumnName("currency")
                .HasConversion<string>().HasMaxLength(3);
            entity.Property(
                x => x.Balance).HasColumnName("balance")
                .HasPrecision(19, 2);
            entity.Property(
                x => x.ClosedAt).HasColumnName("closed_at");
            entity.HasIndex
            (x => x.Number).IsUnique()
            .HasDatabaseName("ux_accounts_number");
            entity.HasOne<CustomerRow>()
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_accounts_customers");
        });

        model.Entity<CardRow>(entity =>
        {
            entity.ToTable("cards", "accounts", table =>
                table
                .HasCheckConstraint(
                    "ck_cards_type",
                    "type IN ('Debit', 'Credit')"
                ));
            entity.Property(
                x => x.Id)
                .HasColumnName("id");
            entity.Property(
                x => x.AccountId)
                .HasColumnName("account_id");
            entity.Property(
                x => x.Number)
                .HasColumnName("number").HasMaxLength(19);
            entity.Property(
                x => x.ExpirationDate)
                .HasColumnName("expiration_date");
            entity.Property(
                x => x.Type)
                .HasColumnName("type")
                .HasConversion<string>()
                .HasMaxLength(6);
            entity.Property(
                x => x.IsBlocked)
                .HasColumnName("is_blocked");
            entity.HasIndex(
                x => x.Number)
                .IsUnique()
                .HasDatabaseName("ux_cards_number");
            entity.HasOne<AccountRow>()
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_cards_accounts");
        });

        model.Entity<TransferRow>(entity =>
        {
            entity.ToTable("transfers", "accounts", table =>
            {
                table.HasCheckConstraint(
                    "ck_transfers_amount",
                    "amount > 0 AND amount <> 'NaN'::numeric");
                table.HasCheckConstraint(
                    "ck_transfers_distinct_accounts",
                    "source_account_id <> destination_account_id");
                table.HasCheckConstraint(
                    "ck_transfers_currency",
                    "currency IN ('KGS', 'USD', 'EUR')");
            });
            entity.Property(
                x => x.Id)
                .HasColumnName("id");
            entity.Property(
                x => x.SourceAccountId)
                .HasColumnName("source_account_id");
            entity.Property(
                x => x.DestinationAccountId)
                .HasColumnName("destination_account_id");
            entity.Property(
                x => x.Amount)
                .HasColumnName("amount")
                .HasPrecision(19, 2);
            entity.Property(
                x => x.Currency)
                .HasColumnName("currency")
                .HasConversion<string>()
                .HasMaxLength(3);
            entity.Property(
                x => x.CreatedAt)
                .HasColumnName("created_at");
            entity.HasOne<AccountRow>()
            .WithMany()
            .HasForeignKey(x => x.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<AccountRow>()
            .WithMany()
            .HasForeignKey(x => x.DestinationAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
