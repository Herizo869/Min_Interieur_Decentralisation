using Collectivites.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Collectivites.Api.Data;

/// <summary>
/// Contexte EF Core de la plateforme (PostgreSQL + PostGIS).
/// Deux hiérarchies par héritage (TPH) : Collectivité et Signalement.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Hiérarchie Collectivité (TPH) : Commune / Département / Région / EPCI
    public DbSet<Collectivite> Collectivites => Set<Collectivite>();
    public DbSet<Commune> Communes => Set<Commune>();
    public DbSet<Departement> Departements => Set<Departement>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<Epci> Epcis => Set<Epci>();

    // Hiérarchie Signalement (TPH) : Litige / Doléance
    public DbSet<Signalement> Signalements => Set<Signalement>();
    public DbSet<Litige> Litiges => Set<Litige>();
    public DbSet<Doleance> Doleances => Set<Doleance>();

    public DbSet<ProjetDotation> ProjetsDotations => Set<ProjetDotation>();
    public DbSet<Indicateur> Indicateurs => Set<Indicateur>();
    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();
    public DbSet<Historique> Historiques => Set<Historique>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // L'extension PostGIS doit exister sur le serveur (Docker ou installation locale)
        modelBuilder.HasPostgresExtension("postgis");

        // ---- Hiérarchie Collectivité (TPH) ----
        modelBuilder.Entity<Collectivite>(entity =>
        {
            entity.UseTphMappingStrategy()
                  .HasDiscriminator<string>("TypeCollectivite")
                  .HasValue<Commune>("Commune")
                  .HasValue<Departement>("Departement")
                  .HasValue<Region>("Region")
                  .HasValue<Epci>("Epci");

            entity.HasKey(c => c.Id);
            entity.Property(c => c.CodeAdministratif).HasMaxLength(10).IsRequired();
            entity.Property(c => c.Nom).HasMaxLength(200).IsRequired();

            // Colonne géométrique PostGIS (SRID 4326, WGS84)
            entity.Property(c => c.Contour).HasColumnType("geometry(MultiPolygon, 4326)");

            entity.HasIndex(c => c.CodeAdministratif).IsUnique();
        });

        // ---- Hiérarchie Signalement (TPH) ----
        modelBuilder.Entity<Signalement>(entity =>
        {
            entity.UseTphMappingStrategy()
                  .HasDiscriminator<string>("TypeSignalement")
                  .HasValue<Litige>("Litige")
                  .HasValue<Doleance>("Doleance");

            entity.HasKey(s => s.Id);
            entity.Property(s => s.Description).HasMaxLength(2000);
            entity.Property(s => s.Geometrie).HasColumnType("geometry(Geometry, 4326)");
        });

        // ---- Litige ----
        modelBuilder.Entity<Litige>(entity =>
        {
            entity.Property(l => l.ZoneConflit).HasColumnType("geometry(Geometry, 4326)");

            entity.HasOne(l => l.CollectiviteA)
                  .WithMany()
                  .HasForeignKey(l => l.CollectiviteAId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.CollectiviteB)
                  .WithMany()
                  .HasForeignKey(l => l.CollectiviteBId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(l => new { l.CollectiviteAId, l.CollectiviteBId });
        });

        // ---- Doléance ----
        modelBuilder.Entity<Doleance>(entity =>
        {
            entity.Property(d => d.NumeroSuivi).HasMaxLength(20).IsRequired();
            entity.Property(d => d.Auteur).HasMaxLength(100);

            entity.HasOne(d => d.CollectiviteRattachee)
                  .WithMany()
                  .HasForeignKey(d => d.CollectiviteRattacheeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(d => d.NumeroSuivi).IsUnique();
            entity.HasIndex(d => d.CollectiviteRattacheeId);
        });

        // ---- Projet / Dotation ----
        modelBuilder.Entity<ProjetDotation>(entity =>
        {
            entity.Property(p => p.Intitule).HasMaxLength(300).IsRequired();
            entity.Property(p => p.Devise).HasMaxLength(5).IsRequired();
            entity.Property(p => p.Montant).HasPrecision(18, 2);

            entity.HasOne(p => p.Collectivite)
                  .WithMany(c => c.ProjetsDotations)
                  .HasForeignKey(p => p.CollectiviteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Indicateur ----
        modelBuilder.Entity<Indicateur>(entity =>
        {
            entity.Property(i => i.Type).HasMaxLength(100).IsRequired();
            entity.Property(i => i.Unite).HasMaxLength(30).IsRequired();
            entity.Property(i => i.Source).HasMaxLength(100).IsRequired();
            entity.Property(i => i.Valeur).HasPrecision(18, 4);

            entity.HasOne(i => i.Collectivite)
                  .WithMany(c => c.Indicateurs)
                  .HasForeignKey(i => i.CollectiviteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Utilisateur ----
        modelBuilder.Entity<Utilisateur>(entity =>
        {
            entity.Property(u => u.Nom).HasMaxLength(150).IsRequired();
            entity.Property(u => u.Identifiant).HasMaxLength(100).IsRequired();
            entity.Property(u => u.MotDePasseHash).HasMaxLength(200).IsRequired();
            entity.HasIndex(u => u.Identifiant).IsUnique();

            // Périmètre d'accès géographique (association *-* avec Collectivité)
            entity.HasMany(u => u.CollectivitesAcces)
                  .WithMany(c => c.UtilisateursAcces)
                  .UsingEntity(j => j.ToTable("UtilisateurCollectivites"));
        });

        // ---- Historique (audit) ----
        modelBuilder.Entity<Historique>(entity =>
        {
            entity.Property(h => h.Entite).HasMaxLength(50).IsRequired();
            entity.Property(h => h.Action).HasMaxLength(100).IsRequired();
            entity.Property(h => h.Auteur).HasMaxLength(150).IsRequired();

            entity.HasIndex(h => new { h.Entite, h.EntiteId });
        });

        // ---- Énumérations stockées en texte ----
        modelBuilder.Entity<Utilisateur>().Property(u => u.Role).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<ProjetDotation>().Property(p => p.Statut).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<Litige>().Property(l => l.Statut).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<Doleance>().Property(d => d.Statut).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<Doleance>().Property(d => d.Categorie).HasConversion<string>().HasMaxLength(30);
    }
}
