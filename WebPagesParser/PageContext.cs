using System.Threading;

namespace WebPagesParser
{
    public class PageContext
    {
        public string Link { get; set; }
        public CancellationToken Token { get; set; }
        public ManualResetEvent ResetEvent { get; set; }

        public PageContext(string link, CancellationToken token, ManualResetEvent resetEvent)
        {
            Link = link;
            Token = token;
            ResetEvent = resetEvent;
        }
    }
}