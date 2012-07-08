namespace System
{
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

    public static class StreamExtensions
    {
        //copied from http://stackoverflow.com/a/1253049/1399492
        //it can be removed after move to .net 4
        public static void CopyTo(this System.IO.Stream src, System.IO.Stream dest)
        {
            int size = (src.CanSeek) ? Math.Min((int)(src.Length - src.Position), 0x2000) : 0x2000;
            byte[] buffer = new byte[size];
            int n;
            do
            {
                n = src.Read(buffer, 0, buffer.Length);
                dest.Write(buffer, 0, n);
            } while (n != 0);
        }
    }


}
