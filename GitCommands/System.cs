namespace System
{
    public delegate void Action();

    public delegate T Func<T>();    

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
}