namespace GitUI.SpellChecker
{
    public struct TextPos
    {
        public int End;
        public int Start;

        public TextPos(int start, int end)
        {
            Start = start;
            End = end;
        }
    }
}