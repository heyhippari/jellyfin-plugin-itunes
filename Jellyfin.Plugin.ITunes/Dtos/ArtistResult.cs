using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.ITunes.Dtos
{
    public partial class ITunesArtistDto
    {
        [JsonPropertyName("resultCount")]
        public long ResultCount { get; set; }

        [JsonPropertyName("results")]
        public ArtistResult[] Results { get; set; }
    }

    public partial class ArtistResult
    {
        [JsonPropertyName("wrapperType")]
        public string WrapperType { get; set; }

        [JsonPropertyName("artistType")]
        public string ArtistType { get; set; }

        [JsonPropertyName("artistName")]
        public string ArtistName { get; set; }

        [JsonPropertyName("artistLinkUrl")]
        public string ArtistLinkUrl { get; set; }

        [JsonPropertyName("artistId")]
        public long ArtistId { get; set; }

        [JsonPropertyName("primaryGenreName")]
        public string PrimaryGenreName { get; set; }

        [JsonPropertyName("primaryGenreId")]
        public long? PrimaryGenreId { get; set; }

        [JsonPropertyName("amgArtistId")]
        public long? AmgArtistId { get; set; }
    }
}
