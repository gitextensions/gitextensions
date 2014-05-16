using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces.Notifications;
using ResourceManager;

namespace GitUI.Notifications
{
    /// <summary>Popup for current/most recent notification notification.</summary>
    public partial class NotificationPopup : GitExtensionsForm
    {
        TranslationString _of = new TranslationString("of");

        NotificationsForm notificationsForm;

        public NotificationPopup()
        {
            InitializeComponent();

            Opacity = 0;

            fadeTimer = new Timer { Interval = 100 };

            btnNewest.Click += OnNewestClick;
            btnNewer.Click += OnNewerClick;
            btnOlder.Click += OnOlderClick;
            btnOldest.Click += OnOldestClick;
            btnXofY.Click += OnXofYClick;

            Disposed += OnDisposed;
        }

        void OnXofYClick(object sender, EventArgs e)
        {
            notificationsForm.Show();
        }

        /// <summary>times the fade in/out</summary>
        Timer fadeTimer;
        /// <summary>true if popup is fading in</summary>
        bool isFadingIn;
        /// <summary>true if popup is fading out</summary>
        bool isFadingOut;
        /// <summary>0.1 : amount opacity is incremented/decremented during fading</summary>
        static readonly double OpacityIncrement = 0.1;

        /// <summary>true if popup is completely visible</summary>
        public bool IsFadedIn { get { return (Opacity >= 1); } }
        /// <summary>true if popup is completely transparent</summary>
        public bool IsFadedOut { get { return (Opacity <= 0); } }

        /// <summary>Immediately makes the popup opaque.
        /// <remarks>Starting to fade out and a new message arrives.</remarks></summary>
        void Opaque()
        {
            if (IsFadedIn) { return; }

            // cancel any fading
            fadeTimer.Stop();
            if (isFadingIn)
            {
                fadeTimer.Tick -= FadeInTick;
            }
            if (isFadingOut)
            {
                fadeTimer.Tick -= FadeOutTick;
            }

            Opacity = 1;
        }

        /// <summary>Animates display of the popup (fade in).</summary>
        void StartFadeIn()
        {
            if (IsFadedIn || isFadingIn) { return; }// already faded/fading in

            if (isFadingOut)
            {
                fadeTimer.Stop();
            }

            isFadingIn = false;
            isFadingOut = false;

            fadeTimer.Tick += FadeInTick;
            fadeTimer.Start();
        }

        /// <summary>Animates hiding the popop (fade out).</summary>
        void StartFadeOut()
        {
            isFadingIn = false;
            isFadingOut = true;
            fadeTimer.Tick -= FadeInTick;
            fadeTimer.Tick += FadeOutTick;
            fadeTimer.Start();
        }

        /// <summary>Occurs</summary>
        void FadeInTick(object sender, EventArgs e)
        {
            Opacity += OpacityIncrement;
            if (IsFadedIn)
            {
                fadeTimer.Stop();
            }
        }

        void FadeOutTick(object sender, EventArgs e)
        {
            Opacity -= OpacityIncrement;
            if (IsFadedOut)
            {// transparent -> close
                fadeTimer.Stop();
                Close();
            }
        }

        /// <summary>Disposes the <see cref="relavanceTimer"/>.</summary>
        void OnDisposed(object sender, EventArgs e)
        {
            relavanceTimer.Dispose();
            relavanceTimer = null;
            fadeTimer.Dispose();
            fadeTimer = null;
        }

        void OnOldestClick(object sender, EventArgs e)
        {
            SetCurrent(notificationList.Count - 1);
        }

        void OnOlderClick(object sender, EventArgs e)
        {
            Debug.Assert((iCurrent - 1) >= 0);
            SetCurrent(iCurrent - 1);
        }

        void OnNewerClick(object sender, EventArgs e)
        {
            Debug.Assert((iCurrent + 1) < notificationList.Count);
            SetCurrent(iCurrent + 1);
        }

        void OnNewestClick(object sender, EventArgs e)
        {
            SetCurrent(0);
        }

        int iCurrent;
        public Notification Current { get; private set; }

        /// <summary>Sets the current item to the <see cref="Notification"/> at the specified index.</summary>
        void SetCurrent(int index)
        {
            SetCurrent(notificationList[index], index);
        }

        /// <summary>Sets the current item to the specified <see cref="Notification"/>
        /// and updates the current index.</summary>
        void SetCurrent(Notification value, int index)
        {
            Current = value;
            iCurrent = index;

            if (index == 0)
            {// current element EQUALS newest element
                btnNewest.Enabled = false;
                btnNewer.Enabled = false;
                btnOlder.Enabled = true;
                btnOldest.Enabled = true;
            }
            else if (index == (notificationList.Count - 1))
            {// current element EQUALS oldest element
                btnNewest.Enabled = true;
                btnNewer.Enabled = true;
                btnOlder.Enabled = false;
                btnOldest.Enabled = false;
            }
        }

        NotificationList notificationList;

        void Upate()
        {
            //imgIcon.Image = NotifierHelpers.GetImage(Current.Notification);
            txtText.Text = Current.Text;

        }

        /// <summary>Indicates that there is a new, incoming notification.</summary>
        public void New(Notification notificationView, Point point)
        {
            Location = point;
            Show();
            StartFadeIn();

            // "3 of 4"
            btnXofY.Text = string.Format(
                "{0} {1} {2}",
                iCurrent + 1,
                _of.Text,
                notificationList.Count);

            throw new NotImplementedException();
        }


        /// <summary>Gets the displayed notification.</summary>
        internal Notification Notification { get; private set; }
        public string Id { get; private set; }

        /// <summary>syncronizes the notification's relevancy duration</summary>
        Timer relavanceTimer;

        /// <summary>Updates the text and image for UI, and start expiration timer.</summary>
        void Update(Notification notification)
        {
            if (notification != null)
            {
                Notification = notification;
                Text = notification.Text;
                imgIcon.Image = notification.GetImage();
            }
            else
            {
                Text = string.Empty;
                imgIcon.Image = NotifierHelpers.blank;
            }
            Visible = true;
            hasExpired = false;
            StartExpiration(TimeSpan.FromSeconds(5));
        }

        /// <summary>Starts the relevance timer which will remove the notification when it expires.</summary>
        void StartExpiration(TimeSpan expiration)
        {
            relavanceTimer = new Timer { Interval = (int)expiration.TotalMilliseconds };
            relavanceTimer.Tick += OnExpiredInternal;
            relavanceTimer.Start();
        }

        public override string ToString()
        {
            return Notification.ToString();
        }

        bool isMouseInside = false;
        bool hasExpired = false;

        /// <summary>Disposes the <see cref="relavanceTimer"/>.</summary>
        void DisposeRelevanceTimer()
        {
            if (relavanceTimer == null) { return; }

            relavanceTimer.Stop();
            relavanceTimer.Tick -= OnExpiredInternal;
            relavanceTimer.Dispose();
            relavanceTimer = null;
        }

        /// <summary>Disposes the timer, and if mouse is OUTside,
        /// raises the <see cref="Expired"/> event, then hides the control.</summary>
        void OnExpiredInternal(object sender, EventArgs e)
        {
            DisposeRelevanceTimer();

            hasExpired = true;
            if (isMouseInside) { return; }// don't change the control while user inside

            OnExpired(sender, e);

            StartFadeOut();
        }

        /// <summary>Occurs when a notification's relevance has expired.</summary>
        public event EventHandler Expired;

        /// <summary>Raises the <see cref="Expired"/> event.</summary>
        void OnExpired(object sender, EventArgs e)
        {
            var handler = Expired;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        /// <summary>Sets a flag indicating the user is inside the control.</summary>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isMouseInside = true;
        }

        /// <summary>If the notification has expired, invokes <see cref="OnExpiredInternal"/>.</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            isMouseInside = false;
            if (hasExpired)
            {
                OnExpiredInternal(null, null);
            }
            base.OnMouseLeave(e);
        }
    }
}
