using System;
using System.Collections.Generic;

namespace GitCommands
{
    public class GitVersion : IComparable<GitVersion>
    {
        private static readonly GitVersion v1_7_0 = new GitVersion("1.7.0");
        private static readonly GitVersion v1_7_1 = new GitVersion("1.7.1");
        private static readonly GitVersion v1_7_7 = new GitVersion("1.7.7");
        private static readonly GitVersion v1_7_11 = new GitVersion("1.7.11");

        public static readonly GitVersion LastSupportedVersion = v1_7_0;

        private const string Prefix = "git version";

        public readonly string Full;
        private readonly int a;
        private readonly int b;
        private readonly int c;
        private readonly int d;

        public GitVersion(string version)
        {
            Full = Fix(version);

            IList<int> numbers = GetNumbers(Full);
            a = Get(numbers, 0);
            b = Get(numbers, 1);
            c = Get(numbers, 2);
            d = Get(numbers, 3);
        }

        public bool FetchCanAskForProgress
        {
            get { return this >= v1_7_1; }
        }

        public bool PushCanAskForProgress
        {
            get { return this >= v1_7_1; }
        }

        public bool StashUntrackedFilesSupported
        {
            get { return this >= v1_7_7; }
        }

        public bool SupportPushWithRecursiveSubmodulesCheck
        {
            get { return this >= v1_7_7; }
        }

        public bool SupportPushWithRecursiveSubmodulesOnDemand
        {
            get { return this >= v1_7_11; }
        }

        public bool IsUnknown
        {
            get { return a == 0 && b == 0 && c == 0 && d == 0; }
        }

        // Returns true if it's possible to pass given string as command line
        // argument to git for searching.
        // As of msysgit 1.7.3.1 git-rev-list requires its search arguments
        // (--author, --committer, --regex) to be encoded with the exact encoding
        // used at commit time.
        // This causes problems under Windows, where command line arguments are
        // passed as WideChars. Git uses argv, which contains strings
        // recoded into 8-bit system codepage, and that means searching for strings
        // outside ASCII range gets crippled, unless commit messages in git
        // are encoded according to system codepage.
        // For versions of git displaying such behaviour, this function should return
        // false if its argument isn't command-line safe, i.e. it contains chars
        // outside ASCII (7bit) range.
        public bool IsRegExStringCmdPassable(string s)
        {
            if (s==null) return true;
            foreach (char ch in s)
                if ((uint)ch >= 0x80) return false;
            return true;
        }

        private static string Fix(string version)
        {
            if (version == null)
                return String.Empty;

            if (version.StartsWith(Prefix))
                return version.Substring(Prefix.Length).Trim();

            return version.Trim();
        }

        private static int Get(IList<int> values, int index)
        {
            return index < values.Count ? values[index] : 0;
        }

        private static IList<int> GetNumbers(string version)
        {
            IEnumerable<int> numbers = ParseNumbers(version);
            return new List<int>(numbers);
        }

        private static IEnumerable<int> ParseNumbers(string version)
        {
            string[] numbers = version.Split('.');

            foreach (var number in numbers)
            {
                int value;

                if (Int32.TryParse(number, out value))
                {
                    yield return value;
                }
            }
        }

        public int CompareTo(GitVersion other)
        {
            return Compare(this, other);
        }

        private static int Compare(GitVersion left, GitVersion right)
        {
            if (left == null && right == null) return 0;
            if (right == null) return 1;
            if (left == null) return -1;
            
            int compareA = left.a.CompareTo(right.a);
            if (compareA != 0) return compareA;

            int compareB = left.b.CompareTo(right.b);
            if (compareB != 0) return compareB;

            int compareC = left.c.CompareTo(right.c);
            if (compareC != 0) return compareC;

            return left.d.CompareTo(right.d);
        }

        public static bool operator >(GitVersion left, GitVersion right)
        {
            return Compare(left, right) > 0;
        }

        public static bool operator <(GitVersion left, GitVersion right)
        {
            return Compare(left, right) < 0;
        }

        public static bool operator >=(GitVersion left, GitVersion right)
        {
            return Compare(left, right) >= 0;
        }

        public static bool operator <=(GitVersion left, GitVersion right)
        {
            return Compare(left, right) <= 0;
        }

        public override string ToString()
        {
            return Full.Replace(".msysgit.0", "").Replace(".msysgit.1", "");
        }
    }
}
