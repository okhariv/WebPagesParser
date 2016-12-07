using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace WebPagesParser
{
    /// <summary>
    /// Represents class helper for link processing
    /// </summary>
    public static class LinkHelper
    {
        public static string GetHtmlByLink(string url)
        {
            WebClient client = new WebClient();

            return client.DownloadString(url);
        }

        public static List<string> ExtractLinks(string html)
        {
            var list = new List<string>();
            Regex regex = new Regex("(?:href)=[\"|']?(https?.*?)[\"|'|>]+", RegexOptions.Singleline | RegexOptions.CultureInvariant);
            if (regex.IsMatch(html))
            {
                foreach (Match match in regex.Matches(html))
                {
                    list.Add(match.Groups[1].Value);
                }
            }

            return list;
        }

        public static bool IsLinkValid(string link)
        {
            var regex = new Regex(@"^http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=\(\)]*)?$", RegexOptions.Singleline | RegexOptions.CultureInvariant);

            return regex.IsMatch(link);
        }

        public static bool IsLocalLinkValid(string link)
        {
            var regex = new Regex(@"^http(s)?://(localhost:\d+)[\w-]+(/[\w- /?%&=]*)?$", RegexOptions.Singleline | RegexOptions.CultureInvariant);

            return regex.IsMatch(link);
        }
    }
}