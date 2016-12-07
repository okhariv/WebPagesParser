using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace WebPagesParser
{
    public enum Status { Downloading, ContainsWord, NotContainsWord, DownloadError }

    /// <summary>
    /// Represents manager class that provides web pages processing in different threads
    /// </summary>
    public class PoolManager
    {
        private readonly string wordToFind;
        private readonly int maxPagesToProcessCount;
        private CancellationTokenSource cancelTokenSource;
        private ConcurrentBag<ManualResetEvent[]> ManualEventsContainer;
        Dispatcher dispatcher;
        private bool isMessageShown;
        public event Action MaxPageNumberEncounteredCallback;
        static object locker = new object();

        /// <summary>
        /// Gets or serts all pages info data that have been processed
        /// </summary>
        public ConcurrentQueueObservable ProcessedPages { get; private set; }

        public PoolManager(string wordToFind, int maxThreadsCount, int maxPagesToProcessCount,
                           CancellationTokenSource cancelTokenSource, ManualResetEvent[] manualEvents,
                           ConcurrentQueueObservable processedPages)
        {
            ProcessedPages = processedPages;
            int workerThreads, completionPortThreads;
            ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
            ThreadPool.SetMaxThreads(maxThreadsCount, completionPortThreads);
            this.wordToFind = wordToFind;
            this.maxPagesToProcessCount = maxPagesToProcessCount;
            this.cancelTokenSource = cancelTokenSource;
            ManualEventsContainer = new ConcurrentBag<ManualResetEvent[]>();
            ManualEventsContainer.Add(manualEvents);
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        /// <summary>
        /// Provides page processing
        /// </summary>
        public void ProcessWork(object info)
        {
            PageContext pageInfo = info as PageContext;
            var link = pageInfo.Link;
            var token = pageInfo.Token;
            var manualEvent = pageInfo.ResetEvent;
            if (token.IsCancellationRequested)
            {
                return;
            }
            if (!String.IsNullOrEmpty(link))
            {
                try
                {
                    var status = Status.Downloading;
                    SavePageStatusInfo(link, status);

                    // 1. get html
                    var html = LinkHelper.GetHtmlByLink(link);
                    // 2. find word
                    var isWordPresent = TextFinder.IsWordPresent(html, wordToFind);
                    status = isWordPresent ? Status.ContainsWord : Status.NotContainsWord;
                    SavePageStatusInfo(link, status);
                    manualEvent.Set();
                    WaitForAllOnTheLevel();

                    if (ProcessedPages.Count <= maxPagesToProcessCount)
                    {
                        // 3. scan html for contained links
                        var linksOnThePageToEnqueue = LinkHelper.ExtractLinks(html)
                                                                .Where(lnk => !ProcessedPages.ContainsKey(lnk))
                                                                .Distinct()
                                                                .ToList();
                        // 4. for every link add new task to the pool
                        if (linksOnThePageToEnqueue.Count != 0)
                        {
                            AddTasksToThePool(linksOnThePageToEnqueue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    var status = Status.DownloadError;
                    SavePageStatusInfo(link, status, ex.Message);
                    manualEvent.Set();
                }
            }
        }

        private void WaitForAllOnTheLevel()
        {
            foreach (var el in ManualEventsContainer)
            {
                ManualResetEvent[] events;
                WaitHandle.WaitAll(el);
                ManualEventsContainer.TryTake(out events);
            }
        }

        private void AddTaskToThePool(PageContext info)
        {
            ThreadPool.QueueUserWorkItem(ProcessWork, info);
        }

        public void AddTasksToThePool(List<string> links)
        {
            var count = maxPagesToProcessCount - ProcessedPages.Count;
            if (count == 0)
            {
                StopProcessing();
            }

            if (links.Count > count)
            {
                links = links.Take(count).ToList();
            }

            var linksCount = links.Count;
            var manualEvents = new ManualResetEvent[linksCount];
            for (int i = 0; i < linksCount; i++)
            {
                manualEvents[i] = new ManualResetEvent(false);
            }

            ManualEventsContainer.Add(manualEvents);
            for (int i = 0; i < linksCount; i++)
            {
                PageContext info = new PageContext(links[i], cancelTokenSource.Token, manualEvents[i]);
                AddTaskToThePool(info);
            }
        }

        public void SavePageStatusInfo(string link, Status status, string exceptionMessage = "")
        {
            if (!ProcessedPages.ContainsKey(link))
            {
                ProcessedPages.Add(link, status, exceptionMessage, (l, s) => status);
            }
            else
            {
                ProcessedPages.Update(link, status, exceptionMessage, (l, s) => status);
            }
        }

        private void StopProcessing()
        {
            lock (locker)
            {
                if (!isMessageShown)
                {
                    isMessageShown = true;
                    if (dispatcher.CheckAccess())
                    {
                        MaxPageNumberEncounteredCallback();
                    }
                    else
                    {
                        dispatcher.Invoke(
                            new Action(() =>
                            {
                                MaxPageNumberEncounteredCallback();
                            }));
                    }
                }
            }
        }
    }
}