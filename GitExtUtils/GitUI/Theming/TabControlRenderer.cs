using System;
using System.Windows.Forms;
using GitUI;

namespace GitExtUtils.GitUI.Theming
{
    internal class TabControlRenderer
    {
        private readonly TabControl _tabs;

        public TabControlRenderer(TabControl tabs)
        {
            _tabs = tabs;
        }

        public void Setup()
        {
            _tabs.SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            _tabs.Paint += HandlePaint;
            _tabs.Disposed += HandleDisposed;
        }

        private void HandlePaint(object s, PaintEventArgs e) =>
            new TabControlPaintContext((TabControl)s, e).Paint();

        private void HandleDisposed(object sender, EventArgs e)
        {
            _tabs.Paint -= HandlePaint;
            _tabs.Disposed -= HandleDisposed;
        }
    }
}
