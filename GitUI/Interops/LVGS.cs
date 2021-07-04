using Windows.Win32;

namespace System
{
    internal static partial class NativeMethods
    {
        [Flags]
        public enum LVGS : uint
        {
            /// <summary>
            /// Groups are expanded, the group name is displayed,
            /// and all items in the group are displayed.
            /// </summary>
            Normal = Constants.LVGS_NORMAL,

            /// <summary>
            /// The group is collapsed.
            /// </summary>
            Collapsed = Constants.LVGS_COLLAPSED,

            /// <summary>
            /// The group is hidden.
            /// </summary>
            Hidden = Constants.LVGS_HIDDEN,

            /// <summary>
            /// Version 6.00 and Windows Vista. The group does not display a header.
            /// </summary>
            NoHeader = Constants.LVGS_NOHEADER,

            /// <summary>
            /// Version 6.00 and Windows Vista. The group can be collapsed.
            /// </summary>
            Collapsible = Constants.LVGS_COLLAPSIBLE,

            /// <summary>
            /// Version 6.00 and Windows Vista. The group has keyboard focus.
            /// </summary>
            Focused = Constants.LVGS_FOCUSED,

            /// <summary>
            /// Version 6.00 and Windows Vista. The group is selected.
            /// </summary>
            Selected = Constants.LVGS_SELECTED,

            /// <summary>
            /// Version 6.00 and Windows Vista. The group displays only a portion of its items.
            /// </summary>
            SubSeted = Constants.LVGS_SUBSETED,

            /// <summary>
            /// Version 6.00 and Windows Vista. The subset link of the group has keyboard focus.
            /// </summary>
            SubSetLinkFocused = Constants.LVGS_SUBSETLINKFOCUSED,
        }
    }
}
