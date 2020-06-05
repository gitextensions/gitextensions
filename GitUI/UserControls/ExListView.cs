using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GitCommands.Utils;
using JetBrains.Annotations;
using static System.NativeMethods;

namespace GitUI.UserControls
{
    internal class ExListView : NativeListView
    {
        private static readonly PropertyInfo ListViewGroupIdProperty;
        private static readonly PropertyInfo ListViewGroupListProperty;
        private static readonly PropertyInfo ListViewDefaultGroupProperty;

        /// <summary> Position in <see cref="ListView.Groups"/> collection after the technical "Default" group </summary>
        private int _minGroupInsertionIndex;

        /// <summary>
        /// Occurs when the user presses mouse button in a <see cref="ListViewGroup"/> within the list view control.
        /// </summary>
        public event EventHandler<ListViewGroupMouseEventArgs> GroupMouseDown;

        /// <summary>
        /// Occurs when the user releases mouse button in a <see cref="ListViewGroup"/> within the list view control.
        /// </summary>
        public event EventHandler<ListViewGroupMouseEventArgs> GroupMouseUp;

        public event ScrollEventHandler Scroll;

        static ExListView()
        {
            ListViewGroupIdProperty = typeof(ListViewGroup).GetProperty("ID",
                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic);

            ListViewGroupListProperty = typeof(ListViewGroupCollection).GetProperty("List",
                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic);

            ListViewDefaultGroupProperty = typeof(ListView).GetProperty("DefaultGroup",
                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public ExListView()
        {
            DoubleBuffered = true;
            IsGroupStateSupported = EnvUtils.RunningOnWindows() && Environment.OSVersion.Version.Major >= 6;
        }

        [Category("Behavior"), DefaultValue(false)]
        public bool AllowCollapseGroups { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private bool IsGroupStateSupported { get; }

        [CanBeNull]
        public ListViewGroupHitInfo GetGroupHitInfo(Point location)
        {
            var info = new LVHITTESTINFO
            {
                pt = location
            };

            if (SendMessageW(Handle, LVM_SUBITEMHITTEST, (IntPtr)(-1), ref info) == new IntPtr(-1))
            {
                return null;
            }

            if ((info.flags & LVHITTESTFLAGS.LVHT_EX_GROUP_HEADER) == 0)
            {
                return null;
            }

            foreach (ListViewGroup group in Groups)
            {
                var groupId = GetGroupId(group);
                if (info.iItem == groupId)
                {
                    bool isCollapseButton = (info.flags & LVHITTESTFLAGS.LVHT_EX_GROUP_COLLAPSE) > 0;
                    return new ListViewGroupHitInfo(group, isCollapseButton, location);
                }
            }

            return null;
        }

        /// <summary>
        /// Modifies <see cref="ListView.Groups"/> collection to match supplied one.
        /// </summary>
        /// <remarks>
        /// The order of newly inserted groups is preserved.
        /// The mutual order of already existing groups is left intact to keep algorithm simpler and non-destructive.
        /// </remarks>
        public void SetGroups(IReadOnlyList<ListViewGroup> groups, StringComparer nameComparer)
        {
            var groupNames = new HashSet<string>(groups.Select(g => g.Name), nameComparer);

            BeginGroupInsertion();

            try
            {
                var toRemove = GetGroups().Where(grp => !groupNames.Contains(grp.Name)).ToArray();
                foreach (var grp in toRemove)
                {
                    Groups.Remove(grp);
                }

                var existingGroups = GetGroups().ToArray();

                // leave only names to be inserted
                groupNames.ExceptWith(existingGroups.Select(g => g.Name));
                for (int i = 0; i < groups.Count; i++)
                {
                    if (groupNames.Contains(groups[i].Name))
                    {
                        Groups.Insert(i + _minGroupInsertionIndex, groups[i]);
                    }
                }
            }
            finally
            {
                EndGroupInsertion();
            }

            IEnumerable<ListViewGroup> GetGroups() =>
                Groups.Cast<ListViewGroup>().Skip(_minGroupInsertionIndex);
        }

        protected override void WndProc(ref Message m)
        {
            var message = m;
            switch (m.Msg)
            {
                case WM_LBUTTONUP when GetGroupHitInfo(message.LParam.ToPoint())?.IsCollapseButton == true:
                    DefWndProc(ref m); // collapse / expand by clicking button in group header
                    break;

                case LVM_INSERTGROUP:
                    base.WndProc(ref m);
                    HandleAddedGroup(m);
                    break;

                case WM_RBUTTONUP when IsGroupMouseEventHandled(MouseButtons.Right, isDown: false):
                case WM_RBUTTONDOWN when IsGroupMouseEventHandled(MouseButtons.Right, isDown: true):
                case WM_LBUTTONUP when IsGroupMouseEventHandled(MouseButtons.Left, isDown: false):
                case WM_LBUTTONDOWN when IsGroupMouseEventHandled(MouseButtons.Left, isDown: true):
                    break;

                default:
                    HandleScroll(m);
                    base.WndProc(ref m);
                    break;
            }

            void HandleScroll(Message msg)
            {
                ScrollEventType type;
                int? newValue = null;

                switch (msg.Msg)
                {
                    case WM_VSCROLL:
                        type = (ScrollEventType)LowWord(msg.WParam.ToInt64());
                        newValue = HighWord(msg.WParam.ToInt64());
                        break;

                    case WM_MOUSEWHEEL:
                        type = HighWord(msg.WParam.ToInt64()) > 0
                            ? ScrollEventType.SmallDecrement
                            : ScrollEventType.SmallIncrement;
                        break;

                    case WM_KEYDOWN:
                        switch ((Keys)msg.WParam.ToInt32())
                        {
                            case Keys.Up:
                                type = ScrollEventType.SmallDecrement;
                                break;
                            case Keys.Down:
                                type = ScrollEventType.SmallIncrement;
                                break;
                            case Keys.PageUp:
                                type = ScrollEventType.LargeDecrement;
                                break;
                            case Keys.PageDown:
                                type = ScrollEventType.LargeIncrement;
                                break;
                            case Keys.Home:
                                type = ScrollEventType.First;
                                break;
                            case Keys.End:
                                type = ScrollEventType.Last;
                                break;
                            default:
                                return;
                        }

                        break;

                    default:
                        return;
                }

                newValue = newValue ?? GetScrollPos(Handle, SB.VERT);
                Scroll?.Invoke(this, new ScrollEventArgs(type, newValue.Value));

                short LowWord(long number) =>
                    unchecked((short)(number & 0x0000ffff));

                short HighWord(long number) =>
                    unchecked((short)(number >> 16));
            }

            void HandleAddedGroup(Message msg)
            {
                if (!IsGroupStateSupported || !AllowCollapseGroups)
                {
                    return;
                }

                int groupIndex = GetGroupIndex();
                if (groupIndex < _minGroupInsertionIndex)
                {
                    return;
                }

                var listViewGroup = Groups[groupIndex];
                SetGroupState(listViewGroup, LVGS.Collapsible);
                Invalidate();

                int GetGroupIndex()
                {
                    // https://docs.microsoft.com/en-us/windows/desktop/controls/lvm-insertgroup
                    int index = msg.WParam.ToInt32();
                    if (index == -1)
                    {
                        // -1 because addition already happened
                        return Groups.Count - 1;
                    }

                    return index - 1 + _minGroupInsertionIndex;
                }
            }

            bool IsGroupMouseEventHandled(MouseButtons button, bool isDown)
            {
                var hitInfo = GetGroupHitInfo(message.LParam.ToPoint());

                if (hitInfo == null)
                {
                    return false;
                }

                var eventArgs = new ListViewGroupMouseEventArgs(button, hitInfo, 1, 0);
                if (isDown)
                {
                    GroupMouseDown?.Invoke(this, eventArgs);
                }
                else
                {
                    GroupMouseUp?.Invoke(this, eventArgs);
                }

                return eventArgs.Handled;
            }
        }

        public unsafe void SetGroupState(ListViewGroup group, LVGS state)
        {
            int groupId = GetGroupId(group);
            if (groupId < 0)
            {
                groupId = Groups.IndexOf(group) - _minGroupInsertionIndex;
            }

            var lvgroup = new LVGROUPW();
            lvgroup.cbSize = (uint)sizeof(LVGROUPW);
            lvgroup.state = state;
            lvgroup.mask = LVGF.STATE;
            lvgroup.iGroupId = groupId;

            NativeMethods.SendMessageW(Handle, LVM_SETGROUPINFO, (IntPtr)groupId, ref lvgroup);
        }

        /// <summary>
        /// Call this method once after <see cref="ListView.Groups"/> collection was cleared (or created)
        /// before making any calls to <see cref="ListViewGroupCollection.Insert"/>
        ///
        /// <see cref="ListViewGroupCollection.Add(ListViewGroup)"/> calls must
        /// not be used after calling this method
        /// </summary>
        private void BeginGroupInsertion()
        {
            // .NET ListView.Groups.Insert(...) implementation has a bug
            // It does not count the technical "Default" group from ListView when passing inserted
            // group index to native Win32 ListView.

            // ListView.Groups.Add(...) implementation on the other hand does count the
            // technical "Default" group correctly therefore it becomes broken after calling this method

            var defaultGroup = ListViewDefaultGroupProperty.GetValue(this);
            var list = (ArrayList)ListViewGroupListProperty.GetValue(Groups);
            if (list.Count > 0 && list[0] == defaultGroup)
            {
                throw new InvalidOperationException($"{nameof(BeginGroupInsertion)} was already called");
            }

            list.Insert(0, defaultGroup);
            _minGroupInsertionIndex = 1;
        }

        private void EndGroupInsertion()
        {
            var defaultGroup = ListViewDefaultGroupProperty.GetValue(this);
            var list = (ArrayList)ListViewGroupListProperty.GetValue(Groups);

            if (list.Count == 0 || list[0] != defaultGroup)
            {
                throw new InvalidOperationException($"{nameof(BeginGroupInsertion)} was not called before");
            }

            list.RemoveAt(0);
            _minGroupInsertionIndex = 0;
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // Prevent flickering horizontal scrollbar when control width is reduced,
            // by preventing redrawing the control before column width is adjusted to new width in
            // some event handler e.g. ClientSizeChanged.
            BeginUpdate();
            base.SetBoundsCore(x, y, width, height, specified);
            EndUpdate();
        }

        private static int GetGroupId(ListViewGroup listViewGroup)
        {
            if (ListViewGroupIdProperty != null)
            {
                try
                {
                    return (int)ListViewGroupIdProperty.GetValue(listViewGroup);
                }
                catch
                {
                    // no-op
                }
            }

            return -1;
        }
    }
}
