using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Collectivites.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "Collectivites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeAdministratif = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Population = table.Column<int>(type: "integer", nullable: false),
                    Contour = table.Column<Geometry>(type: "geometry(MultiPolygon, 4326)", nullable: false),
                    TypeCollectivite = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    CodePostal = table.Column<string>(type: "text", nullable: true),
                    Prefecture = table.Column<string>(type: "text", nullable: true),
                    Siren = table.Column<string>(type: "text", nullable: true),
                    Nature = table.Column<string>(type: "text", nullable: true),
                    ChefLieu = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collectivites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Historiques",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Entite = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Auteur = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historiques", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nom = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Identifiant = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MotDePasseHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Indicateurs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Valeur = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Unite = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateReleve = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CollectiviteId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indicateurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Indicateurs_Collectivites_CollectiviteId",
                        column: x => x.CollectiviteId,
                        principalTable: "Collectivites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjetsDotations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Intitule = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Montant = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Devise = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Statut = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DateDebut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CollectiviteId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjetsDotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjetsDotations_Collectivites_CollectiviteId",
                        column: x => x.CollectiviteId,
                        principalTable: "Collectivites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Signalements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Geometrie = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: false),
                    TypeSignalement = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Categorie = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Doleance_Statut = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Auteur = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NumeroSuivi = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CollectiviteRattacheeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Statut = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ZoneConflit = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: true),
                    CollectiviteAId = table.Column<Guid>(type: "uuid", nullable: true),
                    CollectiviteBId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signalements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Signalements_Collectivites_CollectiviteAId",
                        column: x => x.CollectiviteAId,
                        principalTable: "Collectivites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Signalements_Collectivites_CollectiviteBId",
                        column: x => x.CollectiviteBId,
                        principalTable: "Collectivites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Signalements_Collectivites_CollectiviteRattacheeId",
                        column: x => x.CollectiviteRattacheeId,
                        principalTable: "Collectivites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UtilisateurCollectivites",
                columns: table => new
                {
                    CollectivitesAccesId = table.Column<Guid>(type: "uuid", nullable: false),
                    UtilisateursAccesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilisateurCollectivites", x => new { x.CollectivitesAccesId, x.UtilisateursAccesId });
                    table.ForeignKey(
                        name: "FK_UtilisateurCollectivites_Collectivites_CollectivitesAccesId",
                        column: x => x.CollectivitesAccesId,
                        principalTable: "Collectivites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UtilisateurCollectivites_Utilisateurs_UtilisateursAccesId",
                        column: x => x.UtilisateursAccesId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Collectivites_CodeAdministratif",
                table: "Collectivites",
                column: "CodeAdministratif",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Historiques_Entite_EntiteId",
                table: "Historiques",
                columns: new[] { "Entite", "EntiteId" });

            migrationBuilder.CreateIndex(
                name: "IX_Indicateurs_CollectiviteId",
                table: "Indicateurs",
                column: "CollectiviteId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjetsDotations_CollectiviteId",
                table: "ProjetsDotations",
                column: "CollectiviteId");

            migrationBuilder.CreateIndex(
                name: "IX_Signalements_CollectiviteAId_CollectiviteBId",
                table: "Signalements",
                columns: new[] { "CollectiviteAId", "CollectiviteBId" });

            migrationBuilder.CreateIndex(
                name: "IX_Signalements_CollectiviteBId",
                table: "Signalements",
                column: "CollectiviteBId");

            migrationBuilder.CreateIndex(
                name: "IX_Signalements_CollectiviteRattacheeId",
                table: "Signalements",
                column: "CollectiviteRattacheeId");

            migrationBuilder.CreateIndex(
                name: "IX_Signalements_NumeroSuivi",
                table: "Signalements",
                column: "NumeroSuivi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UtilisateurCollectivites_UtilisateursAccesId",
                table: "UtilisateurCollectivites",
                column: "UtilisateursAccesId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Identifiant",
                table: "Utilisateurs",
                column: "Identifiant",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Historiques");

            migrationBuilder.DropTable(
                name: "Indicateurs");

            migrationBuilder.DropTable(
                name: "ProjetsDotations");

            migrationBuilder.DropTable(
                name: "Signalements");

            migrationBuilder.DropTable(
                name: "UtilisateurCollectivites");

            migrationBuilder.DropTable(
                name: "Collectivites");

            migrationBuilder.DropTable(
                name: "Utilisateurs");
        }
    }
}
