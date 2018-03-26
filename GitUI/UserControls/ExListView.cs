using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GitCommands.Utils;

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
        public ExListView()
        {
            DoubleBuffered = true;
        }

        #region Win32 Apis

        protected static class NativeMethods
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

            #region Windows constants

            public const int WM_LBUTTONDOWN = 0x0201;
            public const int WM_LBUTTONUP = 0x0202;
            public const int WM_PAINT = 0x0F;
            public const int WM_REFLECT_NOTIFY = 0x204E;
            public const int LVM_FIRST = 0x1000;
            public const int LVM_HITTEST = LVM_FIRST + 18;
            public const int LVM_SETGROUPINFO = LVM_FIRST + 147;
            public const int LVM_SUBITEMHITTEST = LVM_FIRST + 57;

            #endregion Windows constants

            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public readonly int X;
                public readonly int Y;

                public POINT(int x, int y)
                {
                    X = x;
                    Y = y;
                }

                public static implicit operator System.Drawing.Point(POINT p)
                {
                    return new System.Drawing.Point(p.X, p.Y);
                }

                public static implicit operator POINT(System.Drawing.Point p)
                {
                    return new POINT(p.X, p.Y);
                }
            }

            public static POINT LParamToPOINT(uint lParam)
            {
                uint ulParam = lParam;
                return new POINT(
                    (int)(ulParam & 0x0000ffff),
                    (int)((ulParam & 0xffff0000) >> 16));
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

        private bool _isInWmPaintMsg;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case NativeMethods.WM_PAINT:
                    _isInWmPaintMsg = true;
                    base.WndProc(ref m);
                    _isInWmPaintMsg = false;
                    break;
                case NativeMethods.WM_REFLECT_NOTIFY:
                    var nmhdr = (NativeMethods.NMHDR)m.GetLParam(typeof(NativeMethods.NMHDR));
                    if (nmhdr.code == -12)
                    {
                        // NM_CUSTOMDRAW
                        if (_isInWmPaintMsg)
                        {
                            base.WndProc(ref m);
                        }
                    }
                    else
                    {
                        base.WndProc(ref m);
                    }

                    break;
                case NativeMethods.WM_LBUTTONUP:
                case NativeMethods.WM_LBUTTONDOWN:
                    var info = new NativeMethods.LVHITTESTINFO();

                    info.pt = NativeMethods.LParamToPOINT((uint)m.LParam);

                    // if the click is on the group header, exit, otherwise send message
                    var handleRef = new HandleRef(this, Handle);
                    if (NativeMethods.SendMessage(handleRef, NativeMethods.LVM_SUBITEMHITTEST, (IntPtr)(-1), ref info) != new IntPtr(-1))
                    {
                        if ((info.flags & NativeMethods.LVHITTESTFLAGS.LVHT_EX_GROUP_HEADER) != 0)
                        {
                            return;
                        }
                    }

                    base.WndProc(ref m);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private static int? GetGroupID(ListViewGroup lstvwgrp)
        {
            int? rtnval = null;
            Type groupType = lstvwgrp.GetType();
            PropertyInfo pi = groupType.GetProperty("ID", BindingFlags.NonPublic |
                                                      BindingFlags.Instance);
            if (pi != null)
            {
                object tmprtnval = pi.GetValue(lstvwgrp, null);
                if (tmprtnval != null)
                {
                    rtnval = tmprtnval as int?;
                }
            }

            return rtnval;
        }

        private void SetGrpState(ListViewGroup lstvwgrp, ListViewGroupState state)
        {
            if (lstvwgrp == null)
            {
                return;
            }

            int? groupId = GetGroupID(lstvwgrp);
            int groupIndex = Groups.IndexOf(lstvwgrp);
            var group = new NativeMethods.LVGROUP();
            group.CbSize = Marshal.SizeOf(group);
            group.State = state;
            group.Mask = NativeMethods.ListViewGroupMask.State;
            var handleRef = new HandleRef(this, Handle);
            group.IGroupId = groupId ?? groupIndex;
            NativeMethods.SendMessage(handleRef,
                NativeMethods.LVM_SETGROUPINFO, (IntPtr)group.IGroupId, ref group);
            NativeMethods.SendMessage(handleRef,
                NativeMethods.LVM_SETGROUPINFO, (IntPtr)group.IGroupId, ref group);
            Refresh();
        }

        public void SetGroupState(ListViewGroupState state)
        {
            if (!EnvUtils.RunningOnWindows() || Environment.OSVersion.Version.Major < 6)
            {
                // Only Vista and forward
                // allows collapse of ListViewGroups
                return;
            }

            foreach (ListViewGroup lvg in Groups)
            {
                SetGrpState(lvg, state);
            }
        }
    }
}
