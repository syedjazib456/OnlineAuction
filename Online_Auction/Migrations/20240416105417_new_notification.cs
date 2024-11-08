using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Auction.Migrations
{
    public partial class new_notification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationMessage = table.Column<string>(type: "varchar(max)", nullable: false),
                    NotificationTimeStamp = table.Column<DateTime>(type: "datetime", nullable: false),
                    NotificationStatus = table.Column<string>(type: "varchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
