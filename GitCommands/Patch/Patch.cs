using System.Text;

namespace PatchApply
{
    public class Patch
    {
        private StringBuilder _textBuilder;

        public Patch()
        {
            File = FileType.Text;
        }

        public enum PatchType
        {
            NewFile,
            DeleteFile,
            ChangeFile,
            ChangeFileMode
        }

        public enum FileType
        {
            Binary,
            Text
        }

        public string PatchHeader { get; set; }
        public string PatchIndex { get; set; }
        public FileType File { get; set; }
        public string FileNameA { get; set; }
        public string FileNameB { get; set; }
        public bool CombinedDiff { get; set; }

        public PatchType Type { get; set; }

        public string Text => _textBuilder?.ToString();

        public void AppendText(string text)
        {
            GetTextBuilder().Append(text);
        }

        public void AppendTextLine(string line)
        {
            GetTextBuilder().Append(line).Append('\n');
        }

        private StringBuilder GetTextBuilder()
        {
            return _textBuilder ?? (_textBuilder = new StringBuilder());
        }
    }
}