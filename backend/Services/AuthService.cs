using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
}
