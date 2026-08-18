using System.Text.Json;
using System.Text.Json.Serialization;

namespace Collectivites.Api.Models.Converters;

/// <summary>
/// Convertisseur JSON : les dates sans fuseau (Kind=Unspecified) sont interprétées en UTC,
/// comme l'exige Npgsql pour les colonnes PostgreSQL « timestamp with time zone ».
/// </summary>
public class DateTimeUtcJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var valeur = reader.GetDateTime();
        return valeur.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(valeur, DateTimeKind.Utc) : valeur;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value);
}
