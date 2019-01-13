using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;

#pragma warning disable SA1305 // Field names should not use Hungarian notation

namespace GitUI.Editor.RichTextBoxExtension
{
    internal static class RichTextBoxXhtmlSupportExtension
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
        private static IntPtr BeginUpdate(HandleRef handleRef)
        {
            // Prevent the control from raising any events.
            IntPtr oldEventMask = NativeMethods.SendMessage(handleRef,
                NativeMethods.EM_SETEVENTMASK, IntPtr.Zero, IntPtr.Zero);

            // Prevent the control from redrawing itself.
            NativeMethods.SendMessage(handleRef,
                NativeMethods.WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);

            return oldEventMask;
        }

        public static IntPtr BeginUpdate(this RichTextBox rtb)
        {
            var handleRef = new HandleRef(rtb, rtb.Handle);
            return BeginUpdate(handleRef);
        }

        /// <summary>
        /// Resumes drawing and event handling.
        /// </summary>
        /// <remarks>
        /// This method should be called every time a call is made
        /// made to BeginUpdate. It resets the event mask to it's
        /// original value and enables redrawing of the control.
        /// </remarks>
        private static void EndUpdate(HandleRef handleRef, IntPtr oldEventMask)
        {
            // Allow the control to redraw itself.
            NativeMethods.SendMessage(handleRef,
                NativeMethods.WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);

            // Allow the control to raise event messages.
            NativeMethods.SendMessage(handleRef,
                NativeMethods.EM_SETEVENTMASK, IntPtr.Zero, oldEventMask);
        }

        public static void EndUpdate(this RichTextBox rtb, IntPtr oldEventMask)
        {
            var handleRef = new HandleRef(rtb, rtb.Handle);
            EndUpdate(handleRef, oldEventMask);
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

            SMALLCAPS = 0x0040,         /* (*)  */
            ALLCAPS = 0x0080,           /* Displayed by 3.0 */
            HIDDEN = 0x0100,            /* Hidden by 3.0 */
            OUTLINE = 0x0200,           /* (*)  */
            SHADOW = 0x0400,            /* (*)  */
            EMBOSS = 0x0800,            /* (*)  */
            IMPRINT = 0x1000,           /* (*)  */
            DISABLED = 0x2000,
            REVISED = 0x4000,

            BACKCOLOR = 0x04000000,
            LCID = 0x02000000,
            UNDERLINETYPE = 0x00800000,     /* Many displayed by 3.0 */
            WEIGHT = 0x00400000,
            SPACING = 0x00200000,       /* Displayed by 3.0 */
            KERNING = 0x00100000,       /* (*)  */
            STYLE = 0x00080000,     /* (*)  */
            ANIMATION = 0x00040000,     /* (*)  */
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
            SUBSCRIPT = 0x00010000,     /* Superscript and subscript are */
            SUPERSCRIPT = 0x00020000,     /*  mutually exclusive             */

            SMALLCAPS = 0x0040,         /* (*)  */
            ALLCAPS = 0x0080,           /* Displayed by 3.0 */
            HIDDEN = 0x0100,            /* Hidden by 3.0 */
            OUTLINE = 0x0200,           /* (*)  */
            SHADOW = 0x0400,            /* (*)  */
            EMBOSS = 0x0800,            /* (*)  */
            IMPRINT = 0x1000,           /* (*)  */
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
            UNDERLINEWORD = 0x02, /* (*) displayed as ordinary underline    */
            UNDERLINEDOUBLE = 0x03, /* (*) displayed as ordinary underline  */
            UNDERLINEDOTTED = 0x04,
            UNDERLINEDASH = 0x05,
            UNDERLINEDASHDOT = 0x06,
            UNDERLINEDASHDOTDOT = 0x07,
            UNDERLINEWAVE = 0x08,
            UNDERLINETHICK = 0x09,
            UNDERLINEHAIRLINE = 0x0A /* (*) displayed as ordinary underline */
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
            BORDER = 0x00000800,    // (*)
            SHADING = 0x00001000,   // (*)
            NUMBERINGSTYLE = 0x00002000,    // RE 3.0
            NUMBERINGTAB = 0x00004000,  // RE 3.0
            NUMBERINGSTART = 0x00008000,    // RE 3.0

            RTLPARA = 0x00010000,
            KEEP = 0x00020000,  // (*)
            KEEPNEXT = 0x00040000,  // (*)
            PAGEBREAKBEFORE = 0x00080000,   // (*)
            NOLINENUMBER = 0x00100000,  // (*)
            NOWIDOWCONTROL = 0x00200000,    // (*)
            DONOTHYPHEN = 0x00400000,   // (*)
            SIDEBYSIDE = 0x00800000,    // (*)
            TABLE = 0x40000000, // RE 3.0
            TEXTWRAPPINGBREAK = 0x20000000, // RE 3.0
            TABLEROWDELIMITER = 0x10000000, // RE 4.0

            // The following three properties are read only
            COLLAPSED = 0x01000000, // RE 3.0
            OUTLINELEVEL = 0x02000000,  // RE 3.0
            BOX = 0x04000000,   // RE 3.0
            RESERVED2 = 0x08000000 // RE 4.0
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
                cbSize = Marshal.SizeOf(this);
                dwMask = mask;
                dwEffects = effects;
                szFaceName = "";
            }

            public int cbSize;
            public CFM dwMask;
            public CFE dwEffects;
            public int yHeight;
            public int yOffset;
            public int crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szFaceName;

            // CHARFORMAT2 from here onwards.
            public FW wWeight;
            public short sSpacing;
            public int crBackColor;
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
        internal static class NativeMethods
        {
            // Constants from the Platform SDK.
            internal const int WM_USER = 0x0400;
            internal const int EM_GETCHARFORMAT = WM_USER + 58;
            internal const int EM_SETCHARFORMAT = WM_USER + 68;
            internal const int EM_HIDESELECTION = WM_USER + 63;
            internal const int EM_GETSCROLLPOS = WM_USER + 221;
            internal const int EM_SETSCROLLPOS = WM_USER + 222;

            internal const int EM_SETEVENTMASK = 1073;
            internal const int EM_GETPARAFORMAT = 1085;
            internal const int EM_SETPARAFORMAT = 1095;
            internal const int WM_SETREDRAW = 11;

            // Defines for EM_SETCHARFORMAT/EM_GETCHARFORMAT
            internal const int SCF_SELECTION = 0x0001;
            internal const int SCF_WORD = 0x0002;
            internal const int SCF_ALL = 0x0004;

            internal const int LF_FACESIZE = 32;

            [DllImport("user32", CharSet = CharSet.Auto)]
            internal static extern IntPtr SendMessage(HandleRef hWnd,
                int msg,
                IntPtr wParam,
                IntPtr lParam);

            [DllImport("user32", CharSet = CharSet.Auto)]
            internal static extern IntPtr SendMessage(HandleRef hWnd,
                int msg,
                IntPtr wParam,
                ref Point lParam);

            [DllImport("user32", CharSet = CharSet.Auto)]
            internal static extern IntPtr SendMessage(HandleRef hWnd,
                int msg,
                IntPtr wParam,
                ref PARAFORMAT lp);

            [DllImport("user32", CharSet = CharSet.Auto)]
            internal static extern IntPtr SendMessage(HandleRef hWnd,
                int msg,
                IntPtr wParam,
                ref CHARFORMAT lp);
        }
        #endregion

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
            return (cf.dwEffects & CFE.SUPERSCRIPT) == CFE.SUPERSCRIPT;
        }

        public static bool IsSubScript(this RichTextBox rtb)
        {
            CHARFORMAT cf = rtb.GetCharFormat();
            return (cf.dwEffects & CFE.SUBSCRIPT) == CFE.SUBSCRIPT;
        }

        public static bool IsLink(this RichTextBox rtb)
        {
            CHARFORMAT cf = rtb.GetCharFormat();
            return (cf.dwEffects & CFE.LINK) == CFE.LINK;
        }

        private static void RtbSetSelectedRtf(RichTextBox rtb, string str)
        {
            // Work around bug in DotNet.
            // Basically it assumes that incoming text is always in the default
            // encoding so giving it a unicode string throws an exception.
            // (Bug #5005)
            try
            {
                rtb.SelectedRtf = str;
            }
            catch (ArgumentException)
            {
                // NOTE: This will break any text which actually contains UTF-8 characters!
                if (str.StartsWith(@"{\urtf"))
                {
                    // Convert "urtf" -> "rtf"
                    str = str.Remove(2, 1);

                    // Encode unicode characters
                    var sb = new StringBuilder();
                    foreach (var c in str)
                    {
                        if (c < 0x7f)
                        {
                            sb.Append(c);
                        }
                        else
                        {
                            sb.Append(@"\u" + Convert.ToUInt32(c) + "?");
                        }
                    }

                    rtb.SelectedRtf = sb.ToString();
                }
            }
        }

        private static PARAFORMAT GetParaFormat(HandleRef handleRef)
        {
            var pf = new PARAFORMAT();
            pf.cbSize = Marshal.SizeOf(pf);

            // Get the alignment.
            NativeMethods.SendMessage(handleRef,
                NativeMethods.EM_GETPARAFORMAT,
                (IntPtr)NativeMethods.SCF_SELECTION, ref pf);

            return pf;
        }

        public static PARAFORMAT GetParaFormat(this RichTextBox rtb)
        {
            var handleRef = new HandleRef(rtb, rtb.Handle);
            return GetParaFormat(handleRef);
        }

        private static void SetParaFormat(HandleRef handleRef, PARAFORMAT value)
        {
            Debug.Assert(value.cbSize == Marshal.SizeOf(value), "value.cbSize == Marshal.SizeOf(value)");

            // Set the alignment.
            NativeMethods.SendMessage(handleRef,
                NativeMethods.EM_SETPARAFORMAT,
                (IntPtr)NativeMethods.SCF_SELECTION, ref value);
        }

        public static void SetParaFormat(this RichTextBox rtb, PARAFORMAT value)
        {
            var handleRef = new HandleRef(rtb, rtb.Handle);
            SetParaFormat(handleRef, value);
        }

        private static PARAFORMAT GetDefaultParaFormat(HandleRef handleRef)
        {
            var pf = new PARAFORMAT();
            pf.cbSize = Marshal.SizeOf(pf);

            // Get the alignment.
            NativeMethods.SendMessage(handleRef,
                NativeMethods.EM_GETPARAFORMAT,
                (IntPtr)NativeMethods.SCF_ALL, ref pf);

            return pf;
        }

        public static PARAFORMAT GetDefaultParaFormat(this RichTextBox rtb)
        {
            var handleRef = new HandleRef(rtb, rtb.Handle);
            return GetDefaultParaFormat(handleRef);
        }

        private static void SetDefaultParaFormat(HandleRef handleRef, PARAFORMAT value)
        {
            Debug.Assert(value.cbSize == Marshal.SizeOf(value), "value.cbSize == Marshal.SizeOf(value)");

            // Set the alignment.
            NativeMethods.SendMessage(handleRef,
                NativeMethods.EM_SETPARAFORMAT,
                (IntPtr)NativeMethods.SCF_ALL, ref value);
        }

        public static void SetDefaultParaFormat(this RichTextBox rtb, PARAFORMAT value)
        {
            var handleRef = new HandleRef(rtb, rtb.Handle);
            SetDefaultParaFormat(handleRef, value);
        }

        private static CHARFORMAT GetCharFormat(HandleRef handleRef)
        {
            var cf = new CHARFORMAT();
            cf.cbSize = Marshal.SizeOf(cf);

            // Get the alignment.
            NativeMethods.SendMessage(handleRef,
                NativeMethods.EM_GETCHARFORMAT,
                (IntPtr)NativeMethods.SCF_SELECTION, ref cf);

            return cf;
        }

        public static CHARFORMAT GetCharFormat(this RichTextBox rtb)
        {
            var handleRef = new HandleRef(rtb, rtb.Handle);
            return GetCharFormat(handleRef);
        }

        private static void SetCharFormat(HandleRef handleRef, CHARFORMAT value)
        {
            Debug.Assert(value.cbSize == Marshal.SizeOf(value), "value.cbSize == Marshal.SizeOf(value)");

            // Set the alignment.
            NativeMethods.SendMessage(handleRef,
                NativeMethods.EM_SETCHARFORMAT,
                (IntPtr)NativeMethods.SCF_SELECTION, ref value);
        }

        public static void SetCharFormat(this RichTextBox rtb, CHARFORMAT value)
        {
            var handleRef = new HandleRef(rtb, rtb.Handle);
            SetCharFormat(handleRef, value);
        }

        public static void SetCharFormat(this RichTextBox rtb, CFM mask, CFE effects)
        {
            var cf = new CHARFORMAT(mask, effects);
            rtb.SetCharFormat(cf);
        }

        private static CHARFORMAT GetDefaultCharFormat(HandleRef handleRef)
        {
            var cf = new CHARFORMAT();
            cf.cbSize = Marshal.SizeOf(cf);

            // Get the alignment.
            NativeMethods.SendMessage(handleRef,
                NativeMethods.EM_GETCHARFORMAT,
                (IntPtr)NativeMethods.SCF_ALL, ref cf);

            return cf;
        }

        public static CHARFORMAT GetDefaultCharFormat(this RichTextBox rtb)
        {
            var handleRef = new HandleRef(rtb, rtb.Handle);
            return GetDefaultCharFormat(handleRef);
        }

        private static void SetDefaultCharFormat(HandleRef handleRef, CHARFORMAT value)
        {
            Debug.Assert(value.cbSize == Marshal.SizeOf(value), "value.cbSize == Marshal.SizeOf(value)");

            // Set the alignment.
            NativeMethods.SendMessage(handleRef,
                NativeMethods.EM_SETCHARFORMAT,
                (IntPtr)NativeMethods.SCF_ALL, ref value);
        }

        public static void SetDefaultCharFormat(this RichTextBox rtb, CHARFORMAT value)
        {
            var handleRef = new HandleRef(rtb, rtb.Handle);
            SetDefaultCharFormat(handleRef, value);
        }

        public static void SetDefaultCharFormat(this RichTextBox rtb, CFM mask, CFE effects)
        {
            var cf = new CHARFORMAT(mask, effects);
            rtb.SetDefaultCharFormat(cf);
        }

        private static Point GetScrollPoint(HandleRef handleRef)
        {
            var scrollPoint = new Point();
            NativeMethods.SendMessage(handleRef, NativeMethods.EM_GETSCROLLPOS, IntPtr.Zero, ref scrollPoint);
            return scrollPoint;
        }

        public static Point GetScrollPoint(this RichTextBox rtb)
        {
            var handleRef = new HandleRef(rtb, rtb.Handle);
            return GetScrollPoint(handleRef);
        }

        private static void SetScrollPoint(HandleRef handleRef, Point scrollPoint)
        {
            NativeMethods.SendMessage(handleRef, NativeMethods.EM_SETSCROLLPOS, IntPtr.Zero, ref scrollPoint);
        }

        public static void SetScrollPoint(this RichTextBox rtb, Point scrollPoint)
        {
            var handleRef = new HandleRef(rtb, rtb.Handle);
            SetScrollPoint(handleRef, scrollPoint);
        }

#region COLORREF helper functions

        // convert COLORREF to Color
        private static Color GetColor(int crColor)
        {
            var r = (byte)crColor;
            var g = (byte)(crColor >> 8);
            var b = (byte)(crColor >> 16);

            return Color.FromArgb(r, g, b);
        }

        // convert COLORREF to Color
        private static int GetCOLORREF(int r, int g, int b)
        {
            int r2 = r;
            int g2 = g << 8;
            int b2 = b << 16;

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
            var v = e.LinkText.Split(new[] { '#' }, 2);
            if (v.Length == 0)
            {
                return "";
            }
            else if (v.Length == 1)
            {
                return v[0];
            }
            else
            {
                return v[1];
            }
        }

        public static void GetLinkText(this LinkClickedEventArgs e, out string url, out string text)
        {
            var v = e.LinkText.Split(new[] { '#' }, 2);
            if (v.Length == 0)
            {
                url = "";
                text = "";
                return;
            }

            text = v[0];

            url = v.Length == 1 ? v[0] : v[1];
        }

        // format states
        private enum ctformatStates
        {
            nctNone = 0, // none format applied
            nctNew = 1, // new format
            nctContinue = 2, // continue with previous format
            nctReset = 3 // reset format (close this tag)
        }

        public static string GetXHTMLText(this RichTextBox rtb, bool bParaFormat)
        {
            var strHTML = new StringBuilder();

            rtb.HideSelection = true;
            IntPtr oldMask = rtb.BeginUpdate();

            int nStart = rtb.SelectionStart;
            int nEnd = rtb.SelectionLength;

            try
            {
                // to store formatting
                List<KeyValuePair<int, string>> colFormat = new List<KeyValuePair<int, string>>();
                string strT = ProcessTags(rtb, colFormat, bParaFormat);

                // apply format by replacing and inserting HTML tags
                // stored in the Format Array
                int nAcum = 0;
                for (int i = 0; i < colFormat.Count; i++)
                {
                    var (pos, markup) = colFormat[i];
                    strHTML.Append(WebUtility.HtmlEncode(strT.Substring(nAcum, pos - nAcum)) + markup);
                    nAcum = pos;
                }

                if (nAcum < strT.Length)
                {
                    strHTML.Append(strT.Substring(nAcum));
                }
            }
            catch (Exception /*ex*/)
            {
                ////MessageBox.Show(ex.Message);
            }
            finally
            {
                // finish, restore
                rtb.SelectionStart = nStart;
                rtb.SelectionLength = nEnd;

                rtb.EndUpdate(oldMask);
                rtb.HideSelection = false;
            }

            return strHTML.ToString();
        }

        private static string ProcessTags(RichTextBox rtb, List<KeyValuePair<int, string>> colFormat, bool bParaFormat)
        {
            var sbT = new StringBuilder();

            ctformatStates bold = ctformatStates.nctNone;
            ctformatStates bitalic = ctformatStates.nctNone;
            ctformatStates bstrikeout = ctformatStates.nctNone;
            ctformatStates bunderline = ctformatStates.nctNone;
            ctformatStates super = ctformatStates.nctNone;
            ctformatStates sub = ctformatStates.nctNone;

            ctformatStates bacenter = ctformatStates.nctNone;
            ctformatStates baleft = ctformatStates.nctNone;
            ctformatStates baright = ctformatStates.nctNone;
            ctformatStates bnumbering = ctformatStates.nctNone;
            bool fontSet = false;
            string strFont = "";
            int crFont = 0;
            int yHeight = 0;

            int i;
            int pos = 0;
            int k = rtb.TextLength;
            char[] chtrim = { ' ', '\x0000' };

            //--------------------------------
            // this is an inefficient method to get text format
            // but RichTextBox doesn't provide another method to
            // get something like an array of charformat and paraformat
            //--------------------------------
            for (i = 0; i < k; i++)
            {
                // select one character
                rtb.Select(i, 1);
                string strChar = rtb.SelectedText;

                // get format for this character
                CHARFORMAT cf = rtb.GetCharFormat();
                PARAFORMAT pf = rtb.GetParaFormat();

                string strfname = cf.szFaceName;
                strfname = strfname.Trim(chtrim);

                // new font format ?
                if ((strFont != strfname) || (crFont != cf.crTextColor) || (yHeight != cf.yHeight))
                {
                    KeyValuePair<int, string> mfr;
                    if (strFont != "")
                    {
                        // close previous <font> tag
                        mfr = new KeyValuePair<int, string>(pos, "</font>");
                        colFormat.Add(mfr);
                    }

                    // save this for cache
                    strFont = strfname;
                    crFont = cf.crTextColor;
                    yHeight = cf.yHeight;

                    fontSet = strFont != "";

                    // font size should be translate to
                    // html size (Approximately)
                    int fsize = yHeight / (20 * 5);

                    // color object from COLORREF
                    var color = GetColor(crFont);

                    // add <font> tag
                    string strcolor = string.Concat("#", (color.ToArgb() & 0x00FFFFFF).ToString("X6"));

                    mfr = new KeyValuePair<int, string>(pos, "<font face=\"" + strFont + "\" color=\"" + strcolor + "\" size=\"" + fsize + "\">");
                    colFormat.Add(mfr);
                }

                // are we in another line ?
                if ((strChar == "\r") || (strChar == "\n"))
                {
                    // yes?
                    // then, we need to reset paragraph format
                    // and character format
                    if (bParaFormat)
                    {
                        bnumbering = ctformatStates.nctNone;
                        baleft = ctformatStates.nctNone;
                        baright = ctformatStates.nctNone;
                        bacenter = ctformatStates.nctNone;
                    }

                    // close previous tags

                    // is italic? => close it
                    if (bitalic != ctformatStates.nctNone)
                    {
                        var mfr = new KeyValuePair<int, string>(pos, "</i>");
                        colFormat.Add(mfr);
                        bitalic = ctformatStates.nctNone;
                    }

                    // is bold? => close it
                    if (bold != ctformatStates.nctNone)
                    {
                        var mfr = new KeyValuePair<int, string>(pos, "</b>");
                        colFormat.Add(mfr);
                        bold = ctformatStates.nctNone;
                    }

                    // is underline? => close it
                    if (bunderline != ctformatStates.nctNone)
                    {
                        var mfr = new KeyValuePair<int, string>(pos, "</u>");
                        colFormat.Add(mfr);
                        bunderline = ctformatStates.nctNone;
                    }

                    // is strikeout? => close it
                    if (bstrikeout != ctformatStates.nctNone)
                    {
                        var mfr = new KeyValuePair<int, string>(pos, "</s>");
                        colFormat.Add(mfr);
                        bstrikeout = ctformatStates.nctNone;
                    }

                    // is super? => close it
                    if (super != ctformatStates.nctNone)
                    {
                        var mfr = new KeyValuePair<int, string>(pos, "</sup>");
                        colFormat.Add(mfr);
                        super = ctformatStates.nctNone;
                    }

                    // is sub? => close it
                    if (sub != ctformatStates.nctNone)
                    {
                        var mfr = new KeyValuePair<int, string>(pos, "</sub>");
                        colFormat.Add(mfr);
                        sub = ctformatStates.nctNone;
                    }
                }

                // now, process the paragraph format,
                // managing states: none, new, continue {with previous}, reset
                if (bParaFormat)
                {
                    // align to center?
                    UpdateState(pf.wAlignment == PFA.CENTER, ref bacenter);

                    if (bacenter == ctformatStates.nctNew)
                    {
                        var mfr = new KeyValuePair<int, string>(pos, "<p align=\"center\">");
                        colFormat.Add(mfr);
                    }
                    else if (bacenter == ctformatStates.nctReset)
                    {
                        bacenter = ctformatStates.nctNone;
                    }

                    // align to left
                    UpdateState(pf.wAlignment == PFA.LEFT, ref baleft);

                    if (baleft == ctformatStates.nctNew)
                    {
                        var mfr = new KeyValuePair<int, string>(pos, "<p align=\"left\">");
                        colFormat.Add(mfr);
                    }
                    else if (baleft == ctformatStates.nctReset)
                    {
                        baleft = ctformatStates.nctNone;
                    }

                    // align to right
                    UpdateState(pf.wAlignment == PFA.RIGHT, ref baright);

                    if (baright == ctformatStates.nctNew)
                    {
                        var mfr = new KeyValuePair<int, string>(pos, "<p align=\"right\">");
                        colFormat.Add(mfr);
                    }
                    else if (baright == ctformatStates.nctReset)
                    {
                        baright = ctformatStates.nctNone;
                    }

                    // bullet
                    UpdateState(pf.wNumbering == PFN.BULLET, ref bnumbering);

                    if (bnumbering == ctformatStates.nctNew)
                    {
                        var mfr = new KeyValuePair<int, string>(pos, "<li>");
                        colFormat.Add(mfr);
                    }
                    else if (bnumbering == ctformatStates.nctReset)
                    {
                        bnumbering = ctformatStates.nctNone;
                    }
                }

                // bold
                UpdateState((cf.dwEffects & CFE.BOLD) == CFE.BOLD, ref bold);
                AddTag(pos, "b", colFormat, ref bold);

                // Italic
                UpdateState((cf.dwEffects & CFE.ITALIC) == CFE.ITALIC, ref bitalic);
                AddTag(pos, "i", colFormat, ref bitalic);

                // strikeout
                UpdateState((cf.dwEffects & CFE.STRIKEOUT) == CFE.STRIKEOUT, ref bstrikeout);
                AddTag(pos, "s", colFormat, ref bstrikeout);

                // underline
                UpdateState((cf.dwEffects & CFE.UNDERLINE) == CFE.UNDERLINE, ref bunderline);
                AddTag(pos, "u", colFormat, ref bunderline);

                // superscript
                UpdateState((cf.dwEffects & CFE.SUPERSCRIPT) == CFE.SUPERSCRIPT, ref super);
                AddTag(pos, "sup", colFormat, ref super);

                // subscript
                UpdateState((cf.dwEffects & CFE.SUBSCRIPT) == CFE.SUBSCRIPT, ref sub);
                AddTag(pos, "sub", colFormat, ref sub);

                sbT.Append(strChar);
                pos = sbT.Length;
            }

            // close pending tags
            if (bold != ctformatStates.nctNone)
            {
                var mfr = new KeyValuePair<int, string>(pos, "</b>");
                colFormat.Add(mfr);
            }

            if (bitalic != ctformatStates.nctNone)
            {
                var mfr = new KeyValuePair<int, string>(pos, "</i>");
                colFormat.Add(mfr);
            }

            if (bstrikeout != ctformatStates.nctNone)
            {
                var mfr = new KeyValuePair<int, string>(pos, "</s>");
                colFormat.Add(mfr);
            }

            if (bunderline != ctformatStates.nctNone)
            {
                var mfr = new KeyValuePair<int, string>(pos, "</u>");
                colFormat.Add(mfr);
            }

            if (super != ctformatStates.nctNone)
            {
                var mfr = new KeyValuePair<int, string>(pos, "</sup>");
                colFormat.Add(mfr);
            }

            if (sub != ctformatStates.nctNone)
            {
                var mfr = new KeyValuePair<int, string>(pos, "</sub>");
                colFormat.Add(mfr);
            }

            if (fontSet)
            {
                // close pending font format
                var mfr = new KeyValuePair<int, string>(pos, "</font>");
                colFormat.Add(mfr);
            }

            // now, reorder the formatting array
            k = colFormat.Count;
            for (i = 0; i < k - 1; i++)
            {
                for (int j = i + 1; j < k; j++)
                {
                    var mfr = colFormat[i];
                    var mfr2 = colFormat[j];

                    if (mfr2.Key < mfr.Key)
                    {
                        colFormat.RemoveAt(j);
                        colFormat.Insert(i, mfr2);
                        j--;
                    }
                }
            }

            return sbT.ToString();
        }

        private static void UpdateState(bool value, ref ctformatStates state)
        {
            if (value)
            {
                state = state == ctformatStates.nctNone
                    ? ctformatStates.nctNew
                    : ctformatStates.nctContinue;
            }
            else
            {
                if (state != ctformatStates.nctNone)
                {
                    state = ctformatStates.nctReset;
                }
            }
        }

        private static void AddTag(int pos, string tag, List<KeyValuePair<int, string>> colFormat, ref ctformatStates state)
        {
            if (state == ctformatStates.nctNew)
            {
                var mfr = new KeyValuePair<int, string>(pos, "<" + tag + ">");
                colFormat.Add(mfr);
            }
            else if (state == ctformatStates.nctReset)
            {
                var mfr = new KeyValuePair<int, string>(pos, "</" + tag + ">");
                colFormat.Add(mfr);
                state = ctformatStates.nctNone;
            }
        }

        public static string GetPlainText(this RichTextBox rtb)
        {
            return GetPlainText(rtb, 0, rtb.TextLength);
        }

        public static string GetSelectionPlainText(this RichTextBox rtb)
        {
            return GetPlainText(rtb, rtb.SelectionStart, rtb.SelectionStart + rtb.SelectionLength);
        }

        public static string GetPlainText(this RichTextBox rtb, int from, int to)
        {
            if (to == 0)
            {
                return string.Empty;
            }

            IntPtr oldMask = rtb.BeginUpdate();

            int nStart = rtb.SelectionStart;
            int nEnd = rtb.SelectionLength;
            var text = new StringBuilder();

            try
            {
                //--------------------------------
                // this is an inefficient method to get text format
                // but RichTextBox doesn't provide another method to
                // get something like an array of charformat and paraformat
                //--------------------------------
                for (int i = from; i < to; i++)
                {
                    // select one character
                    rtb.Select(i, 1);
                    text.Append(rtb.SelectedText);
                }
            }
            catch (Exception /*ex*/)
            {
                ////MessageBox.Show(ex.Message);
            }
            finally
            {
                // finish, restore
                rtb.SelectionStart = nStart;
                rtb.SelectionLength = nEnd;

                rtb.EndUpdate(oldMask);
                rtb.Invalidate();
            }

            return text.ToString();
        }

        public static string GetLink(this RichTextBox rtb, int charIndex)
        {
            var text = rtb.Text;
            if (charIndex < 0 || text.Length <= charIndex)
            {
                return null;
            }

            IntPtr oldMask = rtb.BeginUpdate();

            int nStart = rtb.SelectionStart;
            int nEnd = rtb.SelectionLength;

            try
            {
                //--------------------------------
                // this is an inefficient method to get text format
                // but RichTextBox doesn't provide another method to
                // get something like an array of charformat and paraformat
                //--------------------------------
                rtb.Select(charIndex, 1);
                if (!rtb.IsLink())
                {
                    return null;
                }

                int from = charIndex;
                while (from > 0)
                {
                    rtb.Select(from - 1, 1);
                    if (!rtb.IsLink())
                    {
                        break;
                    }

                    --from;
                }

                int to = charIndex + 1;
                while (to < text.Length)
                {
                    rtb.Select(to, 1);
                    if (!rtb.IsLink())
                    {
                        break;
                    }

                    ++to;
                }

                if (to < text.Length && text[to] == '#')
                {
                    ++to;
                    from = to;
                    while (to < text.Length && !char.IsWhiteSpace(text[to]) && text[to] != ',')
                    {
                        ++to;
                    }
                }

                return text.Substring(from, to - from);
            }
            catch
            {
                return null;
            }
            finally
            {
                // finish, restore
                rtb.SelectionStart = nStart;
                rtb.SelectionLength = nEnd;

                rtb.EndUpdate(oldMask);
                rtb.Invalidate();
            }
        }

        /// <summary>
        /// Returns input text with characters disallowed by XML spec (e.g. most control codes in 0x00-0x20 range)
        /// replaced with equivalent character references or question marks if there is an unrecoverable error.
        /// Although they are disallowed even when escaped, this step seems necessary to make them acceptable
        /// by XmlReader with CheckCharacters disabled.
        /// </summary>
        private static string EscapeNonXMLChars(string input)
        {
            var result = new StringBuilder();
            foreach (char ch in input)
            {
                if (XmlConvert.IsXmlChar(ch))
                {
                    result.Append(ch);
                }
                else
                {
                    try
                    {
                        result.Append("&#" + (int)ch + ';');
                    }
                    catch (ArgumentException)
                    {
                        result.Append('?');
                    }
                }
            }

            return result.ToString();
        }

        private class RTFCurrentState
        {
            public RTFCurrentState()
            {
                scf = new Stack<CHARFORMAT>();
                spf = new Stack<PARAFORMAT>();
                links = new List<KeyValuePair<int, int>>();
                hyperlink = null;
                hyperlinkStart = -1;
                charFormatChanged = false;
                paraFormatChanged = false;
            }

            public readonly List<KeyValuePair<int, int>> links;
            public readonly Stack<CHARFORMAT> scf;
            public readonly Stack<PARAFORMAT> spf;
            public CHARFORMAT cf;
            public PARAFORMAT pf;
            public bool charFormatChanged;
            public bool paraFormatChanged;
            public string hyperlink;
            public int hyperlinkStart;
        }

        public static void SetXHTMLText(this RichTextBox rtb, string xhtmlText)
        {
            rtb.Clear();
            var cs = new RTFCurrentState();

            var handleRef = new HandleRef(rtb, rtb.Handle);
            cs.cf = GetDefaultCharFormat(handleRef); // to apply character formatting
            cs.pf = GetDefaultParaFormat(handleRef); // to apply paragraph formatting

            IntPtr oldMask = BeginUpdate(handleRef);

            var settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                CheckCharacters = false
            };

            try
            {
                using (var stringreader = new StringReader(EscapeNonXMLChars(xhtmlText)))
                {
                    XmlReader reader = XmlReader.Create(stringreader, settings);
                    while (reader.Read())
                    {
                        ProcessNode(rtb, handleRef, reader, cs);
                    }
                }
            }
            catch (XmlException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            // apply links style
            var ncf = new CHARFORMAT(CFM.LINK, CFE.LINK);
            ncf.cbSize = Marshal.SizeOf(ncf);
            foreach (var (start, length) in cs.links)
            {
                rtb.Select(start, length);
                SetCharFormat(handleRef, ncf);
            }

            // reposition to first
            rtb.Select(0, 0);
            EndUpdate(handleRef, oldMask);
            rtb.Invalidate();
        }

        private static void ProcessNode(RichTextBox rtb, HandleRef handleRef, XmlReader reader, RTFCurrentState cs)
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    ProcessElement(reader, cs, rtb);
                    break;
                case XmlNodeType.EndElement:
                    ProcessEndElement(reader, cs, rtb);
                    break;
                case XmlNodeType.Text:
                    string strData = reader.Value;
                    bool bNewParagraph = (strData.IndexOf("\r\n", 0) >= 0) || (strData.IndexOf("\n", 0) >= 0);

                    if (strData.Length > 0)
                    {
                        // now, add text to control
                        int nStartCache = rtb.SelectionStart;
                        rtb.SelectedText = strData;
                        rtb.Select(nStartCache, strData.Length);

                        // apply format
                        if (cs.paraFormatChanged)
                        {
                            SetParaFormat(handleRef, cs.pf);
                        }

                        if (cs.charFormatChanged)
                        {
                            SetCharFormat(handleRef, cs.cf);
                        }

                        cs.charFormatChanged = false;
                        cs.paraFormatChanged = false;

                        // reposition to final
                        rtb.Select(rtb.TextLength + 1, 0);

                        // new paragraph requires to reset alignment
                        if (bNewParagraph)
                        {
                            cs.pf.dwMask = PFM.ALIGNMENT | PFM.NUMBERING;
                            cs.pf.wAlignment = PFA.LEFT;
                            cs.pf.wNumbering = 0;
                            cs.paraFormatChanged = true;
                        }
                    }

                    break;
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    rtb.SelectedText = reader.Value;
                    break;
                case XmlNodeType.XmlDeclaration:
                case XmlNodeType.ProcessingInstruction:
                    break;
                case XmlNodeType.Comment:
                    break;
            }
        }

        private static void ProcessElement(XmlReader reader, RTFCurrentState cs, RichTextBox rtb)
        {
            switch (reader.Name.ToLower())
            {
                case "b":
                    cs.cf.dwMask |= CFM.WEIGHT | CFM.BOLD;
                    cs.cf.dwEffects |= CFE.BOLD;
                    cs.cf.wWeight = FW.BOLD;
                    cs.charFormatChanged = true;
                    break;
                case "i":
                    cs.cf.dwMask |= CFM.ITALIC;
                    cs.cf.dwEffects |= CFE.ITALIC;
                    cs.charFormatChanged = true;
                    break;
                case "u":
                    cs.cf.dwMask |= CFM.UNDERLINE | CFM.UNDERLINETYPE;
                    cs.cf.dwEffects |= CFE.UNDERLINE;
                    cs.cf.bUnderlineType = CFU.UNDERLINE;
                    cs.charFormatChanged = true;
                    break;
                case "s":
                    cs.cf.dwMask |= CFM.STRIKEOUT;
                    cs.cf.dwEffects |= CFE.STRIKEOUT;
                    cs.charFormatChanged = true;
                    break;
                case "sup":
                    cs.cf.dwMask |= CFM.SUPERSCRIPT;
                    cs.cf.dwEffects |= CFE.SUPERSCRIPT;
                    cs.charFormatChanged = true;
                    break;
                case "sub":
                    cs.cf.dwMask |= CFM.SUBSCRIPT;
                    cs.cf.dwEffects |= CFE.SUBSCRIPT;
                    cs.charFormatChanged = true;
                    break;
                case "a":
                    cs.hyperlinkStart = rtb.TextLength;
                    cs.hyperlink = null;
                    while (reader.MoveToNextAttribute())
                    {
                        if (reader.Name.ToLower() == "href")
                        {
                            cs.hyperlink = reader.Value;
                        }
                    }

                    reader.MoveToElement();
                    break;
                case "p":
                    cs.spf.Push(cs.pf);
                    while (reader.MoveToNextAttribute())
                    {
                        if (reader.Name.ToLower() == "align")
                        {
                            if (reader.Value == "left")
                            {
                                cs.pf.dwMask |= PFM.ALIGNMENT;
                                cs.pf.wAlignment = PFA.LEFT;
                                cs.paraFormatChanged = true;
                            }
                            else if (reader.Value == "right")
                            {
                                cs.pf.dwMask |= PFM.ALIGNMENT;
                                cs.pf.wAlignment = PFA.RIGHT;
                                cs.paraFormatChanged = true;
                            }
                            else if (reader.Value == "center")
                            {
                                cs.pf.dwMask |= PFM.ALIGNMENT;
                                cs.pf.wAlignment = PFA.CENTER;
                                cs.paraFormatChanged = true;
                            }
                        }
                    }

                    reader.MoveToElement();
                    break;
                case "li":
                    cs.spf.Push(cs.pf);
                    if (cs.pf.wNumbering != PFN.BULLET)
                    {
                        cs.pf.dwMask |= PFM.NUMBERING;
                        cs.pf.wNumbering = PFN.BULLET;
                        cs.paraFormatChanged = true;
                    }

                    break;
                case "font":
                    cs.scf.Push(cs.cf);
                    string strFont = cs.cf.szFaceName;
                    int crFont = cs.cf.crTextColor;
                    int yHeight = cs.cf.yHeight;

                    while (reader.MoveToNextAttribute())
                    {
                        switch (reader.Name.ToLower())
                        {
                            case "face":
                                cs.cf.dwMask |= CFM.FACE;
                                strFont = reader.Value;
                                break;
                            case "size":
                                cs.cf.dwMask |= CFM.SIZE;
                                yHeight = int.Parse(reader.Value);
                                yHeight *= 20 * 5;
                                break;
                            case "color":
                                cs.cf.dwMask |= CFM.COLOR;
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

                    cs.cf.szFaceName = strFont;
                    cs.cf.crTextColor = crFont;
                    cs.cf.yHeight = yHeight;

                    cs.cf.dwEffects &= ~CFE.AUTOCOLOR;
                    cs.charFormatChanged = true;
                    break;
            }
        }

        private static void ProcessEndElement(XmlReader reader, RTFCurrentState cs, RichTextBox rtb)
        {
            switch (reader.Name)
            {
                case "b":
                    cs.cf.dwEffects &= ~CFE.BOLD;
                    cs.cf.wWeight = FW.NORMAL;
                    cs.charFormatChanged = true;
                    break;
                case "i":
                    cs.cf.dwEffects &= ~CFE.ITALIC;
                    cs.charFormatChanged = true;
                    break;
                case "u":
                    cs.cf.dwEffects &= ~CFE.UNDERLINE;
                    cs.charFormatChanged = true;
                    break;
                case "s":
                    cs.cf.dwEffects &= ~CFE.STRIKEOUT;
                    cs.charFormatChanged = true;
                    break;
                case "sup":
                    cs.cf.dwEffects &= ~CFE.SUPERSCRIPT;
                    cs.charFormatChanged = true;
                    break;
                case "sub":
                    cs.cf.dwEffects &= ~CFE.SUBSCRIPT;
                    cs.charFormatChanged = true;
                    break;
                case "a":
                    int length = rtb.TextLength - cs.hyperlinkStart;

                    if (cs.hyperlink != null)
                    {
                        rtb.Select(cs.hyperlinkStart, length);
                        if (cs.hyperlink != rtb.SelectedText)
                        {
                            string rtfText = rtb.SelectedRtf;
                            int idx = rtfText.LastIndexOf('}');
                            if (idx != -1)
                            {
                                string head = rtfText.Substring(0, idx);
                                string tail = rtfText.Substring(idx);
                                RtbSetSelectedRtf(rtb, head + @"\v #" + cs.hyperlink + @"\v0" + tail);
                                length = rtb.TextLength - cs.hyperlinkStart;
                            }
                        }

                        // reposition to final
                        rtb.Select(rtb.TextLength + 1, 0);
                    }

                    cs.links.Add(new KeyValuePair<int, int>(cs.hyperlinkStart, length));

                    cs.hyperlinkStart = -1;
                    break;
                case "p":
                    cs.pf = cs.spf.Pop();
                    cs.paraFormatChanged = true;
                    break;
                case "li":
                    cs.pf = cs.spf.Pop();
                    cs.paraFormatChanged = true;
                    break;
                case "font":
                    cs.cf = cs.scf.Pop();
                    cs.charFormatChanged = true;
                    break;
            }
        }

        public static void SetXHTMLTextAsPlainText(this RichTextBox rtb, string xhtmlText)
        {
            rtb.Clear();

            rtb.HideSelection = true;

            var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };

            try
            {
                using (var strReader = new StringReader(xhtmlText))
                {
                    XmlReader reader = XmlReader.Create(strReader, settings);
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Text:
                            case XmlNodeType.Whitespace:
                            case XmlNodeType.SignificantWhitespace:
                                rtb.SelectedText = reader.Value;
                                break;
                            case XmlNodeType.Element:
                            case XmlNodeType.EndElement:
                                break;
                            case XmlNodeType.XmlDeclaration:
                            case XmlNodeType.ProcessingInstruction:
                                break;
                            case XmlNodeType.Comment:
                                break;
                        }
                    }
                }
            }
            catch (XmlException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            rtb.HideSelection = false;

            // reposition to final
            rtb.Select(rtb.TextLength + 1, 0);
        }
    }
}
