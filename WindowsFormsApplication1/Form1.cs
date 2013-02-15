using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using GitUI;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public Form1()
        {
            InitializeComponent();

            Button button = new Button { Text = "Test" };
            button.Click += (o, e) => Test();
            Controls.Add(button);

            Test();
        }

        void Test()
        {
            Debug.WriteLine("Thread: {0}", Thread.CurrentThread.ManagedThreadId);
            var notificationFeed = new NotificationFeed();
            statusStrip.Items.Insert(0, notificationFeed);

            var batch = Notification.BatchStart(StatusSeverity.Info, "Start");
            var next1 = batch.BatchNext(StatusSeverity.Info, "Next1");
            var next2 = next1.BatchNext(StatusSeverity.Info, "Next2");
            var done = batch.BatchLast(StatusSeverity.Success, "Yay");

            var mockStatusUPdates = new Notification[]
            {
                new Notification(StatusSeverity.Fail, "1. fail"), 
                new Notification(StatusSeverity.Fail, "2. fail2"), 
                batch,
                next1,
                new Notification(StatusSeverity.Info, "3. info"), 
                new Notification(StatusSeverity.Success, "4. success"), 
                next2,
                new Notification(StatusSeverity.Warn, "5. warn"), 
                new Notification(StatusSeverity.Success, "6. really long text with a lot of text alsdjfksjadfl;jkasdflkjs;dfjslkjfdskljdfklsj;dfljslkdfjskldfjlsjdfkljsdfjslkdjfkl"), 
                done,
            };

            var statusFeedDelay = Observable.Create<Notification>(o =>
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

            mockStatusUPdates
                .ToObservable()
                .ObserveOn(this)
                .DelaySubscription(TimeSpan.FromSeconds(3))
                .Subscribe(notificationFeed.Notify);
        }
    }
}
