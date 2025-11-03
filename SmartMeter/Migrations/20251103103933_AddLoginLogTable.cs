using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartMeter.Migrations
{
    /// <inheritdoc />
    public partial class AddLoginLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoginLog",
                columns: table => new
                {
                    LogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserType = table.Column<string>(type: "varchar(20)", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    ConsumerId = table.Column<long>(type: "bigint", nullable: true),
                    Identifier = table.Column<string>(type: "varchar(200)", nullable: false),
                    AttemptResult = table.Column<string>(type: "varchar(20)", nullable: false),
                    IpAddress = table.Column<string>(type: "varchar(45)", nullable: true),
                    UserAgent = table.Column<string>(type: "varchar(500)", nullable: true),
                    AttemptTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdditionalInfo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginLog", x => x.LogId);
                    table.CheckConstraint("CHK_LoginLog_AttemptResult", "\"AttemptResult\" IN ('Success','InvalidPassword','UserNotFound','Inactive','Deleted')");
                    table.CheckConstraint("CHK_LoginLog_UserType", "\"UserType\" IN ('User','Consumer')");
                });

            migrationBuilder.CreateIndex(
                name: "IDX_LoginLog_AttemptResult",
                table: "LoginLog",
                column: "AttemptResult");

            migrationBuilder.CreateIndex(
                name: "IDX_LoginLog_AttemptTime",
                table: "LoginLog",
                column: "AttemptTime");

            migrationBuilder.CreateIndex(
                name: "IDX_LoginLog_ConsumerId",
                table: "LoginLog",
                column: "ConsumerId");

            migrationBuilder.CreateIndex(
                name: "IDX_LoginLog_Time_UserType",
                table: "LoginLog",
                columns: new[] { "AttemptTime", "UserType" });

            migrationBuilder.CreateIndex(
                name: "IDX_LoginLog_UserId",
                table: "LoginLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IDX_LoginLog_UserType",
                table: "LoginLog",
                column: "UserType");

            migrationBuilder.CreateIndex(
                name: "IDX_LoginLog_UserType_Identifier",
                table: "LoginLog",
                columns: new[] { "UserType", "Identifier" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginLog");
        }
    }
}
