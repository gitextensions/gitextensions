using System.Collections;
using Microsoft;

namespace GitExtensions.Extensibility;

/// <summary>
/// Splits a string by a delimiter, producing substrings lazily during enumeration.
/// </summary>
/// <remarks>
/// Unlike <see cref="string.Split(char[])"/> and overloads, <see cref="LazyStringSplit"/>
/// does not allocate an array for the return, and allocates strings on demand during
/// enumeration. A custom enumerator type is used so that the only allocations made are
/// the substrings themselves. We also avoid the large internal arrays assigned by the
/// methods on <see cref="string"/>.
/// </remarks>
public readonly struct LazyStringSplit : IEnumerable<string>
{
    private readonly string _input;
    private readonly char _delimiter;
    private readonly StringSplitOptions _options;

    public LazyStringSplit(string input, char delimiter, StringSplitOptions options = StringSplitOptions.None)
    {
        Requires.NotNull(input, nameof(input));

        _input = input;
        _delimiter = delimiter;
        _options = options;
    }

    public Enumerator GetEnumerator() => new(this, _options);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IEnumerator<string> IEnumerable<string>.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<string>
    {
        private readonly StringSplitOptions _options;
        private readonly string _input;
        private readonly char _delimiter;
        private int _index;

        internal Enumerator(in LazyStringSplit split, StringSplitOptions options)
        {
            _options = options;
            _index = 0;
            _input = split._input;
            _delimiter = split._delimiter;
            Current = null!;
        }

        public string Current { get; private set; }

        public bool MoveNext()
        {
            while (_index < _input.Length)
            {
                int delimiterIndex = _input.IndexOf(_delimiter, _index);

                if (delimiterIndex == -1)
                {
                    Current = _input[_index..];
                    _index = _input.Length + 1;
                    return true;
                }

                int length = delimiterIndex - _index;

                if (length == 0)
                {
                    _index++;
                    if (_options == StringSplitOptions.RemoveEmptyEntries)
                    {
                        continue;
                    }
                    else
                    {
                        Current = "";
                        return true;
                    }
                }

                Current = _input.Substring(_index, length);
                _index = delimiterIndex + 1;

                return true;
            }

            if (_options == StringSplitOptions.None && _index == _input.Length)
            {
                _index++;
                Current = "";
                return true;
            }

            return false;
        }

        object IEnumerator.Current => Current;

        void IEnumerator.Reset()
        {
            _index = 0;
            Current = null!;
        }

        void IDisposable.Dispose()
        {
        }
    }
}

public static class LazyStringSplitExtensions
{
    public static LazyStringSplit LazySplit(this string s, char delimiter, StringSplitOptions options = StringSplitOptions.None)
    {
        return new(s, delimiter, options);
    }

    public static IEnumerable<T> Select<T>(this LazyStringSplit split, Func<string, T> func)
    {
        foreach (string value in split)
        {
            yield return func(value);
        }
    }

    public static string First(this LazyStringSplit split)
    {
        return split.FirstOrDefault() ?? throw new InvalidOperationException("Sequence is empty.");
    }

    public static string? FirstOrDefault(this LazyStringSplit split)
    {
        using LazyStringSplit.Enumerator enumerator = split.GetEnumerator();
        return enumerator.MoveNext() ? enumerator.Current : null;
    }

    public static string? LastOrDefault(this LazyStringSplit split)
    {
        using LazyStringSplit.Enumerator enumerator = split.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            return null;
        }

        while (true)
        {
            string last = enumerator.Current;

            if (!enumerator.MoveNext())
            {
                return last;
            }
        }
    }

    public static IEnumerable<string> Where(this LazyStringSplit split, Func<string, bool> predicate)
    {
        foreach (string s in split)
        {
            if (predicate(s))
            {
                yield return s;
            }
        }
    }
}
