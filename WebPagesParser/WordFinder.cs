namespace WebPagesParser
{
    /// <summary>
    /// Represents helper class for text search
    /// </summary>
    public static class TextFinder
    {
        public static bool IsWordPresent(string html, string word)
        {
            return html.Contains(word);
        }
    }
}