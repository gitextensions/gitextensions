// --------------------------------------------------------------------------------------------------------------------
// <copyright company="SharpDevelop" file="RtfWriter.cs">
//   Mike Krüger, Alexander Ulitin
// </copyright>
// <file>
//     <copyright see="prj:///doc/copyright.txt" />
//     <license see="prj:///doc/license.txt" />
//     <owner name="Mike Krüger" email="mike@icsharpcode.net" />
//     <version>$Revision$</version>
// </file>
// <summary>
//   Rtf formatter for TextArea with support highlight code and markers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor
{
    public class RtfWriter
    {
        private Dictionary<string, int> _colors;
        private int _colorNum;
        private StringBuilder _colorString;

        private List<TextMarker> _markers;
        private StringBuilder _contentRtf;
        private RtfWriterState _state;

        private HashSet<int> _offsetHiChange;

        private TextArea _textArea;

        private RtfWriter()
        {
        }

        private IDocument Document
        {
            get
            {
                return _textArea.Document;
            }
        }

        public static string GenerateSelectedRtf(TextArea textArea)
        {
            return new RtfWriter().GenerateRtfInternal(textArea, true);
        }

        public static string GenerateRtf(TextArea textArea)
        {
            return new RtfWriter().GenerateRtfInternal(textArea, false);
        }

        private string GenerateRtfInternal(TextArea textArea, bool onlySelected)
        {
            _textArea = textArea;
            _colors = new Dictionary<string, int>();
            _colorNum = 0;
            _colorString = new StringBuilder();
            var docRtf = new StringBuilder();

            docRtf.Append(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1031");
 
            BuildFontTable(docRtf);
                
 
            docRtf.Append('\n');
            if (onlySelected)
            {
                BuildFileContentSelected();
            }
            else
            {
                BuildFileContentAll();
            }

            BuildColorTable(docRtf);
            docRtf.Append('\n');
            docRtf.Append(@"\viewkind4\uc1\pard");
            docRtf.Append(_contentRtf);
            docRtf.Append("}");
            return docRtf.ToString();
        }

        private void BuildColorTable(StringBuilder outRtf)
        {
            outRtf.Append(@"{\colortbl ;");
            outRtf.Append(_colorString);
            outRtf.Append("}");
        }

        private void BuildFontTable(StringBuilder outRtf)
        {
            outRtf.Append(@"{\fonttbl");
            outRtf.Append(@"{\f0\fmodern\fprq1\fcharset0 " + Document.TextEditorProperties.Font.Name + ";}");
            outRtf.Append("}");
        }

        private void BuildFileContentSelected()
        {
            InitializeContent();

            foreach (ISelection selection in _textArea.SelectionManager.SelectionCollection)
            {
                int selectionOffset = Document.PositionToOffset(selection.StartPosition);
                int selectionEndOffset = Document.PositionToOffset(selection.EndPosition);

                BuildMarkers(selectionOffset, selectionEndOffset);
                for (int i = selection.StartPosition.Y; i <= selection.EndPosition.Y; ++i)
                {
                    LineSegment line = Document.GetLineSegment(i);
                    ProcessLineSegment(line, selectionOffset, selectionEndOffset);
                }
            }
        }

        private void BuildFileContentAll()
        {
            InitializeContent();
            int selectionOffset = 0;
            int selectionEndOffset = Document.TextLength;
            BuildMarkers(selectionOffset, selectionEndOffset);
            foreach (var line in Document.LineSegmentCollection)
            {
                ProcessLineSegment(line, selectionOffset, selectionEndOffset);
            }
        }

        private void ProcessLineSegment(LineSegment line, int selectionOffset, int selectionEndOffset)
        {
            int offset = line.Offset;
            if (line.Words == null)
            {
                return;
            }

            foreach (TextWord word in line.Words)
            {
                offset = ProcessWord(word, offset, selectionOffset, selectionEndOffset);
            }

            if (offset < selectionEndOffset)
            {
                _contentRtf.Append(@"\par");
            }

            _contentRtf.Append('\n');
        }

        private void BuildMarkers(int selectionOffset, int selectionEndOffset)
        {
            _markers =
                Document.MarkerStrategy.GetMarkers(selectionOffset, selectionEndOffset - selectionOffset)
                        .Where(m => m.TextMarkerType == TextMarkerType.SolidBlock)
                        .ToList();

            _offsetHiChange = new HashSet<int>(_markers.Select(m => m.Offset).Concat(_markers.Select(m => m.EndOffset + 1)));
        }

        private void InitializeContent()
        {
            _contentRtf = new StringBuilder();
            _state = new RtfWriterState
                {
                    CurFontColor = Color.Black,
                    EscapeSequence = false,
                    FirstLine = true,
                    OldItalic = false,
                    OldBold = false
                };
        }

        private int ProcessWord(
            TextWord word,
            int offset,
            int selectionOffset,
            int selectionEndOffset)
        {
            switch (word.Type)
            {
                case TextWordType.Space:
                    BuildHighLight(offset, ref _state.EscapeSequence);
                    if (offset < selectionEndOffset)
                    {
                        _contentRtf.Append(' ');
                    }
                    ++offset;
                    break;

                case TextWordType.Tab:
                    BuildHighLight(offset, ref _state.EscapeSequence);
                    if (offset < selectionEndOffset)
                    {
                        _contentRtf.Append(@"\tab");
                    }
                    ++offset;
                    _state.EscapeSequence = true;
                    break;

                case TextWordType.Word:
                    Color c = word.Color;

                    if (offset + word.Word.Length > selectionOffset && offset < selectionEndOffset)
                    {
                        SetForeColor(c);

                        SetItalic(word);

                        SetBold(word);

                        EmitFirstLineTag(_textArea);

                        var printWord = GetCroppedWord(word, offset, selectionOffset, selectionEndOffset);

                        foreach (char c1 in printWord)
                        {
                            EmitChar(offset, c1);
                            offset++;
                        }
                    }
                    break;
            }
            return offset;
        }

        private void EmitChar(int offset, char c1)
        {
            BuildHighLight(offset, ref _state.EscapeSequence);

            if (_state.EscapeSequence)
            {
                _contentRtf.Append(' ');
                _state.EscapeSequence = false;
            }

            AppendChar(_contentRtf, c1);
        }

        private string GetCroppedWord(TextWord word, int offset, int selectionOffset, int selectionEndOffset)
        {
            string printWord;
            if (offset < selectionOffset)
            {
                printWord = word.Word.Substring(selectionOffset - offset);
            }
            else if (offset + word.Word.Length > selectionEndOffset)
            {
                printWord = word.Word.Substring(0, (offset + word.Word.Length) - selectionEndOffset);
            }
            else
            {
                printWord = word.Word;
            }

            return printWord;
        }

        private void SetForeColor(Color c)
        {
            var colorIndex = GetColorIndex(c);
            if (c != _state.CurFontColor || _state.FirstLine)
            {
                _contentRtf.Append(@"\cf" + colorIndex.ToString());
                _state.CurFontColor = c;
                _state.EscapeSequence = true;
            }
        }

        private void EmitFirstLineTag(TextArea textArea)
        {
            if (_state.FirstLine)
            {
                _contentRtf.Append(@"\f0\fs" + (textArea.TextEditorProperties.Font.Size * 2));
                _state.FirstLine = false;
            }
        }

        private void SetBold(TextWord word)
        {
            if (_state.OldBold != word.Bold)
            {
                if (word.Bold)
                {
                    _contentRtf.Append(@"\b");
                }
                else
                {
                    _contentRtf.Append(@"\b0");
                }
                _state.OldBold = word.Bold;
                _state.EscapeSequence = true;
            }
        }

        private void SetItalic(TextWord word)
        {
            if (_state.OldItalic != word.Italic)
            {
                if (word.Italic)
                {
                    _contentRtf.Append(@"\i");
                }
                else
                {
                    _contentRtf.Append(@"\i0");
                }
                _state.OldItalic = word.Italic;
                _state.EscapeSequence = true;
            }
        }

        private void BuildHighLight(int offset, ref bool escsc)
        {
            TextMarker hi;
            if (_offsetHiChange.Contains(offset))
            {
                escsc = true;
                hi = _markers.FirstOrDefault(m => offset >= m.Offset && offset < m.EndOffset);
                if (hi != null)
                {
                    int colindex = GetColorIndex(hi.Color);
                    _contentRtf.Append(@"\highlight");
                    _contentRtf.Append(colindex);
                }
                else
                {
                    _contentRtf.Append(@"\highlight0");
                }
            }
        }

        private int GetColorIndex(Color c)
        {
            string colorstr = c.R + ", " + c.G + ", " + c.B;

            if (!_colors.ContainsKey(colorstr))
            {
                _colors[colorstr] = ++_colorNum;
                _colorString.Append(@"\red" + c.R + @"\green" + c.G + @"\blue" + c.B + ";");
            }

            var colorIndex = _colors[colorstr];
            return colorIndex;
        }

        private void AppendChar(StringBuilder rtfOutput, char c)
        {
            switch (c)
            {
                case '\\':
                    rtfOutput.Append(@"\\");
                    break;
                case '{':
                    rtfOutput.Append("\\{");
                    break;
                case '}':
                    rtfOutput.Append("\\}");
                    break;
                default:
                    if (c < 256)
                    {
                        rtfOutput.Append(c);
                    }
                    else
                    {
                        // yes, RTF really expects signed 16-bit integers!
                        rtfOutput.Append("\\u" + unchecked((short)c).ToString() + "?");
                    }
                    break;
            }
        }

        private struct RtfWriterState
        {
            public bool FirstLine;
            public Color CurFontColor;
            public bool OldItalic;
            public bool OldBold;
            public bool EscapeSequence;
        }
    }
}
