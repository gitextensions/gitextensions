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

namespace GitUI.UserControls
{
    public enum ListViewGroupState : uint
    {
        /// <summary>
        /// Groups are expanded, the group name is displayed,
        /// and all items in the group are displayed.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// The group is collapsed.
        /// </summary>
        Collapsed = 1,

        /// <summary>
        /// The group is hidden.
        /// </summary>
        Hidden = 2,

        /// <summary>
        /// Version 6.00 and Windows Vista. The group does not display a header.
        /// </summary>
        NoHeader = 4,

        /// <summary>
        /// Version 6.00 and Windows Vista. The group can be collapsed.
        /// </summary>
        Collapsible = 8,

        /// <summary>
        /// Version 6.00 and Windows Vista. The group has keyboard focus.
        /// </summary>
        Focused = 16,

        /// <summary>
        /// Version 6.00 and Windows Vista. The group is selected.
        /// </summary>
        Selected = 32,

        /// <summary>
        /// Version 6.00 and Windows Vista. The group displays only a portion of its items.
        /// </summary>
        SubSeted = 64,

        /// <summary>
        /// Version 6.00 and Windows Vista. The subset link of the group has keyboard focus.
        /// </summary>
        SubSetLinkFocused = 128
    }

    internal class ExListView : NativeListView
    {
        private static readonly PropertyInfo ListViewGroupIdProperty;
        private static readonly PropertyInfo ListViewGroupListProperty;
        private static readonly PropertyInfo ListViewDefaultGroupProperty;

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

        /// <summary>
        /// Call this method once after <see cref="ListView.Groups"/> collection was cleared (or created)
        /// before making any calls to <see cref="ListViewGroupCollection.Insert"/>
        ///
        /// <see cref="ListViewGroupCollection.Add(System.Windows.Forms.ListViewGroup)"/> calls must
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

        [Category("Behavior"), DefaultValue(false)]
        public bool AllowCollapseGroups { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private bool IsGroupStateSupported { get; }

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

        #region Win32 Apis

        private static class NativeMethods
        {
            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern IntPtr SendMessage(HandleRef hWnd,
                                                   int msg,
                                                   IntPtr wParam,
                                                   ref LVHITTESTINFO lParam);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern IntPtr SendMessage(HandleRef hWnd,
                                                   int msg,
                                                   IntPtr wParam,
                                                   ref LVGROUP lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern int GetScrollPos(IntPtr hWnd, int nBar);

            #region Windows constants

            public const int WM_LBUTTONDOWN = 0x0201;
            public const int WM_LBUTTONUP = 0x0202;
            public const int WM_RBUTTONDOWN = 0x0204;
            public const int WM_RBUTTONUP = 0x0205;
            public const int WM_PAINT = 0x0F;
            public const int WM_REFLECT_NOTIFY = 0x204E;

            public const int WM_VSCROLL = 0x115;
            public const int WM_MOUSEWHEEL = 0x020A;
            public const int WM_KEYDOWN = 0x0100;
            public const int SB_VERT = 1;

            public const int LVM_FIRST = 0x1000;
            public const int LVM_HITTEST = LVM_FIRST + 18;
            public const int LVM_SETGROUPINFO = LVM_FIRST + 147;
            public const int LVM_SUBITEMHITTEST = LVM_FIRST + 57;
            public const int LVM_INSERTGROUP = LVM_FIRST + 145;

            public const int NM_FIRST = 0;
            public const int NM_CUSTOMDRAW = NM_FIRST - 12;

            #endregion

            [StructLayout(LayoutKind.Sequential)]
            public readonly struct POINT
            {
                public readonly int X;
                public readonly int Y;

                public POINT(int x, int y)
                {
                    X = x;
                    Y = y;
                }

                public static implicit operator System.Drawing.Point(POINT p) => new System.Drawing.Point(p.X, p.Y);
                public static implicit operator POINT(System.Drawing.Point p) => new POINT(p.X, p.Y);
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct NMHDR
            {
                public IntPtr hwndFrom;
                public IntPtr idFrom;
                public int code;
            }

            /// <summary>
            /// see http://msdn.microsoft.com/en-us/library/bb774754%28v=VS.85%29.aspx
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct LVHITTESTINFO
            {
                public POINT pt;
                public LVHITTESTFLAGS flags;
                public int iItem;
                public int iSubItem;

                // Vista/Win7+
                public int iGroup;
            }

            /// <summary>
            /// see http://msdn.microsoft.com/en-us/library/bb774754%28v=VS.85%29.aspx
            /// </summary>
            [Flags]
            public enum LVHITTESTFLAGS : uint
            {
                LVHT_NOWHERE = 0x00000001,
                LVHT_ONITEMICON = 0x00000002,
                LVHT_ONITEMLABEL = 0x00000004,
                LVHT_ONITEMSTATEICON = 0x00000008,
                LVHT_ONITEM = LVHT_ONITEMICON | LVHT_ONITEMLABEL | LVHT_ONITEMSTATEICON,
                LVHT_ABOVE = 0x00000008,
                LVHT_BELOW = 0x00000010,
                LVHT_TORIGHT = 0x00000020,
                LVHT_TOLEFT = 0x00000040,

                // Vista/Win7+ only
                LVHT_EX_GROUP_HEADER = 0x10000000,
                LVHT_EX_GROUP_FOOTER = 0x20000000,
                LVHT_EX_GROUP_COLLAPSE = 0x40000000,
                LVHT_EX_GROUP_BACKGROUND = 0x80000000,
                LVHT_EX_GROUP_STATEICON = 0x01000000,
                LVHT_EX_GROUP_SUBSETLINK = 0x02000000,
            }

            public enum ListViewGroupMask : uint
            {
                None = 0x00000,
                Header = 0x00001,
                Footer = 0x00002,
                State = 0x00004,
                Align = 0x00008,
                GroupId = 0x00010,
                SubTitle = 0x00100,
                Task = 0x00200,
                DescriptionTop = 0x00400,
                DescriptionBottom = 0x00800,
                TitleImage = 0x01000,
                ExtendedImage = 0x02000,
                Items = 0x04000,
                Subset = 0x08000,
                SubsetItems = 0x10000
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct LVGROUP
            {
                public int CbSize;
                public ListViewGroupMask Mask;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string PszHeader;
                public int CchHeader;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string PszFooter;
                public int CchFooter;
                public int IGroupId;
                public int StateMask;
                public ListViewGroupState State;
                public uint UAlign;
            }
        }
        #endregion

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // Prevent flickering horizontal scrollbar when control width is reduced,
            // by preventing redrawing the control before column width is adjusted to new width in
            // some event handler e.g. ClientSizeChanged.
            BeginUpdate();
            base.SetBoundsCore(x, y, width, height, specified);
            EndUpdate();
        }

        private bool _isInWmPaintMsg;

        protected override void WndProc(ref Message m)
        {
            var message = m;

            switch (m.Msg)
            {
                case NativeMethods.WM_LBUTTONUP when GetGroupHitInfo(message.LParam.ToPoint())?.IsCollapseButton == true:
                    DefWndProc(ref m); // collapse / expand by clicking button in group header
                    break;

                case NativeMethods.LVM_INSERTGROUP:
                    base.WndProc(ref m);
                    HandleAddedGroup(m);
                    break;

                case NativeMethods.WM_PAINT:
                    _isInWmPaintMsg = true;
                    base.WndProc(ref m);
                    _isInWmPaintMsg = false;
                    break;

                case NativeMethods.WM_REFLECT_NOTIFY when IsCustomDraw(m) && !_isInWmPaintMsg:
                case NativeMethods.WM_RBUTTONUP when IsGroupMouseEventHandled(MouseButtons.Right, isDown: false):
                case NativeMethods.WM_RBUTTONDOWN when IsGroupMouseEventHandled(MouseButtons.Right, isDown: true):
                case NativeMethods.WM_LBUTTONUP when IsGroupMouseEventHandled(MouseButtons.Left, isDown: false):
                case NativeMethods.WM_LBUTTONDOWN when IsGroupMouseEventHandled(MouseButtons.Left, isDown: true):
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
                    case NativeMethods.WM_VSCROLL:
                        type = (ScrollEventType)LowWord(msg.WParam.ToInt64());
                        newValue = HighWord(msg.WParam.ToInt64());
                        break;

                    case NativeMethods.WM_MOUSEWHEEL:
                        type = HighWord(msg.WParam.ToInt64()) > 0
                            ? ScrollEventType.SmallDecrement
                            : ScrollEventType.SmallIncrement;
                        break;

                    case NativeMethods.WM_KEYDOWN:
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

                newValue = newValue ?? NativeMethods.GetScrollPos(Handle, NativeMethods.SB_VERT);
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
                SetGrpState(listViewGroup, ListViewGroupState.Collapsible);
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

                void SetGrpState(ListViewGroup grp, ListViewGroupState state)
                {
                    int groupId = GetGroupId(grp);
                    if (groupId < 0)
                    {
                        groupId = Groups.IndexOf(grp) - _minGroupInsertionIndex;
                    }

                    var lvgroup = new NativeMethods.LVGROUP();
                    lvgroup.CbSize = Marshal.SizeOf(lvgroup);
                    lvgroup.State = state;
                    lvgroup.Mask = NativeMethods.ListViewGroupMask.State;
                    lvgroup.IGroupId = groupId;

                    var handleRef = new HandleRef(this, Handle);

                    NativeMethods.SendMessage(handleRef, NativeMethods.LVM_SETGROUPINFO, (IntPtr)groupId, ref lvgroup);
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

            bool IsCustomDraw(Message msg)
            {
                var nmhdr = (NativeMethods.NMHDR)msg.GetLParam(typeof(NativeMethods.NMHDR));
                return nmhdr.code == NativeMethods.NM_CUSTOMDRAW;
            }
        }

        [CanBeNull]
        public ListViewGroupHitInfo GetGroupHitInfo(Point location)
        {
            var info = new NativeMethods.LVHITTESTINFO
            {
                pt = location
            };

            var handleRef = new HandleRef(this, Handle);
            if (NativeMethods.SendMessage(handleRef, NativeMethods.LVM_SUBITEMHITTEST, (IntPtr)(-1), ref info) == new IntPtr(-1))
            {
                return null;
            }

            if ((info.flags & NativeMethods.LVHITTESTFLAGS.LVHT_EX_GROUP_HEADER) == 0)
            {
                return null;
            }

            foreach (ListViewGroup group in Groups)
            {
                var groupId = GetGroupId(group);
                if (info.iItem == groupId)
                {
                    bool isCollapseButton = (info.flags & NativeMethods.LVHITTESTFLAGS.LVHT_EX_GROUP_COLLAPSE) > 0;
                    return new ListViewGroupHitInfo(group, isCollapseButton, location);
                }
            }

            return null;
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

        /// <summary> Position in <see cref="ListView.Groups"/> collection after the technical "Default" group </summary>
        private int _minGroupInsertionIndex;
    }
}
