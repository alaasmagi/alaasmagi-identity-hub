using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddClientDefaultRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DefaultRoleId",
                table: "Clients",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_DefaultRoleId",
                table: "Clients",
                column: "DefaultRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_AspNetRoles_DefaultRoleId",
                table: "Clients",
                column: "DefaultRoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_AspNetRoles_DefaultRoleId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_DefaultRoleId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "DefaultRoleId",
                table: "Clients");
        }
    }
}
