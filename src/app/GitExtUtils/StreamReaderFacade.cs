namespace GitExtUtils;

public sealed class StreamReaderFacade(StreamReader streamReader) : IStreamReader
{
    private StreamReader _streamReader = streamReader;

    public bool EndOfStream => _streamReader.EndOfStream;

    public ValueTask<int> ReadAsync(Memory<char> buffer, CancellationToken cancellationToken) => _streamReader.ReadAsync(buffer, cancellationToken);
}
