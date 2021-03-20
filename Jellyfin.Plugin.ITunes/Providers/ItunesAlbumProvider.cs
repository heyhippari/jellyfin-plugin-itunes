using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
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
    public class ITunesAlbumImageProvider : IRemoteImageProvider, IHasOrder
    {
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ITunesAlbumImageProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
        /// <param name="logger">Instance of the <see cref="ILogger{CoverArtArchiveImageProvider}"/> interface.</param>
        public ITunesAlbumImageProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <inheritdoc />
        public string Name => "Apple Music";

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
                    // The artwork size can vary quite a bit, but for our uses, 1400x1400 should be plenty.
                    // https://artists.apple.com/support/88-artist-image-guidelines
                    var image1400 = result.ArtworkUrl100.Replace("100x100bb","1400x1400bb");

                    list.Add(
                        new RemoteImageInfo
                        {
                            ProviderName = Name,
                            Url = image1400,
                            Type = ImageType.Primary,
                            ThumbnailUrl = result.ArtworkUrl100,
                            Height = 1400,
                            Width = 1400
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
