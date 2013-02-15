using System;
using System.Diagnostics;
using System.Drawing;
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

            NotificationFeed notificationFeed = new NotificationFeed();
            notifier = notificationFeed;
            statusStrip.Items.Insert(0, notificationFeed);
            
            btnDelayed.Click += (o, e) => Test(true);
            btnAtOnce.Click += (o, e) => Test(false);

            Test(false);
        }

        INotifier notifier;

        void Test(bool hasDelay)
        {
            var batch = Notification.BatchStart(StatusSeverity.Info, "Start");
            var next1 = batch.BatchNext(StatusSeverity.Info, "Next1");
            var next2 = next1.BatchNext(StatusSeverity.Info, "Next2");
            var done = batch.BatchLast(StatusSeverity.Success, "Yay");

            var mockStatusUPdates = new Notification[]
            {
                new Notification(StatusSeverity.Fail, "fail"), 
                batch,
                next1,
                new Notification(StatusSeverity.Info, "info"), 
                next2,
                new Notification(StatusSeverity.Warn, "warn"), 
                new Notification(StatusSeverity.Success, "really long text with a lot of text alsdjfksjadfl;jkasdflkjs;dfjslkjfdskljdfklsj;dfljslkdfjskldfjlsjdfkljsdfjslkdjfkl"), 
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

            var observable = hasDelay
                    ? statusFeedDelay// this one adds them all at once
                    : mockStatusUPdates.ToObservable();// this on

                observable
                    .ObserveOn(this)
                    .DelaySubscription(TimeSpan.FromSeconds(2))
                    .Subscribe(notifier.Notify);
        }
    }
}
