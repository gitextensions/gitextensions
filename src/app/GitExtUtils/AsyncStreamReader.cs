using GitExtensions.Extensibility;
using GitUI;
using Microsoft.VisualStudio.Threading;

namespace GitExtUtils;

/// <summary>
///  Reads a stream asynchronously and provides the output split at a linefeed or at a carriage return not followed by linefeed.
///  If no line end is received within a timeout, the already received output is provided.
/// </summary>
/// <remarks>
///  Multiple lines can be provided as one block.
///  <br/>
///  The timeout expires for instance if an input prompt is shown in the stream.
/// </remarks>
public sealed class AsyncStreamReader : IDisposable
{
    private static readonly TimeSpan _nextCharReadTimeout = TimeSpan.FromMilliseconds(300);

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly TaskManager _taskManager = ThreadHelper.CreateTaskManager();

    private bool _disposedValue;

    /// <summary>
    ///  Starts reading the stream and forwards its output to the <paramref name="notify"/> functor.
    /// </summary>
    public AsyncStreamReader(IStreamReader streamReader, Action<string> notify)
    {
        CancellationToken cancellationToken = _cancellationTokenSource.Token;
        _taskManager.FileAndForget(async () =>
        {
            // Read single chars because ReadAsync blocks until a line end is received
            // Wait for start of new output without timeout, but read consecutive chars with timeout in order to display prompts having no line end
            char[] buffer = new char[1];
            Memory<char> bufferMemory = new(buffer);

            string received = "";
            while (true)
            {
                try
                {
                    CancellationTokenSource? readTimeoutCancellationTokenSource = null;
                    CancellationToken readCancellation = GetReadCancellation(addTimeout: received.Length > 0, ref readTimeoutCancellationTokenSource);
                    int length = await streamReader.ReadAsync(bufferMemory, readCancellation);
                    readTimeoutCancellationTokenSource?.Dispose();

                    if (length == 0)
                    {
                        if (streamReader.EndOfStream)
                        {
                            if (received.Length > 0)
                            {
                                notify(received);
                            }

                            return;
                        }

                        continue;
                    }

                    received += buffer[0];

                    int lastLineEnd = received.LastIndexOf(Delimiters.LineFeed) + 1;
                    if (lastLineEnd == 0)
                    {
                        lastLineEnd = received.LastIndexOf(Delimiters.CarriageReturn) + 1;
                        if (lastLineEnd == 0 || lastLineEnd == received.Length)
                        {
                            continue;
                        }
                    }

                    notify(received[..lastLineEnd]);
                    received = received[lastLineEnd..];
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    if (received.Length > 0)
                    {
                        notify(received);
                        received = "";
                    }
                }
            }
        });

        return;

        CancellationToken GetReadCancellation(bool addTimeout, ref CancellationTokenSource readTimeoutCancellationTokenSource)
        {
            if (!addTimeout)
            {
                return cancellationToken;
            }

            readTimeoutCancellationTokenSource = new CancellationTokenSource(_nextCharReadTimeout);
            return cancellationToken.CombineWith(readTimeoutCancellationTokenSource.Token).Token;
        }
    }

    /// <summary>
    ///  Starts reading the stream and forwards its output to the <paramref name="notify"/> functor.
    /// </summary>
    public AsyncStreamReader(StreamReader streamReader, Action<string> notify)
        : this(new StreamReaderFacade(streamReader), notify)
    {
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // Dispose managed state (managed objects)
                _cancellationTokenSource.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    ///  Cancels the asynchronous read.
    /// </summary>
    public void CancelOperation()
    {
        _cancellationTokenSource.Cancel();
    }

    /// <summary>
    ///  Waits until all stream output has been read and forwarded to the notify functor.
    /// </summary>
    public Task WaitUntilEofAsync(CancellationToken cancellationToken)
    {
        return _taskManager.JoinPendingOperationsAsync(cancellationToken);
    }
}
