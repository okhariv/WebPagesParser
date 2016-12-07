using System;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.Windows.Threading;

namespace WebPagesParser
{
    /// <summary>
    /// Represents ConcurrentQueue class that provides notification about updted values
    /// </summary>
    public class ConcurrentQueueObservable : ConcurrentQueue<PageInfo>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private Dispatcher dispatcher;

        public ConcurrentQueueObservable(Dispatcher dispatcher = null)
        {
            this.dispatcher = dispatcher ?? Dispatcher.CurrentDispatcher;
        }

        public void Add(string link, Status status, string exceptionMessage, Func<string, Status, Status> updateValueFactory)
        {
            Enqueue(new PageInfo { Link = link, Status = status, ExceptionMessage = exceptionMessage });
            NotifyCollectionChanged();
        }

        public void Update(string link, Status status, string exceptionMessage, Func<string, Status, Status> updateValueFactory)
        {
            var pageInfo = this.FirstOrDefault(e => e.Link == link);
            pageInfo.Link = link;
            pageInfo.Status = status;
            pageInfo.ExceptionMessage = exceptionMessage;
            NotifyCollectionChanged();
        }

        public void Clear()
        {
            PageInfo page;
            while (!base.IsEmpty)
            {
                base.TryDequeue(out page);
            }

            NotifyCollectionChanged();
        }

        public bool ContainsKey(string key)
        {
            return this.Any(p => p.Link == key);
        }

        private void NotifyCollectionChanged()
        {
            if (dispatcher.CheckAccess())
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else
            {
                dispatcher.Invoke(CollectionChanged, DispatcherPriority.Send, this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }
}