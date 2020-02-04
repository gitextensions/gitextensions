namespace System
{
    internal static partial class NativeMethods
    {
        [Flags]
        public enum DTT : int
        {
            TextColor = 1,
            BorderColor = 1 << 1,
            ShadowColor = 1 << 2,
            ShadowType = 1 << 3,
            ShadowOffset = 1 << 4,
            BorderSize = 1 << 5,
            FontProp = 1 << 6,
            ColorProp = 1 << 7,
            StateID = 1 << 8,
            CalcRect = 1 << 9,
            ApplyOverlay = 1 << 10,
            GlowSize = 1 << 11,
            Callback = 1 << 12,
            Composited = 1 << 13
        }
    }
}
