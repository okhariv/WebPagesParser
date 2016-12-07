using System;
using System.Linq;
using System.Threading;
using System.Windows;

namespace WebPagesParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConcurrentQueueObservable processedPagesData;
        private CancellationTokenSource cancellationTokenSource;
        private PoolManager poolManager;

        public MainWindow()
        {
            InitializeComponent();
            processedPagesData = new ConcurrentQueueObservable();
            DataContext = new ProcessedPagesViewModel { ProcessedPages = processedPagesData };
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            processedPagesData.Clear();

            var wordToFind = WordToFindValue.Text;
            if (string.IsNullOrEmpty(wordToFind))
            {
                MessageBox.Show("Please, enter non-empty text");
                return;
            }
            var link = StartLinkValue.Text;
            if (string.IsNullOrEmpty(link) || !LinkHelper.IsLocalLinkValid(link) && !LinkHelper.IsLinkValid(link))
            {
                MessageBox.Show("Please, enter non-empty link in a correct format, using http(https) protocol");
                return;
            }
            int maxThreadsCount;
            var processorsCount = Environment.ProcessorCount;
            if (!int.TryParse(MaxThreadsCountValue.Text, out maxThreadsCount) || maxThreadsCount < processorsCount)
            {
                MessageBox.Show($"Please, enter at least {processorsCount} max threads count");
                return;
            }
            int maxPagesToProcessCount;
            if (!int.TryParse(MaxPagesCountValue.Text, out maxPagesToProcessCount) || maxPagesToProcessCount <= 0)
            {
                MessageBox.Show("Please, enter positive max pages to process count");
                return;
            }

            Start.IsEnabled = false;
            Stop.IsEnabled = true;
            StartWebPagesParse(link, wordToFind, maxPagesToProcessCount, maxThreadsCount);
        }

        private void StartWebPagesParse(string link, string wordToFind, int maxPagesToProcessCount, int maxThreadsCount)
        {
            cancellationTokenSource = new CancellationTokenSource();
            var resetEvents = new ManualResetEvent[] { new ManualResetEvent(false) };
            var vm = DataContext as ProcessedPagesViewModel;
            poolManager = new PoolManager(wordToFind, maxThreadsCount, maxPagesToProcessCount,
                                          cancellationTokenSource, resetEvents, processedPagesData);
            poolManager.MaxPageNumberEncounteredCallback += () =>
            {
                Start.IsEnabled = true;
                Stop.IsEnabled = false;
                cancellationTokenSource.Cancel();
                MessageBox.Show("The specified number of pages has  been processed");
            };
            var info = new PageContext(link, cancellationTokenSource.Token, resetEvents.First());

            poolManager.ProcessWork(info);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Start.IsEnabled = true;
            Stop.IsEnabled = false;
            cancellationTokenSource.Cancel();
            MessageBox.Show("Scanning has been stopped");
        }
    }
}