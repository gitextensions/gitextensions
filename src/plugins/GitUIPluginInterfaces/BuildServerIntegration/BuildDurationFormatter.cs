namespace GitUIPluginInterfaces.BuildServerIntegration;

public interface IBuildDurationFormatter
{
    string Format(long? durationMilliseconds);
}

public class BuildDurationFormatter : IBuildDurationFormatter
{
    public string Format(long? durationMilliseconds)
        => durationMilliseconds.HasValue ? TimeSpan.FromMilliseconds(durationMilliseconds.Value).ToString(@"mm\:ss") : string.Empty;
}
