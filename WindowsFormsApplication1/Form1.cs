using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using GitUI.Notifications;
using GitUIPluginInterfaces.Notifications;

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

            //NotificationFeed notificationFeed = new NotificationFeed();
            //statusStrip.Items.Insert(0, notificationFeed);

            btnDelayed.Click += (o, e) => Test(true);
            btnAtOnce.Click += (o, e) => Test(false);

            Test(false);
        }

        void Test(bool hasDelay)
        {
            var mockStatusUPdates = new Notification[]
            {
                new Notification(StatusSeverity.Fail, "fail"), 
                new Notification(StatusSeverity.Info, "info"), 
                new Notification(StatusSeverity.Warn, "warn"), 
                new Notification(StatusSeverity.Success, "really long text with a lot of text alsdjfksjadfl;jkasdflkjs;dfjslkjfdskljdfklsj;dfljslkdfjskldfjlsjdfkljsdfjslkdjfkl"), 
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

            //observable
            //    .ObserveOn(this)
            //    .DelaySubscription(TimeSpan.FromSeconds(2))
            //    .Subscribe(notifier.Notify);
        }
    }
}
