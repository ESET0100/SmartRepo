using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeter.Migrations
{
    /// <inheritdoc />
    public partial class AddConsumerPhotoUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Consumer",
                type: "varchar(300)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Consumer");
        }
    }
}
