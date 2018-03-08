namespace GitUI.SpellChecker
{
    public struct TextPos
    {
        public int End { get; }
        public int Start { get; }

        public TextPos(int start, int end)
        {
            Start = start;
            End = end;
        }
    }
}