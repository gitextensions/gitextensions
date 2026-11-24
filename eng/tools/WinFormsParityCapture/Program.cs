using System.Runtime.ExceptionServices;

namespace WinFormsParityCapture;

internal static class Program
{
    [STAThread]
    private static async Task<int> Main(string[] args)
    {
        try
        {
            CaptureOptions options = CaptureOptions.Parse(args);
            return options.Command switch
            {
                CaptureCommand.Capture => await CaptureRunner.CaptureAsync(options),
                CaptureCommand.Validate => CaptureRunner.Validate(options),
                CaptureCommand.Worker => RunWorkerOnSta(options),
                _ => throw new InvalidOperationException($"Unsupported command {options.Command}.")
            };
        }
        catch (CaptureHelpException)
        {
            CaptureOptions.WriteHelp(Console.Out);
            return 0;
        }
        catch (Exception exception)
        {
            await Console.Error.WriteLineAsync(exception.ToString());
            return 1;
        }
    }

    private static int RunWorkerOnSta(CaptureOptions options)
    {
        int result = 1;
        ExceptionDispatchInfo? exception = null;
        Thread workerThread = new(
            () =>
            {
                try
                {
                    result = CaptureRunner.CaptureWorker(options);
                }
                catch (Exception ex)
                {
                    exception = ExceptionDispatchInfo.Capture(ex);
                }
            })
        {
            IsBackground = false,
            Name = "WinForms parity capture worker"
        };
        workerThread.SetApartmentState(ApartmentState.STA);
        workerThread.Start();
        workerThread.Join();
        exception?.Throw();
        return result;
    }
}
