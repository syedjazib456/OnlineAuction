using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Auction.Migrations
{
    public partial class nn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "number",
                table: "AspNetUsers",
                type: "varchar(15)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "number",
                table: "AspNetUsers");
        }
    }
}
