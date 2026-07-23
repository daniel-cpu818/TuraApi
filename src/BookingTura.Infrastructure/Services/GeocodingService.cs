using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookingTura.Application.DTOs.Geocoding;
using BookingTura.Application.Interfaces;

namespace BookingTura.Infrastructure.Services;

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;

    public GeocodingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GeocodingResultDto?> GeocodeAsync(
        string address,
        string? commune = null,
        string? hood = null,
        string? piso = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("La direccion es obligatoria.", nameof(address));

        foreach (var query in BuildQueryCandidates(address, commune, hood))
        {
            var result = await SearchAsync(query, cancellationToken);
            if (result != null)
                return result;
        }

        return null;
    }

    private async Task<GeocodingResultDto?> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var requestUri = $"search?format=jsonv2&limit=1&countrycodes=co&q={Uri.EscapeDataString(query)}";

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var results = await JsonSerializer.DeserializeAsync<List<NominatimSearchResult>>(
            contentStream,
            cancellationToken: cancellationToken);

        var firstResult = results?.FirstOrDefault();
        if (firstResult == null)
            return null;

        if (!TryParseCoordinate(firstResult.Lat, out var latitude))
            return null;

        if (!TryParseCoordinate(firstResult.Lon, out var longitude))
            return null;

        return new GeocodingResultDto
        {
            Latitude = latitude,
            Longitude = longitude
        };
    }

    private static IReadOnlyList<string> BuildQueryCandidates(
        string address,
        string? commune,
        string? hood)
    {
        var candidates = new List<string>();

        var cleanAddress = Normalize(address);
        var cleanHood = Normalize(hood);
        var cleanCommune = Normalize(commune);

        AddCandidate(cleanAddress, IsUsefulText(cleanCommune) ? cleanCommune : null, "Colombia");
        AddCandidate(cleanAddress, "Colombia, Valle del Cauca, Buenaventura");
        AddCandidate(cleanAddress, IsUsefulText(cleanHood) ? cleanHood : null, "Colombia");
        AddCandidate(cleanAddress);

        return candidates;

        void AddCandidate(params string?[] parts)
        {
            var query = string.Join(", ",
                parts
                    .Where(part => !string.IsNullOrWhiteSpace(part))
                    .Select(part => part!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(query) && !candidates.Contains(query, StringComparer.OrdinalIgnoreCase))
                candidates.Add(query);
        }
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return string.Join(
            " ",
            value
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static bool IsUsefulText(string value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Any(char.IsLetter);
    }

    private static bool TryParseCoordinate(string? value, out double result)
    {
        return double.TryParse(
            value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out result);
    }

    private sealed class NominatimSearchResult
    {
        [JsonPropertyName("lat")]
        public string? Lat { get; set; }

        [JsonPropertyName("lon")]
        public string? Lon { get; set; }
    }
}
