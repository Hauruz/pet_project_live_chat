using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChatRoomCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ChatRooms",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByid",
                table: "ChatRooms",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_CreatedByid",
                table: "ChatRooms",
                column: "CreatedByid");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_Users_CreatedByid",
                table: "ChatRooms",
                column: "CreatedByid",
                principalTable: "Users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_Users_CreatedByid",
                table: "ChatRooms");

            migrationBuilder.DropIndex(
                name: "IX_ChatRooms_CreatedByid",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "CreatedByid",
                table: "ChatRooms");
        }
    }
}
