namespace Collectivites.Api.Models.Options;

/// <summary>Configuration du jeton JWT (UC-01).</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Clé de signature HMAC-SHA256 (dev uniquement — à remplacer en production).</summary>
    public string Key { get; set; } = "cle-de-dev-collectivites-2026-changez-moi-en-production-0123456789";

    public string Issuer { get; set; } = "Collectivites.Api";

    public string Audience { get; set; } = "Collectivites.Client";

    /// <summary>Durée de validité du jeton.</summary>
    public TimeSpan Expiration { get; set; } = TimeSpan.FromHours(8);
}
