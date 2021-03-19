using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.ITunes.Dtos;

namespace Jellyfin.Plugin.ITunes.Providers
{
    public class ITunesAlbumProvider : IRemoteImageProvider, IHasOrder
    {
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ITunesAlbumProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
        /// <param name="logger">Instance of the <see cref="ILogger{CoverArtArchiveImageProvider}"/> interface.</param>
        public ITunesAlbumProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <inheritdoc />
        public string Name => "iTunes";

        /// <inheritdoc />
        public int Order => 1; // After embedded provider

        /// <inheritdoc />
        public bool Supports(BaseItem item)
            => item is MusicAlbum;

        /// <inheritdoc />
        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return new List<ImageType>
            {
                ImageType.Primary
            };
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient(NamedClient.Default);
            return await httpClient.GetAsync(new Uri(url), cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            var album = (MusicAlbum)item;
            var list = new List<RemoteImageInfo>();

            if (!string.IsNullOrEmpty(album.Name))
            {
                var searchQuery = album.Name;

                if (album.AlbumArtists.Count > 0) {
                    string[] terms = {
                        album.AlbumArtists[0],
                        album.Name
                    };
                    searchQuery = String.Join(' ', terms);
                }

                var encodedName = Uri.EscapeUriString(searchQuery);

                list.AddRange(await GetImagesInternal($"https://itunes.apple.com/search?term={encodedName}&media=music&entity=album", cancellationToken)
                    .ConfigureAwait(false));
            }

            return list;
        }

        private async Task<IEnumerable<RemoteImageInfo>> GetImagesInternal(string url, CancellationToken cancellationToken)
        {
            List<RemoteImageInfo> list = new List<RemoteImageInfo>();

            var iTunesAlbumDto = await _httpClientFactory
                .CreateClient(NamedClient.Default)
                .GetFromJsonAsync<ITunesAlbumDto>(new Uri(url))
                .ConfigureAwait(false);;

            if (iTunesAlbumDto != null)
            {
                foreach (Result result in iTunesAlbumDto.Results)
                {
                    // The max artwork size is 3000x3000. Some might return less, but we can't predict that.
                    var image1400 = result.ArtworkUrl100.Replace("100x100bb","3000x3000bb");

                    list.Add(
                        new RemoteImageInfo
                        {
                            ProviderName = Name,
                            Url = image1400,
                            Type = ImageType.Primary,
                            ThumbnailUrl = result.ArtworkUrl100,
                            RatingType = RatingType.Score,
                        }
                    );
                }
            }
            else
            {
                return Array.Empty<RemoteImageInfo>();
            }

            return list;
        }
    }
}
