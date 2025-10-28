using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartMeter.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrgUnit",
                columns: table => new
                {
                    OrgUnitId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "varchar(20)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgUnit", x => x.OrgUnitId);
                    table.CheckConstraint("CHK_OrgUnit_Type", "\"Type\" IN ('Zone','Substation','Feeder','DTR')");
                    table.ForeignKey(
                        name: "FK_OrgUnit_OrgUnit_ParentId",
                        column: x => x.ParentId,
                        principalTable: "OrgUnit",
                        principalColumn: "OrgUnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tariff",
                columns: table => new
                {
                    TariffId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    BaseRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tariff", x => x.TariffId);
                    table.CheckConstraint("CHK_Tariff_BaseRate_Positive", "\"BaseRate\" > 0");
                    table.CheckConstraint("CHK_Tariff_EffectiveDates", "\"EffectiveTo\" IS NULL OR \"EffectiveTo\" > \"EffectiveFrom\"");
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "varchar(100)", nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "bytea", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(150)", nullable: false),
                    Email = table.Column<string>(type: "varchar(200)", nullable: true),
                    Phone = table.Column<string>(type: "varchar(30)", nullable: true),
                    LastLoginUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Consumer",
                columns: table => new
                {
                    ConsumerId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false),
                    Phone = table.Column<string>(type: "varchar(30)", nullable: true),
                    Email = table.Column<string>(type: "varchar(200)", nullable: true),
                    OrgUnitId = table.Column<int>(type: "integer", nullable: false),
                    TariffId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValue: "Active"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "varchar(100)", nullable: false, defaultValue: "system"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(100)", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consumer", x => x.ConsumerId);
                    table.CheckConstraint("CHK_Consumer_Status", "\"Status\" IN ('Active','Inactive')");
                    table.CheckConstraint("CHK_Consumer_UpdatedAfterCreated", "\"UpdatedAt\" IS NULL OR \"UpdatedAt\" >= \"CreatedAt\"");
                    table.ForeignKey(
                        name: "FK_Consumer_OrgUnit_OrgUnitId",
                        column: x => x.OrgUnitId,
                        principalTable: "OrgUnit",
                        principalColumn: "OrgUnitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Consumer_Tariff_TariffId",
                        column: x => x.TariffId,
                        principalTable: "Tariff",
                        principalColumn: "TariffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TariffSlab",
                columns: table => new
                {
                    TariffSlabId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TariffId = table.Column<int>(type: "integer", nullable: false),
                    FromKwh = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    ToKwh = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    RatePerKwh = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TariffSlab", x => x.TariffSlabId);
                    table.CheckConstraint("CHK_TariffSlab_Range", "\"FromKwh\" >= 0 AND \"ToKwh\" > \"FromKwh\"");
                    table.CheckConstraint("CHK_TariffSlab_Rate_Positive", "\"RatePerKwh\" > 0");
                    table.ForeignKey(
                        name: "FK_TariffSlab_Tariff_TariffId",
                        column: x => x.TariffId,
                        principalTable: "Tariff",
                        principalColumn: "TariffId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TodRule",
                columns: table => new
                {
                    TodRuleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TariffId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time(0) without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time(0) without time zone", nullable: false),
                    RatePerKwh = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodRule", x => x.TodRuleId);
                    table.CheckConstraint("CHK_TodRule_Rate_NonNegative", "\"RatePerKwh\" >= 0");
                    table.CheckConstraint("CHK_TodRule_TimeRange", "\"EndTime\" > \"StartTime\"");
                    table.ForeignKey(
                        name: "FK_TodRule_Tariff_TariffId",
                        column: x => x.TariffId,
                        principalTable: "Tariff",
                        principalColumn: "TariffId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    AddressId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HouseNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    Locality = table.Column<string>(type: "varchar(200)", nullable: false),
                    City = table.Column<string>(type: "varchar(100)", nullable: false),
                    State = table.Column<string>(type: "varchar(100)", nullable: false),
                    Pincode = table.Column<string>(type: "varchar(10)", nullable: false),
                    ConsumerId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.AddressId);
                    table.ForeignKey(
                        name: "FK_Address_Consumer_ConsumerId",
                        column: x => x.ConsumerId,
                        principalTable: "Consumer",
                        principalColumn: "ConsumerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Meter",
                columns: table => new
                {
                    MeterSerialNo = table.Column<string>(type: "varchar(50)", nullable: false),
                    IpAddress = table.Column<string>(type: "varchar(45)", nullable: false),
                    ICCID = table.Column<string>(type: "varchar(30)", nullable: false),
                    IMSI = table.Column<string>(type: "varchar(30)", nullable: false),
                    Manufacturer = table.Column<string>(type: "varchar(100)", nullable: false),
                    Firmware = table.Column<string>(type: "varchar(50)", nullable: true),
                    Category = table.Column<string>(type: "varchar(50)", nullable: false),
                    InstallTsUtc = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValue: "Active"),
                    ConsumerId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meter", x => x.MeterSerialNo);
                    table.CheckConstraint("CHK_Meter_Status", "\"Status\" IN ('Active','Inactive','Decommissioned')");
                    table.ForeignKey(
                        name: "FK_Meter_Consumer_ConsumerId",
                        column: x => x.ConsumerId,
                        principalTable: "Consumer",
                        principalColumn: "ConsumerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Billing",
                columns: table => new
                {
                    BillId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConsumerId = table.Column<long>(type: "bigint", nullable: false),
                    MeterId = table.Column<string>(type: "varchar(50)", nullable: false),
                    BillingPeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    BillingPeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalUnitsConsumed = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: false, computedColumnSql: "\"BaseAmount\" + \"TaxAmount\"", stored: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp(3) with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "timestamp(3) with time zone", nullable: true),
                    PaymentStatus = table.Column<string>(type: "varchar(20)", nullable: false, defaultValue: "Unpaid"),
                    DisconnectionDate = table.Column<DateTime>(type: "timestamp(3) with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billing", x => x.BillId);
                    table.CheckConstraint("CHK_BaseAmount_Positive", "\"BaseAmount\" >= 0");
                    table.CheckConstraint("CHK_Billing_PaymentStatus", "\"PaymentStatus\" IN ('Unpaid', 'Paid', 'Overdue', 'Cancelled')");
                    table.CheckConstraint("CHK_Billing_Period", "\"BillingPeriodEnd\" > \"BillingPeriodStart\"");
                    table.CheckConstraint("CHK_DueDate_After_End", "\"DueDate\" >= \"BillingPeriodEnd\"");
                    table.CheckConstraint("CHK_TaxAmount_Positive", "\"TaxAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_Billing_Consumer_ConsumerId",
                        column: x => x.ConsumerId,
                        principalTable: "Consumer",
                        principalColumn: "ConsumerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Billing_Meter_MeterId",
                        column: x => x.MeterId,
                        principalTable: "Meter",
                        principalColumn: "MeterSerialNo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeterReading",
                columns: table => new
                {
                    ReadingId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReadingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EnergyConsumed = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    MeterSerialNo = table.Column<string>(type: "varchar(50)", nullable: false),
                    Current = table.Column<decimal>(type: "numeric(8,3)", nullable: true),
                    Voltage = table.Column<decimal>(type: "numeric(8,3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterReading", x => x.ReadingId);
                    table.CheckConstraint("CHK_MeterReading_EnergyConsumed_Positive", "\"EnergyConsumed\" >= 0");
                    table.ForeignKey(
                        name: "FK_MeterReading_Meter_MeterSerialNo",
                        column: x => x.MeterSerialNo,
                        principalTable: "Meter",
                        principalColumn: "MeterSerialNo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Arrears",
                columns: table => new
                {
                    ArrearId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConsumerId = table.Column<long>(type: "bigint", nullable: false),
                    ArrearType = table.Column<string>(type: "varchar(50)", nullable: false),
                    PaidStatus = table.Column<string>(type: "varchar(20)", nullable: false, defaultValue: "Pending"),
                    BillId = table.Column<long>(type: "bigint", nullable: false),
                    ArrearAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp(3) with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arrears", x => x.ArrearId);
                    table.CheckConstraint("CHK_Arrears_Amount_Positive", "\"ArrearAmount\" >= 0");
                    table.CheckConstraint("CHK_Arrears_ArrearType", "\"ArrearType\" IN ('interest', 'penalty', 'overdue')");
                    table.CheckConstraint("CHK_Arrears_PaidStatus", "\"PaidStatus\" IN ('Pending', 'Paid', 'Partial')");
                    table.ForeignKey(
                        name: "FK_Arrears_Billing_BillId",
                        column: x => x.BillId,
                        principalTable: "Billing",
                        principalColumn: "BillId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Arrears_Consumer_ConsumerId",
                        column: x => x.ConsumerId,
                        principalTable: "Consumer",
                        principalColumn: "ConsumerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_Address_ConsumerId",
                table: "Address",
                column: "ConsumerId");

            migrationBuilder.CreateIndex(
                name: "IDX_Address_Pincode",
                table: "Address",
                column: "Pincode");

            migrationBuilder.CreateIndex(
                name: "IDX_Arrears_BillId",
                table: "Arrears",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IDX_Arrears_ConsumerId",
                table: "Arrears",
                column: "ConsumerId");

            migrationBuilder.CreateIndex(
                name: "IDX_Arrears_PaidStatus",
                table: "Arrears",
                column: "PaidStatus");

            migrationBuilder.CreateIndex(
                name: "IDX_Billing_ConsumerId",
                table: "Billing",
                column: "ConsumerId");

            migrationBuilder.CreateIndex(
                name: "IDX_Billing_MeterId",
                table: "Billing",
                column: "MeterId");

            migrationBuilder.CreateIndex(
                name: "IDX_Billing_PaymentStatus",
                table: "Billing",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IDX_Billing_Period",
                table: "Billing",
                columns: new[] { "BillingPeriodStart", "BillingPeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_Consumer_OrgUnitId",
                table: "Consumer",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumer_TariffId",
                table: "Consumer",
                column: "TariffId");

            migrationBuilder.CreateIndex(
                name: "IX_Meter_ConsumerId",
                table: "Meter",
                column: "ConsumerId");

            migrationBuilder.CreateIndex(
                name: "IX_MeterReading_MeterSerialNo",
                table: "MeterReading",
                column: "MeterSerialNo");

            migrationBuilder.CreateIndex(
                name: "IDX_OrgUnit_Name",
                table: "OrgUnit",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IDX_OrgUnit_Type",
                table: "OrgUnit",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_OrgUnit_ParentId",
                table: "OrgUnit",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_TariffSlab_TariffId",
                table: "TariffSlab",
                column: "TariffId");

            migrationBuilder.CreateIndex(
                name: "IX_TodRule_TariffId",
                table: "TodRule",
                column: "TariffId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "Arrears");

            migrationBuilder.DropTable(
                name: "MeterReading");

            migrationBuilder.DropTable(
                name: "TariffSlab");

            migrationBuilder.DropTable(
                name: "TodRule");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Billing");

            migrationBuilder.DropTable(
                name: "Meter");

            migrationBuilder.DropTable(
                name: "Consumer");

            migrationBuilder.DropTable(
                name: "OrgUnit");

            migrationBuilder.DropTable(
                name: "Tariff");
        }
    }
}
