using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Collectivites.Api.Migrations
{
    /// <inheritdoc />
    public partial class AjouterActifUtilisateur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Actif",
                table: "Utilisateurs",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Actif",
                table: "Utilisateurs");
        }
    }
}
