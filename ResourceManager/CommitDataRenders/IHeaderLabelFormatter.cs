namespace ResourceManager.CommitDataRenders
{
    public interface IHeaderLabelFormatter
    {
        string FormatLabel(string label, int desiredLength);
        string FormatLabelPlain(string label, int desiredLength);
    }
}