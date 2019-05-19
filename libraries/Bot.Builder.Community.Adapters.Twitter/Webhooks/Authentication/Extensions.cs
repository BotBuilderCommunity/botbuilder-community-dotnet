using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Authentication
{
    internal static class DictionaryExt
    {
        public static Dictionary<string, string> GetParams(this Uri uri)
        {
            try
            {
                var matches = Regex.Matches(uri.AbsoluteUri, @"[\?&](([^&=]+)=([^&=#]*))", RegexOptions.Compiled);
                var keyValues = new Dictionary<string, string>(matches.Count);
                foreach (Match m in matches)
                {
                    keyValues.Add(Uri.UnescapeDataString(m.Groups[2].Value), Uri.UnescapeDataString(m.Groups[3].Value));
                }

                return keyValues;
            }
            catch (Exception)
            {
                return new Dictionary<string, string>();
            }
        }
    }
}
