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
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using Jellyfin.Plugin.ITunes.Dtos;

namespace Jellyfin.Plugin.ITunes.Providers
{
    public class ITunesArtistImageProvider : IRemoteImageProvider, IHasOrder
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ITunesArtistImageProvider> _logger;

        public ITunesArtistImageProvider(IHttpClientFactory httpClientFactory, ILogger<ITunesArtistImageProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <inheritdoc />
        public string Name => "Apple Music";

        /// <inheritdoc />
        // After fanart
        public int Order => 1;

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
            var artist = (MusicArtist)item;
            var list = new List<RemoteImageInfo>();

            if (!string.IsNullOrEmpty(artist.Name))
            {
                var searchQuery = artist.Name;

                var encodedName = Uri.EscapeUriString(searchQuery);

                list.AddRange(await GetImagesInternal($"https://itunes.apple.com/search?term=${encodedName}&media=music&entity=musicArtist", cancellationToken)
                    .ConfigureAwait(false));
            }

            return list;
        }


        private async Task<IEnumerable<RemoteImageInfo>> GetImagesInternal(string url, CancellationToken cancellationToken)
        {
            List<RemoteImageInfo> list = new List<RemoteImageInfo>();

            var iTunesArtistDto = await _httpClientFactory
                .CreateClient(NamedClient.Default)
                .GetFromJsonAsync<ITunesArtistDto>(new Uri(url))
                .ConfigureAwait(false);;

            if (iTunesArtistDto != null)
            {
                var result = iTunesArtistDto.Results[0];
                _logger.LogInformation("URL: " + result.ArtistLinkUrl);
                HtmlWeb web = new HtmlWeb();
                var doc = web.Load(result.ArtistLinkUrl);
                var navigator = (HtmlAgilityPack.HtmlNodeNavigator)doc.CreateNavigator();

                var metaOgImage = navigator.SelectSingleNode("/html/head/meta[@property='og:image']/@content");

                _logger.LogInformation("Node: " + metaOgImage.NodeType + " | " + metaOgImage.Value);

                // The artwork size can vary quite a bit, but for our uses, 1400x1400 should be plenty.
                // https://artists.apple.com/support/88-artist-image-guidelines
                var image100 = metaOgImage.Value.Replace("1200x630cw","100x100cc");
                _logger.LogInformation("image100: " + image100);
                var image1400 = metaOgImage.Value.Replace("1200x630cw","1400x1400cc");
                _logger.LogInformation("image1400: " + image1400);

                list.Add(
                    new RemoteImageInfo
                    {
                        ProviderName = Name,
                        Url = image1400,
                        Type = ImageType.Primary,
                        ThumbnailUrl = image100
                    }
                );
            }
            else
            {
                return Array.Empty<RemoteImageInfo>();
            }

            return list;
        }

        /// <inheritdoc />
        public bool Supports(BaseItem item)
            => item is MusicArtist;
    }
}
