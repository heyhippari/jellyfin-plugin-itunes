using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.ITunes.Dtos
{
    public partial class ITunesAlbumDto
    {
        [JsonPropertyName("resultCount")]
        public long ResultCount { get; set; }

        [JsonPropertyName("results")]
        public Result[] Results { get; set; }
    }

    public partial class Result
    {
        [JsonPropertyName("wrapperType")]
        public string WrapperType { get; set; }

        [JsonPropertyName("collectionType")]
        public string CollectionType { get; set; }

        [JsonPropertyName("artistId")]
        public long ArtistId { get; set; }

        [JsonPropertyName("collectionId")]
        public long CollectionId { get; set; }

        [JsonPropertyName("artistName")]
        public string ArtistName { get; set; }

        [JsonPropertyName("collectionName")]
        public string CollectionName { get; set; }

        [JsonPropertyName("collectionCensoredName")]
        public string CollectionCensoredName { get; set; }

        [JsonPropertyName("artistViewUrl")]
        public string ArtistViewUrl { get; set; }

        [JsonPropertyName("collectionViewUrl")]
        public string CollectionViewUrl { get; set; }

        [JsonPropertyName("artworkUrl60")]
        public string ArtworkUrl60 { get; set; }

        [JsonPropertyName("artworkUrl100")]
        public string ArtworkUrl100 { get; set; }

        [JsonPropertyName("collectionPrice")]
        public double CollectionPrice { get; set; }

        [JsonPropertyName("collectionExplicitness")]
        public string CollectionExplicitness { get; set; }

        [JsonPropertyName("trackCount")]
        public long TrackCount { get; set; }

        [JsonPropertyName("copyright")]
        public string Copyright { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("releaseDate")]
        public string ReleaseDate { get; set; }

        [JsonPropertyName("primaryGenreName")]
        public string PrimaryGenreName { get; set; }

        [JsonPropertyName("contentAdvisoryRating")]
        public string ContentAdvisoryRating { get; set; }

        [JsonPropertyName("amgArtistId")]
        public long? AmgArtistId { get; set; }
    }
}
