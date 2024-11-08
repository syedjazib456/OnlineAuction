using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Auction.Migrations
{
    public partial class ne : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductID",
                table: "Ratings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductID",
                table: "Ratings");
        }
    }
}
