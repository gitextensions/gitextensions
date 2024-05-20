namespace GitExtUtils.GitUI.Theming
{
    public class BrushScope : IDisposable
    {
        private readonly bool _isSystemBrush;

        public static BrushScope ForSystemBrush(Brush brush) =>
            new(brush, isSystemBrush: true);

        public static BrushScope ForRegularBrush(Brush brush) =>
            new(brush, isSystemBrush: false);

        private BrushScope(Brush brush, bool isSystemBrush)
        {
            Brush = brush;
            _isSystemBrush = isSystemBrush;
        }

        public Brush Brush { get; }

        public void Dispose()
        {
            if (!_isSystemBrush)
            {
                Brush.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
