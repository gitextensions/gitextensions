namespace GitExtUtils;

public interface IStreamReader
{
    bool EndOfStream { get; }

    ValueTask<int> ReadAsync(Memory<char> buffer, CancellationToken cancellationToken);
}
