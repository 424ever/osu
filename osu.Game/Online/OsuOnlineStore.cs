// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;

namespace osu.Game.Online
{
    public class OsuOnlineStore : OnlineStore
    {
        private readonly string apiEndpointUrl;

        public OsuOnlineStore(string apiEndpointUrl)
        {
            this.apiEndpointUrl = apiEndpointUrl;
        }

        protected override string GetLookupUrl(string url)
        {
            // add leading dot to avoid matching hosts named "<anything>ppy.sh"
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) || !uri.Host.EndsWith(@".ppy.sh", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Log($@"Blocking resource lookup from external website: {url}", LoggingTarget.Network, LogLevel.Important);
                return string.Empty;
            }

            return url;
        }
    }
}
