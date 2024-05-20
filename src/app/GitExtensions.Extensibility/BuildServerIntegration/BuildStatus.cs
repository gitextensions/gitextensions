namespace GitExtensions.Extensibility.BuildServerIntegration;

public enum BuildStatus
{
    Unknown,
    InProgress,
    Success,
    Failure,
    Unstable,
    Stopped
}
