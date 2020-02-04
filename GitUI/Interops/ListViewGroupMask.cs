namespace System
{
    internal static partial class NativeMethods
    {
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
    }
}
