using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Collectivites.Api.Data;
using Collectivites.Api.Models.Dtos;
using Collectivites.Api.Models.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Collectivites.Api.Services;

/// <summary>
/// Implémentation de l'authentification (UC-01) :
/// vérification du mot de passe (bcrypt) puis émission d'un jeton JWT.
/// </summary>
public class AuthService(AppDbContext db, IOptions<JwtOptions> options) : IAuthService
{
    public async Task<LoginResponse> AuthentifierAsync(LoginRequest demande, CancellationToken ct = default)
    {
        var utilisateur = await db.Utilisateurs
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Identifiant == demande.Identifiant, ct);

        // Même réponse pour identifiant inconnu ou mot de passe erroné (pas d'énumération de comptes)
        if (utilisateur is null || !BCrypt.Net.BCrypt.Verify(demande.MotDePasse, utilisateur.MotDePasseHash))
        {
            throw new UnauthorizedAccessException("Identifiant ou mot de passe invalide.");
        }

        // Compte désactivé : accès refusé (UC-02)
        if (!utilisateur.Actif)
        {
            throw new UnauthorizedAccessException("Ce compte a été désactivé.");
        }

        var jwt = options.Value;
        var revendications = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, utilisateur.Id.ToString()),
            // Type de claim dédié : "unique_name" serait mappé sur ClaimTypes.Name et écraserait le nom complet
            new Claim("identifiant", utilisateur.Identifiant),
            new Claim(ClaimTypes.Name, utilisateur.Nom),
            new Claim(ClaimTypes.Role, utilisateur.Role.ToString())
        };

        var jeton = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: revendications,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.Add(jwt.Expiration),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                SecurityAlgorithms.HmacSha256));

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(jeton),
            Expiration = jeton.ValidTo,
            UtilisateurId = utilisateur.Id,
            Nom = utilisateur.Nom,
            Identifiant = utilisateur.Identifiant,
            Role = utilisateur.Role.ToString()
        };
    }

    public async Task<ReinitialiserMotDePasseResponse> DemanderReinitialisationAsync(
        DemanderReinitialisationRequest demande, CancellationToken ct = default)
    {
        var utilisateur = await db.Utilisateurs
            .FirstOrDefaultAsync(u => u.Identifiant == demande.Identifiant, ct);

        // Toujours retourner un message identique pour éviter l'énumération de comptes
        if (utilisateur is null)
        {
            throw new InvalidOperationException(
                "Si cet identifiant existe, un token de réinitialisation a été généré.");
        }

        // Générer un token aléatoire (32 octets → 44 caractères en base64 URL-safe)
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');

        utilisateur.TokenReinitialisation = token;
        utilisateur.TokenExpiration = DateTime.UtcNow.AddMinutes(30); // 30 min
        await db.SaveChangesAsync(ct);

        return new ReinitialiserMotDePasseResponse
        {
            Message = "Si cet identifiant existe, un token de réinitialisation a été généré.",
            // En prod : envoyer le token par email. En dev : on le renvoie dans la réponse.
            Token = token
        };
    }

    public async Task ReinitialiserMotDePasseAsync(
        ReinitialiserMotDePasseRequest demande, CancellationToken ct = default)
    {
        var utilisateur = await db.Utilisateurs
            .FirstOrDefaultAsync(u => u.Identifiant == demande.Identifiant, ct);

        // Même réponse pour identifiant inconnu ou token invalide
        if (utilisateur is null
            || utilisateur.TokenReinitialisation != demande.Token
            || utilisateur.TokenExpiration is null
            || utilisateur.TokenExpiration < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Token invalide ou expiré.");
        }

        // Appliquer le nouveau mot de passe et révoquer le token
        utilisateur.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(demande.NouveauMotDePasse);
        utilisateur.TokenReinitialisation = null;
        utilisateur.TokenExpiration = null;

        await db.SaveChangesAsync(ct);
    }

    public async Task<LoginResponse> ModifierProfilAsync(Guid utilisateurId, ModifierProfilRequest demande, CancellationToken ct = default)
    {
        var utilisateur = await db.Utilisateurs.FirstOrDefaultAsync(u => u.Id == utilisateurId, ct);
        if (utilisateur is null)
        {
            throw new InvalidOperationException("Utilisateur introuvable.");
        }

        // Mettre à jour le nom
        if (!string.IsNullOrWhiteSpace(demande.Nom))
        {
            utilisateur.Nom = demande.Nom;
        }

        // Mettre à jour le mot de passe si fourni
        if (!string.IsNullOrWhiteSpace(demande.MotDePasse))
        {
            if (demande.MotDePasse != demande.ConfirmationMotDePasse)
            {
                throw new InvalidOperationException("Les mots de passe ne correspondent pas.");
            }

            utilisateur.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(demande.MotDePasse);
        }

        await db.SaveChangesAsync(ct);

        // Retourner les infos mises à jour
        return new LoginResponse
        {
            Token = string.Empty, // Pas de nouveau token, le client garde le sien
            Expiration = DateTime.MinValue,
            UtilisateurId = utilisateur.Id,
            Nom = utilisateur.Nom,
            Identifiant = utilisateur.Identifiant,
            Role = utilisateur.Role.ToString()
        };
    }
}
