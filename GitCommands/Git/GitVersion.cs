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
        private static readonly GitVersion v1_8_4 = new GitVersion("1.8.4");
        private static readonly GitVersion v1_8_5 = new GitVersion("1.8.5");
        private static readonly GitVersion v2_0_1 = new GitVersion("2.0.1");
        private static readonly GitVersion v2_5_1 = new GitVersion("2.5.1");
        private static readonly GitVersion v2_7_0 = new GitVersion("2.7.0");
        private static readonly GitVersion v2_9_0 = new GitVersion("2.9.0");
        private static readonly GitVersion v2_11_0 = new GitVersion("2.11.0");
        private static readonly GitVersion v2_15_2 = new GitVersion("2.15.2");
        private static readonly GitVersion v2_18_0 = new GitVersion("2.18.0");

        public static readonly GitVersion LastSupportedVersion = v2_11_0;
        public static readonly GitVersion LastRecommendedVersion = v2_18_0;

        private const string Prefix = "git version";

        public readonly string Full;
        private readonly int _a;
        private readonly int _b;
        private readonly int _c;
        private readonly int _d;

        public GitVersion(string version)
        {
            Full = Fix(version);

            var numbers = GetNumbers(Full);
            _a = Get(numbers, 0);
            _b = Get(numbers, 1);
            _c = Get(numbers, 2);
            _d = Get(numbers, 3);
        }

        public bool FetchCanAskForProgress => this >= v1_7_1;

        public bool LogFormatRecodesCommitMessage => this >= v1_8_4;

        public bool PushCanAskForProgress => this >= v1_7_1;

        public bool StashUntrackedFilesSupported => this >= v1_7_7;

        public bool SupportPushWithRecursiveSubmodulesCheck => this >= v1_7_7;

        public bool SupportPushWithRecursiveSubmodulesOnDemand => this >= v1_7_11;

        public bool SupportPushForceWithLease => this >= v1_8_5;

        public bool RaceConditionWhenGitStatusIsUpdatingIndex => this < v2_0_1;

        public bool SupportWorktree => this >= v2_5_1;

        public bool SupportWorktreeList => this >= v2_7_0;

        public bool SupportMergeUnrelatedHistory => this >= v2_9_0;

        public bool SupportStatusPorcelainV2 => this >= v2_11_0;

        public bool SupportNoOptionalLocks => this >= v2_15_2;

        public bool IsUnknown => _a == 0 && _b == 0 && _c == 0 && _d == 0;

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
            if (s == null)
            {
                return true;
            }

            foreach (char ch in s)
            {
                if ((uint)ch >= 0x80)
                {
                    return false;
                }
            }

            return true;
        }

        private static string Fix(string version)
        {
            if (version == null)
            {
                return string.Empty;
            }

            if (version.StartsWith(Prefix))
            {
                return version.Substring(Prefix.Length).Trim();
            }

            return version.Trim();
        }

        private static int Get(IReadOnlyList<int> values, int index)
        {
            return index < values.Count ? values[index] : 0;
        }

        private static IReadOnlyList<int> GetNumbers(string version)
        {
            IEnumerable<int> numbers = ParseNumbers(version);
            return new List<int>(numbers);
        }

        private static IEnumerable<int> ParseNumbers(string version)
        {
            string[] numbers = version.Split('.');

            foreach (var number in numbers)
            {
                if (int.TryParse(number, out var value))
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
            if (left == null && right == null)
            {
                return 0;
            }

            if (right == null)
            {
                return 1;
            }

            if (left == null)
            {
                return -1;
            }

            int compareA = left._a.CompareTo(right._a);
            if (compareA != 0)
            {
                return compareA;
            }

            int compareB = left._b.CompareTo(right._b);
            if (compareB != 0)
            {
                return compareB;
            }

            int compareC = left._c.CompareTo(right._c);
            if (compareC != 0)
            {
                return compareC;
            }

            return left._d.CompareTo(right._d);
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
            return Full
                .Replace(".msysgit.0", "")
                .Replace(".msysgit.1", "")
                .Replace(".windows.0", "")
                .Replace(".windows.1", "");
        }
    }
}
