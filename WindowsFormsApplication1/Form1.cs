using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using GitUI;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Test();
        }

        void Test()
        {
            Debug.WriteLine("Thread: {0}", Thread.CurrentThread.ManagedThreadId);
            WarningToolStripItem warner = new WarningToolStripItem(0);
            statusStrip.Items.Insert(0, warner);

            var mockStatusUPdates = new StatusFeedItem[]
            {
                new StatusFeedItem(StatusSeverity.Fail, "1. fail"), 
                new StatusFeedItem(StatusSeverity.Fail, "2. fail2"), 
                new StatusFeedItem(StatusSeverity.Info, "3. info"), 
                new StatusFeedItem(StatusSeverity.Success, "4. success"), 
                new StatusFeedItem(StatusSeverity.Warn, "5. warn"), 
                new StatusFeedItem(StatusSeverity.Success, "6. really long text with a lot of text alsdjfksjadfl;jkasdflkjs;dfjslkjfdskljdfklsj;dfljslkdfjskldfjlsjdfkljsdfjslkdjfkl"), 
            };

            var statusFeedDelay = Observable.Create<StatusFeedItem>(o =>
            {
                var els = new EventLoopScheduler();
                return mockStatusUPdates.ToObservable()//repoObjectsTree.StatusFeed
                    .ObserveOn(els)
                    .Do(x => els.Schedule(() => Thread.Sleep(5 * 1000)))
                    .Subscribe(o);
            });

            //statusFeedDelay
            //    .Timestamp()
            //    .Select(status => new { status.Value, Timestamp = status.Timestamp.Second + (double)status.Timestamp.Millisecond / 1000.0 })
            //    .Subscribe(status => Debug.WriteLine("{0}: {1}", status.Timestamp, status.Value));
            //statusFeedDelay
            //    .SubscribeOn(this)
            //    .Subscribe(update => statusToolStripItem.Notify(update));

            statusFeedDelay
                //.ToObservable()
                .ObserveOn(this)
                .DelaySubscription(TimeSpan.FromSeconds(3))
                .Subscribe(s => warner.Notify(s));
        }
    }
}
