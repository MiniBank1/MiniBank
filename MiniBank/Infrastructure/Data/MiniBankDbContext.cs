using System;
using System.Collections.Generic;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public partial class MiniBankDbContext : DbContext
{
    public MiniBankDbContext()
    {
    }

    public MiniBankDbContext(DbContextOptions<MiniBankDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Card> Cards { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PaymentType> PaymentTypes { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

   

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Accounts__46A222CDB79701E9");

            entity.HasIndex(e => e.Iban, "UQ__Accounts__983466780AABCB2E").IsUnique();

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.AccountType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("account_type");
            entity.Property(e => e.Balance)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("balance");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Iban)
                .HasMaxLength(34)
                .IsUnicode(false)
                .HasColumnName("iban");
            entity.Property(e => e.InterestRate)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("interest_rate");
            entity.Property(e => e.LastInterestDate)
                .HasColumnType("datetime")
                .HasColumnName("last_interest_date");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accounts_Users");
        });

        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.CardId).HasName("PK__Cards__BDF201DD2EA20BF2");

            entity.HasIndex(e => e.CardNumber, "UQ__Cards__1E6E0AF4DD2CB1F1").IsUnique();

            entity.Property(e => e.CardId).HasColumnName("card_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CardNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("card_number");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Cvv)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("cvv");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");

            entity.HasOne(d => d.Account).WithMany(p => p.Cards)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cards_Accounts");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842FB396292E");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Message)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("message");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("FK_Notifications_Transactions");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<PaymentType>(entity =>
        {
            entity.HasKey(e => e.PaymentTypeId).HasName("PK__PaymentT__8C1ABD6F2DA488DE");

            entity.HasIndex(e => e.Name, "UQ__PaymentT__72E12F1B342F79BD").IsUnique();

            entity.Property(e => e.PaymentTypeId).HasColumnName("payment_type_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Transact__85C600AF929BBA23");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.PaymentTypeId).HasColumnName("payment_type_id");
            entity.Property(e => e.ReceiverAccountId).HasColumnName("receiver_account_id");
            entity.Property(e => e.SenderAccountId).HasColumnName("sender_account_id");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("transaction_type");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PaymentTypeId)
                .HasConstraintName("FK_Transactions_PaymentTypes");

            entity.HasOne(d => d.ReceiverAccount).WithMany(p => p.TransactionReceiverAccounts)
                .HasForeignKey(d => d.ReceiverAccountId)
                .HasConstraintName("FK_Transactions_ReceiverAccount");

            entity.HasOne(d => d.SenderAccount).WithMany(p => p.TransactionSenderAccounts)
                .HasForeignKey(d => d.SenderAccountId)
                .HasConstraintName("FK_Transactions_SenderAccount");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F3F7FB61F");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E6164A0A512EA").IsUnique();

            entity.HasIndex(e => e.TcNo, "UQ__Users__E61FE7CA4079E61F").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.TcNo)
                .HasMaxLength(11)
                .IsUnicode(false)
                .HasColumnName("tc_no");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("user_name");
            entity.Property(e => e.UserSurname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("user_surname");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
