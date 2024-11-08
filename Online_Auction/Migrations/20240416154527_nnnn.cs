using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Auction.Migrations
{
    public partial class nnnn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SoldToUserId",
                table: "Products",
                type: "varchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoldToUserId",
                table: "Products");
        }
    }
}
