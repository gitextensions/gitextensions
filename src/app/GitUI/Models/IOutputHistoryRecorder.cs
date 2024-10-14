namespace GitUI.Models;

public interface IOutputHistoryRecorder
{
    /// <summary>
    ///  Records the output after a process has run.
    /// </summary>
    void RecordHistory(in RunProcessInfo runProcess);

    /// <summary>
    ///  Records an exception.
    /// </summary>
    void RecordHistory(in Exception exception);

    /// <summary>
    ///  Records a string message.
    /// </summary>
    void RecordHistory(in string message);
}
