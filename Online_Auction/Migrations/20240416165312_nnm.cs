using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Auction.Migrations
{
    public partial class nnm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "Notifications",
                type: "varchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Link",
                table: "Notifications");
        }
    }
}
