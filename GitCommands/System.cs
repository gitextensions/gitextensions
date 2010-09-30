namespace System
{
    public class Tuple<T1, T2>
    {
        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
    }

    public static class Tuple
    {
        public static Tuple<T, S> Create<T, S>(T item1, S item2)
        {
            return new Tuple<T, S>(item1, item2);
        }
    }
}
