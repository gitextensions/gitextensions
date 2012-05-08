using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;

namespace GitUI.Editor.RichTextBoxExtension
{
    static class RichTextBoxXHTMLSupportExtension
    {
        /// <summary>
        /// Maintains performance while updating.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It is recommended to call this method before doing
        /// any major updates that you do not wish the user to
        /// see. Remember to call EndUpdate when you are finished
        /// with the update. Nested calls are supported.
        /// </para>
        /// <para>
        /// Calling this method will prevent redrawing. It will
        /// also setup the event mask of the underlying richedit
        /// control so that no events are sent.
        /// </para>
        /// </remarks>

        public static int BeginUpdate(this RichTextBox rtb)
        {
            // Prevent the control from raising any events.
            int oldEventMask = NativeMethods.SendMessage(rtb,
                NativeMethods.EM_SETEVENTMASK, 0, 0);

            // Prevent the control from redrawing itself.
            NativeMethods.SendMessage(rtb,
                NativeMethods.WM_SETREDRAW, 0, 0);

            return oldEventMask;
        }

        /// <summary>
        /// Resumes drawing and event handling.
        /// </summary>
        /// <remarks>
        /// This method should be called every time a call is made
        /// made to BeginUpdate. It resets the event mask to it's
        /// original value and enables redrawing of the control.
        /// </remarks>
        public static void EndUpdate(this RichTextBox rtb, int oldEventMask)
        {
            // Allow the control to redraw itself.
            NativeMethods.SendMessage(rtb,
                NativeMethods.WM_SETREDRAW, 1, 0);

            // Allow the control to raise event messages.
            NativeMethods.SendMessage(rtb,
                NativeMethods.EM_SETEVENTMASK, 0, oldEventMask);
        }

        // Defines for STRUCT_CHARFORMAT member dwMask
        [Flags]
        public enum CFM : uint
        {
            BOLD = 0x00000001,
            ITALIC = 0x00000002,
            UNDERLINE = 0x00000004,
            STRIKEOUT = 0x00000008,
            PROTECTED = 0x00000010,
            LINK = 0x00000020,
            SIZE = 0x80000000,
            COLOR = 0x40000000,
            FACE = 0x20000000,
            OFFSET = 0x10000000,
            CHARSET = 0x08000000,
            SUBSCRIPT = CFE.SUBSCRIPT | CFE.SUPERSCRIPT,
            SUPERSCRIPT = SUBSCRIPT,

            SMALLCAPS = 0x0040,			/* (*)	*/
            ALLCAPS = 0x0080,			/* Displayed by 3.0	*/
            HIDDEN = 0x0100,			/* Hidden by 3.0 */
            OUTLINE = 0x0200,			/* (*)	*/
            SHADOW = 0x0400,			/* (*)	*/
            EMBOSS = 0x0800,			/* (*)	*/
            IMPRINT = 0x1000,			/* (*)	*/
            DISABLED = 0x2000,
            REVISED = 0x4000,

            BACKCOLOR = 0x04000000,
            LCID = 0x02000000,
            UNDERLINETYPE = 0x00800000,		/* Many displayed by 3.0 */
            WEIGHT = 0x00400000,
            SPACING = 0x00200000,		/* Displayed by 3.0	*/
            KERNING = 0x00100000,		/* (*)	*/
            STYLE = 0x00080000,		/* (*)	*/
            ANIMATION = 0x00040000,		/* (*)	*/
            REVAUTHOR = 0x00008000
        }

        // Defines for STRUCT_CHARFORMAT member dwEffects
        [Flags]
        public enum CFE : uint
        {
            BOLD = 0x00000001,
            ITALIC = 0x00000002,
            UNDERLINE = 0x00000004,
            STRIKEOUT = 0x00000008,
            PROTECTED = 0x00000010,
            LINK = 0x00000020,
            AUTOCOLOR = 0x40000000,
            SUBSCRIPT = 0x00010000,		/* Superscript and subscript are */
            SUPERSCRIPT = 0x00020000,     /*  mutually exclusive			 */

            SMALLCAPS = 0x0040,			/* (*)	*/
            ALLCAPS = 0x0080,			/* Displayed by 3.0	*/
            HIDDEN = 0x0100,			/* Hidden by 3.0 */
            OUTLINE = 0x0200,			/* (*)	*/
            SHADOW = 0x0400,			/* (*)	*/
            EMBOSS = 0x0800,			/* (*)	*/
            IMPRINT = 0x1000,			/* (*)	*/
            DISABLED = 0x2000,
            REVISED = 0x4000,

            // CFE.AUTOCOLOR and CFE.AUTOBACKCOLOR correspond to CFM.COLOR and
            // CFM.BACKCOLOR, respectively, which control them
            AUTOBACKCOLOR = 0x04000000
        }

        public enum CFU : byte
        {
            UNDERLINENONE = 0x00,
            UNDERLINE = 0x01,
            UNDERLINEWORD = 0x02, /* (*) displayed as ordinary underline	*/
            UNDERLINEDOUBLE = 0x03, /* (*) displayed as ordinary underline	*/
            UNDERLINEDOTTED = 0x04,
            UNDERLINEDASH = 0x05,
            UNDERLINEDASHDOT = 0x06,
            UNDERLINEDASHDOTDOT = 0x07,
            UNDERLINEWAVE = 0x08,
            UNDERLINETHICK = 0x09,
            UNDERLINEHAIRLINE = 0x0A /* (*) displayed as ordinary underline	*/
        }

        // Font Weights
        public enum FW : short
        {
            DONTCARE = 0,
            THIN = 100,
            EXTRALIGHT = 200,
            LIGHT = 300,
            NORMAL = 400,
            MEDIUM = 500,
            SEMIBOLD = 600,
            BOLD = 700,
            EXTRABOLD = 800,
            HEAVY = 900,

            ULTRALIGHT = EXTRALIGHT,
            REGULAR = NORMAL,
            DEMIBOLD = SEMIBOLD,
            ULTRABOLD = EXTRABOLD,
            BLACK = HEAVY
        }

        // PARAFORMAT mask values
        [Flags]
        public enum PFM : uint
        {
            // PARAFORMAT mask values 
            STARTINDENT = 0x00000001,
            RIGHTINDENT = 0x00000002,
            OFFSET = 0x00000004,
            ALIGNMENT = 0x00000008,
            TABSTOPS = 0x00000010,
            NUMBERING = 0x00000020,
            OFFSETINDENT = 0x80000000,

            // PARAFORMAT 2.0 masks and effects 
            SPACEBEFORE = 0x00000040,
            SPACEAFTER = 0x00000080,
            LINESPACING = 0x00000100,
            STYLE = 0x00000400,
            BORDER = 0x00000800,	// (*)	
            SHADING = 0x00001000,	// (*)	
            NUMBERINGSTYLE = 0x00002000,	// RE 3.0	
            NUMBERINGTAB = 0x00004000,	// RE 3.0	
            NUMBERINGSTART = 0x00008000,	// RE 3.0	

            RTLPARA = 0x00010000,
            KEEP = 0x00020000,	// (*)	
            KEEPNEXT = 0x00040000,	// (*)	
            PAGEBREAKBEFORE = 0x00080000,	// (*)	
            NOLINENUMBER = 0x00100000,	// (*)	
            NOWIDOWCONTROL = 0x00200000,	// (*)	
            DONOTHYPHEN = 0x00400000,	// (*)	
            SIDEBYSIDE = 0x00800000,	// (*)	
            TABLE = 0x40000000,	// RE 3.0 
            TEXTWRAPPINGBREAK = 0x20000000,	// RE 3.0 
            TABLEROWDELIMITER = 0x10000000,	// RE 4.0 

            // The following three properties are read only
            COLLAPSED = 0x01000000,	// RE 3.0 
            OUTLINELEVEL = 0x02000000,	// RE 3.0 
            BOX = 0x04000000,	// RE 3.0 
            RESERVED2 = 0x08000000	// RE 4.0 
        }

        // PARAFORMAT numbering options
        public enum PFN : ushort
        {
            BULLET = 0x0001
        }

        // PARAFORMAT alignment options
        public enum PFA : ushort
        {
            LEFT = 0x0001,
            RIGHT = 0x0002,
            CENTER = 0x0003
        }

        // It makes no difference if we use PARAFORMAT or
        // PARAFORMAT2 here, so I have opted for PARAFORMAT2.
        [StructLayout(LayoutKind.Sequential)]
        public struct PARAFORMAT
        {
            public int cbSize;
            public PFM dwMask;
            public PFN wNumbering;
            public short wReserved;
            public int dxStartIndent;
            public int dxRightIndent;
            public int dxOffset;
            public PFA wAlignment;
            public short cTabCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public int[] rgxTabs;

            // PARAFORMAT2 from here onwards.
            public int dySpaceBefore;
            public int dySpaceAfter;
            public int dyLineSpacing;
            public short sStyle;
            public byte bLineSpacingRule;
            public byte bOutlineLevel;
            public short wShadingWeight;
            public short wShadingStyle;
            public short wNumberingStart;
            public short wNumberingStyle;
            public short wNumberingTab;
            public short wBorderSpace;
            public short wBorderWidth;
            public short wBorders;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct CHARFORMAT
        {
            public CHARFORMAT(CFM mask, CFE effects)
                : this()
            {
                dwMask = mask;
                dwEffects = effects;
            }

            public int cbSize;
            public CFM dwMask;
            public CFE dwEffects;
            public Int32 yHeight;
            public Int32 yOffset;
            public Int32 crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szFaceName;

            // CHARFORMAT2 from here onwards.
            public FW wWeight;
            public short sSpacing;
            public Int32 crBackColor;
            public uint lcid;
            public uint dwReserved;
            public short sStyle;
            public short wKerning;
            public CFU bUnderlineType;
            public byte bAnimation;
            public byte bRevAuthor;
            public byte bReserved1;
        }

        #region Win32 Apis
        internal class NativeMethods
        {
            // Constants from the Platform SDK.
            internal const int WM_USER = 0x0400;
            internal const int EM_GETCHARFORMAT = WM_USER + 58;
            internal const int EM_SETCHARFORMAT = WM_USER + 68;

            internal const int EM_SETEVENTMASK = 1073;
            internal const int EM_GETPARAFORMAT = 1085;
            internal const int EM_SETPARAFORMAT = 1095;
            internal const int WM_SETREDRAW = 11;

            // Defines for EM_SETCHARFORMAT/EM_GETCHARFORMAT
            internal const Int32 SCF_SELECTION = 0x0001;
            internal const Int32 SCF_WORD = 0x0002;
            internal const Int32 SCF_ALL = 0x0004;

            internal const int LF_FACESIZE = 32;

            [DllImport("user32", CharSet = CharSet.Auto)]
            internal static extern IntPtr SendMessage(HandleRef hWnd,
                UInt32 msg,
                IntPtr wParam,
                IntPtr lParam);

            internal static int SendMessage(RichTextBox rtb,
                UInt32 msg,
                int wParam,
                int lParam)
            {
                return (int)SendMessage(new HandleRef(rtb, rtb.Handle),
                    msg, (IntPtr)wParam, (IntPtr)lParam);
            }

            [DllImport("user32", CharSet = CharSet.Auto)]
            internal static extern IntPtr SendMessage(HandleRef hWnd,
                UInt32 msg,
                IntPtr wParam,
                ref PARAFORMAT lp);

            internal static void SendMessage(RichTextBox rtb,
                UInt32 msg,
                int wParam,
                ref PARAFORMAT pf)
            {
                SendMessage(new HandleRef(rtb, rtb.Handle),
                    msg, (IntPtr)wParam, ref pf);
            }

            [DllImport("user32", CharSet = CharSet.Auto)]
            internal static extern int SendMessage(HandleRef hWnd,
                UInt32 msg,
                IntPtr wParam,
                ref CHARFORMAT lp);

            internal static void SendMessage(RichTextBox rtb,
                UInt32 msg,
                int wParam,
                ref CHARFORMAT pf)
            {
                SendMessage(new HandleRef(rtb, rtb.Handle),
                    msg, (IntPtr)wParam, ref pf);
            }
        }
        #endregion

        //----------------------------
        public static void SetSuperScript(this RichTextBox rtb, bool bSet)
        {
            rtb.SetCharFormat(CFM.SUPERSCRIPT, bSet ? CFE.SUPERSCRIPT : 0);
        }

        public static void SetSubScript(this RichTextBox rtb, bool bSet)
        {
            rtb.SetCharFormat(CFM.SUBSCRIPT, bSet ? CFE.SUBSCRIPT : 0);
        }

        public static void SetLink(this RichTextBox rtb, bool bSet)
        {
            rtb.SetCharFormat(CFM.LINK, bSet ? CFE.LINK : 0);
        }

        public static bool IsSuperScript(this RichTextBox rtb)
        {
            CHARFORMAT cf = rtb.GetCharFormat();
            return ((cf.dwEffects & CFE.SUPERSCRIPT) == CFE.SUPERSCRIPT);
        }

        public static bool IsSubScript(this RichTextBox rtb)
        {
            CHARFORMAT cf = rtb.GetCharFormat();
            return ((cf.dwEffects & CFE.SUBSCRIPT) == CFE.SUBSCRIPT);
        }

        public static bool IsLink(this RichTextBox rtb)
        {
            CHARFORMAT cf = rtb.GetCharFormat();
            return ((cf.dwEffects & CFE.LINK) == CFE.LINK);
        }

        static void AddLink(this RichTextBox rtb, string text)
        {
            int position = rtb.SelectionStart;
            if (position < 0 || position > rtb.Text.Length)
                throw new ArgumentOutOfRangeException("position");

            rtb.SelectionStart = position;
            rtb.SelectedText = text;
            int length = rtb.SelectionStart - position;
            rtb.Select(position, length);
            rtb.SetLink(true);
            rtb.Select(position + length, 0);
        }

        static void AddLink(this RichTextBox rtb, string text, string hyperlink)
        {
            int position = rtb.SelectionStart;
            if (position < 0 || position > rtb.Text.Length)
                throw new ArgumentOutOfRangeException("position");

            rtb.SelectionStart = position;
            rtb.SelectedText = text;
            int length = rtb.SelectionStart - position;
            rtb.Select(position, length);
            string rtfText = rtb.SelectedRtf;
            int idx = rtfText.LastIndexOf('}');
            if (idx != -1)
            {
                string head = rtfText.Substring(0, idx);
                string tail = rtfText.Substring(idx);
                rtb.SelectedRtf = head + @"\v #" + hyperlink + @"\v0" + tail;
                length = rtb.SelectionStart - position;
            }
            rtb.SelectedRtf = ("{\rtf1\ansi " + text + "\v #") + hyperlink + "\v0}";
            rtb.Select(position, text.Length + hyperlink.Length + 1);
            rtb.SetLink(true);
            rtb.Select(position + text.Length + hyperlink.Length + 1, 0);
        }
        //----------------------------

        public static PARAFORMAT GetParaFormat(this RichTextBox rtb)
        {
            PARAFORMAT pf = new PARAFORMAT();
            pf.cbSize = Marshal.SizeOf(pf);

            // Get the alignment.
            NativeMethods.SendMessage(rtb,
                NativeMethods.EM_GETPARAFORMAT,
                NativeMethods.SCF_SELECTION, ref pf);

            return pf;
        }

        public static void SetParaFormat(this RichTextBox rtb, PARAFORMAT value)
        {
            PARAFORMAT pf = value;
            pf.cbSize = Marshal.SizeOf(pf);

            // Set the alignment.
            NativeMethods.SendMessage(rtb,
                NativeMethods.EM_SETPARAFORMAT,
                NativeMethods.SCF_SELECTION, ref pf);
        }

        public static PARAFORMAT GetDefaultParaFormat(this RichTextBox rtb)
        {
            PARAFORMAT pf = new PARAFORMAT();
            pf.cbSize = Marshal.SizeOf(pf);

            // Get the alignment.
            NativeMethods.SendMessage(rtb,
                NativeMethods.EM_GETPARAFORMAT,
                NativeMethods.SCF_ALL, ref pf);

            return pf;
        }

        public static void SetDefaultParaFormat(this RichTextBox rtb, PARAFORMAT value)
        {
            PARAFORMAT pf = value;
            pf.cbSize = Marshal.SizeOf(pf);

            // Set the alignment.
            NativeMethods.SendMessage(rtb,
                NativeMethods.EM_SETPARAFORMAT,
                NativeMethods.SCF_ALL, ref pf);
        }

        public static CHARFORMAT GetCharFormat(this RichTextBox rtb)
        {
            CHARFORMAT cf = new CHARFORMAT();
            cf.cbSize = Marshal.SizeOf(cf);

            // Get the alignment.
            NativeMethods.SendMessage(rtb,
                NativeMethods.EM_GETCHARFORMAT,
                NativeMethods.SCF_SELECTION, ref cf);

            return cf;
        }

        public static void SetCharFormat(this RichTextBox rtb, CHARFORMAT value)
        {
            CHARFORMAT cf = value;
            cf.cbSize = Marshal.SizeOf(cf);

            // Set the alignment.
            NativeMethods.SendMessage(rtb,
                NativeMethods.EM_SETCHARFORMAT,
                NativeMethods.SCF_SELECTION, ref cf);
        }

        public static void SetCharFormat(this RichTextBox rtb, CFM mask, CFE effects)
        {
            CHARFORMAT cf = new CHARFORMAT(mask, effects);
            rtb.SetCharFormat(cf);
        }

        public static CHARFORMAT GetDefaultCharFormat(this RichTextBox rtb)
        {
            CHARFORMAT cf = new CHARFORMAT();
            cf.cbSize = Marshal.SizeOf(cf);

            // Get the alignment.
            NativeMethods.SendMessage(rtb,
                NativeMethods.EM_GETCHARFORMAT,
                NativeMethods.SCF_ALL, ref cf);

            return cf;
        }

        public static void SetDefaultCharFormat(this RichTextBox rtb, CHARFORMAT value)
        {
            CHARFORMAT cf = value;
            cf.cbSize = Marshal.SizeOf(cf);

            // Set the alignment.
            NativeMethods.SendMessage(rtb,
                NativeMethods.EM_SETCHARFORMAT,
                NativeMethods.SCF_ALL, ref cf);
        }

        public static void SetDefaultCharFormat(this RichTextBox rtb, CFM mask, CFE effects)
        {
            CHARFORMAT cf = new CHARFORMAT(mask, effects);
            rtb.SetDefaultCharFormat(cf);
        }

        #region COLORREF helper functions

        // convert COLORREF to Color
        private static Color GetColor(int crColor)
        {
            byte r = (byte)(crColor);
            byte g = (byte)(crColor >> 8);
            byte b = (byte)(crColor >> 16);

            return Color.FromArgb(r, g, b);
        }

        // convert COLORREF to Color
        private static int GetCOLORREF(int r, int g, int b)
        {
            int r2 = r;
            int g2 = (g << 8);
            int b2 = (b << 16);

            int result = r2 | g2 | b2;

            return result;
        }

        private static int GetCOLORREF(Color color)
        {
            int r = color.R;
            int g = color.G;
            int b = color.B;

            return GetCOLORREF(r, g, b);
        }
        #endregion

        public static string GetUrl(this LinkClickedEventArgs e)
        {
            var v = e.LinkText.Split(new char[] { '#' }, 2);
            if (v.Length == 0)
                return "";
            if (v.Length == 1)
                return v[0];
            return v[1];
        }

        public static void GetLinkText(this LinkClickedEventArgs e, out string url, out string text)
        {
            var v = e.LinkText.Split(new char[] { '#' }, 2);
            if (v.Length == 0)
            {
                url = "";
                text = "";
                return;
            }
            text = v[0];
            if (v.Length == 1)
                url = v[0];
            else
                url = v[1];
        }

        public static void SetXHTMLText(this RichTextBox rtb, string xhtmlText)
        {
            rtb.Clear();

            Stack<CHARFORMAT> scf = new Stack<CHARFORMAT>();
            Stack<PARAFORMAT> spf = new Stack<PARAFORMAT>();
            List<KeyValuePair<int, int>> links = new List<KeyValuePair<int, int>>();

            CHARFORMAT cf = rtb.GetDefaultCharFormat(); // to apply character formatting
            PARAFORMAT pf = rtb.GetDefaultParaFormat(); // to apply paragraph formatting

            rtb.HideSelection = true;
            int oldMask = rtb.BeginUpdate();
            int hyperlinkStart = -1;
            string hyperlink = null;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;

            try
            {
                using (XmlReader reader = XmlReader.Create(new StringReader(xhtmlText), settings))
                {
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                switch (reader.Name.ToLower())
                                {
                                    case "b":
                                        cf.dwMask |= CFM.WEIGHT | CFM.BOLD;
                                        cf.dwEffects |= CFE.BOLD;
                                        cf.wWeight = FW.BOLD;
                                        break;
                                    case "i":
                                        cf.dwMask |= CFM.ITALIC;
                                        cf.dwEffects |= CFE.ITALIC;
                                        break;
                                    case "u":
                                        cf.dwMask |= CFM.UNDERLINE | CFM.UNDERLINETYPE;
                                        cf.dwEffects |= CFE.UNDERLINE;
                                        cf.bUnderlineType = CFU.UNDERLINE;
                                        break;
                                    case "s":
                                        cf.dwMask |= CFM.STRIKEOUT;
                                        cf.dwEffects |= CFE.STRIKEOUT;
                                        break;
                                    case "sup":
                                        cf.dwMask |= CFM.SUPERSCRIPT;
                                        cf.dwEffects |= CFE.SUPERSCRIPT;
                                        break;
                                    case "sub":
                                        cf.dwMask |= CFM.SUBSCRIPT;
                                        cf.dwEffects |= CFE.SUBSCRIPT;
                                        break;
                                    case "a":
                                        hyperlinkStart = rtb.TextLength;
                                        hyperlink = null;
                                        while (reader.MoveToNextAttribute())
                                        {
                                            switch (reader.Name.ToLower())
                                            {
                                                case "href":
                                                    hyperlink = reader.Value;
                                                    break;
                                            }
                                        }
                                        reader.MoveToElement();
                                        break;
                                    case "p":
                                        spf.Push(pf);
                                        while (reader.MoveToNextAttribute())
                                        {
                                            switch (reader.Name.ToLower())
                                            {
                                                case "align":
                                                    if (reader.Value == "left")
                                                    {
                                                        pf.dwMask |= PFM.ALIGNMENT;
                                                        pf.wAlignment = PFA.LEFT;
                                                    }
                                                    else if (reader.Value == "right")
                                                    {
                                                        pf.dwMask |= PFM.ALIGNMENT;
                                                        pf.wAlignment = PFA.RIGHT;
                                                    }
                                                    else if (reader.Value == "center")
                                                    {
                                                        pf.dwMask |= PFM.ALIGNMENT;
                                                        pf.wAlignment = PFA.CENTER;
                                                    }
                                                    break;
                                            }
                                        }
                                        reader.MoveToElement();
                                        break;
                                    case "li":
                                        spf.Push(pf);
                                        if (pf.wNumbering != PFN.BULLET)
                                        {
                                            pf.dwMask |= PFM.NUMBERING;
                                            pf.wNumbering = PFN.BULLET;
                                        }
                                        break;
                                    case "font":
                                        scf.Push(cf);
                                        string strFont = cf.szFaceName;
                                        int crFont = cf.crTextColor;
                                        int yHeight = cf.yHeight;

                                        while (reader.MoveToNextAttribute())
                                        {
                                            switch (reader.Name.ToLower())
                                            {
                                                case "face":
                                                    cf.dwMask |= CFM.FACE;
                                                    strFont = reader.Value;
                                                    break;
                                                case "size":
                                                    cf.dwMask |= CFM.SIZE;
                                                    yHeight = int.Parse(reader.Value);
                                                    yHeight *= (20 * 5);
                                                    break;
                                                case "color":
                                                    cf.dwMask |= CFM.COLOR;
                                                    string text = reader.Value;
                                                    if (text.StartsWith("#"))
                                                    {
                                                        string strCr = text.Substring(1);
                                                        int nCr = Convert.ToInt32(strCr, 16);
                                                        Color color = Color.FromArgb(nCr);
                                                        crFont = GetCOLORREF(color);
                                                    }
                                                    else if (!int.TryParse(text, out crFont))
                                                    {
                                                        Color color = Color.FromName(text);
                                                        crFont = GetCOLORREF(color);
                                                    }
                                                    break;
                                            }
                                        }
                                        reader.MoveToElement();

                                        cf.szFaceName = strFont;
                                        cf.crTextColor = crFont;
                                        cf.yHeight = yHeight;

                                        cf.dwEffects &= ~CFE.AUTOCOLOR;
                                        break;
                                }
                                break;
                            case XmlNodeType.EndElement:
                                switch (reader.Name)
                                {
                                    case "b":
                                        cf.dwEffects &= ~CFE.BOLD;
                                        cf.wWeight = FW.NORMAL;
                                        break;
                                    case "i":
                                        cf.dwEffects &= ~CFE.ITALIC;
                                        break;
                                    case "u":
                                        cf.dwEffects &= ~CFE.UNDERLINE;
                                        break;
                                    case "s":
                                        cf.dwEffects &= ~CFE.STRIKEOUT;
                                        break;
                                    case "sup":
                                        cf.dwEffects &= ~CFE.SUPERSCRIPT;
                                        break;
                                    case "sub":
                                        cf.dwEffects &= ~CFE.SUBSCRIPT;
                                        break;
                                    case "a":
                                        int length = rtb.TextLength - hyperlinkStart;

                                        if (hyperlink != null)
                                        {
                                            rtb.Select(hyperlinkStart, length);
                                            if (hyperlink != rtb.SelectedText)
                                            {
                                                string rtfText = rtb.SelectedRtf;
                                                int idx = rtfText.LastIndexOf('}');
                                                if (idx != -1)
                                                {
                                                    string head = rtfText.Substring(0, idx);
                                                    string tail = rtfText.Substring(idx);
                                                    rtb.SelectedRtf = head + @"\v #" + hyperlink + @"\v0" + tail;
                                                    length = rtb.TextLength - hyperlinkStart;
                                                }
                                            }
                                            // reposition to final
                                            rtb.Select(rtb.TextLength + 1, 0);
                                        }
                                        links.Add(new KeyValuePair<int, int>(hyperlinkStart, length));

                                        hyperlinkStart = -1;
                                        break;
                                    case "p":
                                        pf = spf.Pop();
                                        break;
                                    case "li":
                                        pf = spf.Pop();
                                        break;
                                    case "font":
                                        cf = scf.Pop();
                                        break;
                                }
                                break;
                            case XmlNodeType.Text:
                            case XmlNodeType.Whitespace:
                            case XmlNodeType.SignificantWhitespace:
                                string strData = reader.Value;
                                bool bNewParagraph = (strData.IndexOf("\r\n", 0) >= 0) || (strData.IndexOf("\n", 0) >= 0);

                                if (strData.Length > 0)
                                {
                                    // now, add text to control
                                    int nStartCache = rtb.SelectionStart;

                                    rtb.SelectedText = strData;

                                    rtb.Select(nStartCache, strData.Length);

                                    // apply format
                                    rtb.SetParaFormat(pf);
                                    rtb.SetCharFormat(cf);
                                }

                                // reposition to final
                                rtb.Select(rtb.TextLength + 1, 0);

                                // new paragraph requires to reset alignment
                                if (bNewParagraph)
                                {
                                    pf.dwMask = PFM.ALIGNMENT | PFM.NUMBERING;
                                    pf.wAlignment = PFA.LEFT;
                                    pf.wNumbering = 0;
                                }
                                break;
                            case XmlNodeType.XmlDeclaration:
                            case XmlNodeType.ProcessingInstruction:
                                break;
                            case XmlNodeType.Comment:
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (System.Xml.XmlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            rtb.HideSelection = false;
            // apply links style
            CHARFORMAT ncf = new CHARFORMAT(CFM.LINK, CFE.LINK);
            foreach (var pair in links)
            {
                rtb.Select(pair.Key, pair.Value);
                rtb.SetCharFormat(ncf);
            }
            // reposition to final
            rtb.Select(rtb.TextLength + 1, 0);
            rtb.EndUpdate(oldMask);
        }
    }
}
