using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Collectivites.Api.Migrations
{
    /// <inheritdoc />
    public partial class AjouterTokenReinitialisation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpiration",
                table: "Utilisateurs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenReinitialisation",
                table: "Utilisateurs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenExpiration",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "TokenReinitialisation",
                table: "Utilisateurs");
        }
    }
}
