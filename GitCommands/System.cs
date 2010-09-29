namespace System
{

    public class Tuple<T, S>
    {
        public Tuple(T item1, S item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public T Item1 { get; private set; }
        public S Item2 { get; private set; }
    }

    public static class Tuple
    {
        public static Tuple<T, S> Create<T, S>(T item1, S item2)
        {
            return new Tuple<T, S>(item1, item2);
        }
    }

    public class Option<T>
    {
        private readonly bool _isNone;
        private readonly T _some;

        private Option(T value)
        {
            _some = value;
        }

        private Option()
        {
            _isNone = true;
        }

        public static Option<T> None
        {
            get { return new Option<T>(); }
        }

        public static Option<T> From(T value)
        {
            return new Option<T>(value);
        }

        public bool IsNone
        {
            get { return _isNone; }
        }

        public T Some
        {
            get
            {
                if (_isNone)
                    throw new InvalidOperationException("Maybe is none");

                return _some;
            }
        }

        public override string ToString()
        {
            if (_isNone)
                return "None: " + typeof(T).FullName;

            return _some.ToString();
        }
    }
}