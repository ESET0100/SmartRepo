using Microsoft.EntityFrameworkCore;
using SmartMeter.Models;

namespace SmartMeter.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<OrgUnit> OrgUnits { get; set; }
        public DbSet<Tariff> Tariffs { get; set; }
        public DbSet<TodRule> TodRules { get; set; }
        public DbSet<TariffSlab> TariffSlabs { get; set; }
        public DbSet<Consumer> Consumers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Meter> Meters { get; set; }
        public DbSet<MeterReading> MeterReadings { get; set; }
        public DbSet<Billing> Billings { get; set; }
        public DbSet<Arrears> Arrears { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // OrgUnit configuration
            modelBuilder.Entity<OrgUnit>(entity =>
            {
                entity.HasKey(e => e.OrgUnitId);

                // Check constraint for Type
                entity.ToTable(t =>
                    t.HasCheckConstraint("CHK_OrgUnit_Type",
                        @"""Type"" IN ('Zone','Substation','Feeder','DTR')"));

                // Self-referencing relationship
                entity.HasOne(e => e.Parent)
                      .WithMany(e => e.Children)
                      .HasForeignKey(e => e.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.Type).HasDatabaseName("IDX_OrgUnit_Type");
                entity.HasIndex(e => e.Name).HasDatabaseName("IDX_OrgUnit_Name");
            });

            // Tariff configuration
            modelBuilder.Entity<Tariff>(entity =>
            {
                entity.HasKey(e => e.TariffId);

                // Check constraints
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CHK_Tariff_EffectiveDates",
                        @"""EffectiveTo"" IS NULL OR ""EffectiveTo"" > ""EffectiveFrom""");
                    t.HasCheckConstraint("CHK_Tariff_BaseRate_Positive",
                        @"""BaseRate"" > 0");
                });
            });

            // TodRule configuration
            modelBuilder.Entity<TodRule>(entity =>
            {
                entity.HasKey(e => e.TodRuleId);

                // Foreign key
                entity.HasOne(e => e.Tariff)
                      .WithMany(e => e.TodRules)
                      .HasForeignKey(e => e.TariffId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Check constraints
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CHK_TodRule_TimeRange",
                        @"""EndTime"" > ""StartTime""");
                    t.HasCheckConstraint("CHK_TodRule_Rate_NonNegative",
                        @"""RatePerKwh"" >= 0");
                });

                entity.Property(e => e.Deleted).HasDefaultValue(false);
            });

            // TariffSlab configuration
            modelBuilder.Entity<TariffSlab>(entity =>
            {
                entity.HasKey(e => e.TariffSlabId);

                // Foreign key
                entity.HasOne(e => e.Tariff)
                      .WithMany(e => e.TariffSlabs)
                      .HasForeignKey(e => e.TariffId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Check constraints
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CHK_TariffSlab_Range",
                        @"""FromKwh"" >= 0 AND ""ToKwh"" > ""FromKwh""");
                    t.HasCheckConstraint("CHK_TariffSlab_Rate_Positive",
                        @"""RatePerKwh"" > 0");
                });

                entity.Property(e => e.Deleted).HasDefaultValue(false);
            });

            // Consumer configuration
            modelBuilder.Entity<Consumer>(entity =>
            {
                entity.HasKey(e => e.ConsumerId);

                // Foreign keys
                entity.HasOne(e => e.OrgUnit)
                      .WithMany(e => e.Consumers)
                      .HasForeignKey(e => e.OrgUnitId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Tariff)
                      .WithMany(e => e.Consumers)
                      .HasForeignKey(e => e.TariffId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Check constraints
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CHK_Consumer_Status",
                        @"""Status"" IN ('Active','Inactive')");
                    t.HasCheckConstraint("CHK_Consumer_UpdatedAfterCreated",
                        @"""UpdatedAt"" IS NULL OR ""UpdatedAt"" >= ""CreatedAt""");
                });

                entity.Property(e => e.Status).HasDefaultValue("Active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.CreatedBy).HasDefaultValue("system");
                entity.Property(e => e.Deleted).HasDefaultValue(false);
            });

            // Address configuration
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(e => e.AddressId);

                // Foreign key
                entity.HasOne(e => e.Consumer)
                      .WithMany(e => e.Addresses)
                      .HasForeignKey(e => e.ConsumerId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.ConsumerId).HasDatabaseName("IDX_Address_ConsumerId");
                entity.HasIndex(e => e.Pincode).HasDatabaseName("IDX_Address_Pincode");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Meter configuration
            modelBuilder.Entity<Meter>(entity =>
            {
                entity.HasKey(e => e.MeterSerialNo);

                // Foreign key
                entity.HasOne(e => e.Consumer)
                      .WithMany(e => e.Meters)
                      .HasForeignKey(e => e.ConsumerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Check constraint
                entity.ToTable(t =>
                    t.HasCheckConstraint("CHK_Meter_Status",
                        @"""Status"" IN ('Active','Inactive','Decommissioned')"));

                entity.Property(e => e.Status).HasDefaultValue("Active");
            });

            // MeterReading configuration
            modelBuilder.Entity<MeterReading>(entity =>
            {
                entity.HasKey(e => e.ReadingId);

                // Foreign key
                entity.HasOne(e => e.Meter)
                      .WithMany(e => e.MeterReadings)
                      .HasForeignKey(e => e.MeterSerialNo)
                      .OnDelete(DeleteBehavior.Cascade);

                // Check constraints
                entity.ToTable(t =>
                    t.HasCheckConstraint("CHK_MeterReading_EnergyConsumed_Positive",
                        @"""EnergyConsumed"" >= 0"));
            });

            // Billing configuration
            modelBuilder.Entity<Billing>(entity =>
            {
                entity.HasKey(e => e.BillId);

                // Foreign keys
                entity.HasOne(e => e.Consumer)
                      .WithMany(e => e.Billings)
                      .HasForeignKey(e => e.ConsumerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Meter)
                      .WithMany(e => e.Billings)
                      .HasForeignKey(e => e.MeterId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Check constraints
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CHK_Billing_Period",
                        @"""BillingPeriodEnd"" > ""BillingPeriodStart""");
                    t.HasCheckConstraint("CHK_DueDate_After_End",
                        @"""DueDate"" >= ""BillingPeriodEnd""");
                    t.HasCheckConstraint("CHK_BaseAmount_Positive",
                        @"""BaseAmount"" >= 0");
                    t.HasCheckConstraint("CHK_TaxAmount_Positive",
                        @"""TaxAmount"" >= 0");
                    t.HasCheckConstraint("CHK_Billing_PaymentStatus",
                        @"""PaymentStatus"" IN ('Unpaid', 'Paid', 'Overdue', 'Cancelled')");
                });

                // Computed column
                entity.Property(e => e.TotalAmount)
                    .HasComputedColumnSql(@"""BaseAmount"" + ""TaxAmount""", stored: true);

                entity.Property(e => e.GeneratedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.PaymentStatus).HasDefaultValue("Unpaid");
                entity.Property(e => e.TaxAmount).HasDefaultValue(0);

                // Indexes
                entity.HasIndex(e => e.ConsumerId).HasDatabaseName("IDX_Billing_ConsumerId");
                entity.HasIndex(e => e.MeterId).HasDatabaseName("IDX_Billing_MeterId");
                entity.HasIndex(e => e.PaymentStatus).HasDatabaseName("IDX_Billing_PaymentStatus");
                entity.HasIndex(e => new { e.BillingPeriodStart, e.BillingPeriodEnd })
                    .HasDatabaseName("IDX_Billing_Period");
            });

            // Arrears configuration
            modelBuilder.Entity<Arrears>(entity =>
            {
                entity.HasKey(e => e.ArrearId);

                // Foreign keys
                entity.HasOne(e => e.Consumer)
                      .WithMany()
                      .HasForeignKey(e => e.ConsumerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Billing)
                      .WithMany(e => e.Arrears)
                      .HasForeignKey(e => e.BillId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Check constraints
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CHK_Arrears_ArrearType",
                        @"""ArrearType"" IN ('interest', 'penalty', 'overdue')");
                    t.HasCheckConstraint("CHK_Arrears_PaidStatus",
                        @"""PaidStatus"" IN ('Pending', 'Paid', 'Partial')");
                    t.HasCheckConstraint("CHK_Arrears_Amount_Positive",
                        @"""ArrearAmount"" >= 0");
                });

                entity.Property(e => e.PaidStatus).HasDefaultValue("Pending");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

                // Indexes
                entity.HasIndex(e => e.ConsumerId).HasDatabaseName("IDX_Arrears_ConsumerId");
                entity.HasIndex(e => e.BillId).HasDatabaseName("IDX_Arrears_BillId");
                entity.HasIndex(e => e.PaidStatus).HasDatabaseName("IDX_Arrears_PaidStatus");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}