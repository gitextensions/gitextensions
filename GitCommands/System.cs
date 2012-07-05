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
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }
    }

    public static class StringExtensions
    {

        public static string SkipStr(this string str, string toSkip)
        {
            if (str == null)
                return null;

            int idx;
            idx = str.IndexOf(toSkip);
            if (idx != -1)
                return str.Substring(idx + toSkip.Length);
            else
                return null;
        }

        public static String TakeUntilStr(this string str, String untilStr)
        {
            if (str == null)
                return null;

            int idx;
            idx = str.IndexOf(untilStr);
            if (idx != -1)
                return str.Substring(0, idx);
            else
                return str;
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }


        public static string Join(this string left, string sep, string right)
        {
            if (left.IsNullOrEmpty())
                return right;
            else if (right.IsNullOrEmpty())
                return left;
            else
                return left + sep + right;
        }

        public static string Quote(this string s)
        {
            return s.Quote("\"");
        }

        public static string Quote(this string s, string quotationMark)
        {
            if (s == null)
                return string.Empty;

            return quotationMark + s + quotationMark;
        }


    }

    public static class BoolExtensions
    {

        public static string AsForce(this bool force)
        {
            return force ? " -f " : string.Empty;
        }
        
    }

}
