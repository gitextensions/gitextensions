using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        public struct DTTOPTS
        {
            public int dwSize;
            public DTT dwFlags;
            public int crText;
            public int crBorder;
            public int crShadow;
            public TEXTSHADOWTYPE iTextShadowType;
            public POINT ptShadowOffset;
            public int iBorderSize;
            public int iFontPropId;
            public int iColorPropId;
            public int iStateId;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fApplyOverlay;
            public int iGlowSize;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public DTT_CALLBACK_PROC pfnDrawTextCallback;
            public IntPtr lParam;
        }
    }
}
