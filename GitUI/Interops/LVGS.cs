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
    }
}
