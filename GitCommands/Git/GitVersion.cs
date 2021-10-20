using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitExtUtils;

namespace GitCommands
{
    public class GitVersion : IComparable<GitVersion>
    {
        private static readonly GitVersion v2_19_0 = new("2.19.0");
        private static readonly GitVersion v2_20_0 = new("2.20.0");

        /// <summary>
        /// The recommonded Git version (normally latest official before a GE release).
        /// This and later versions are green in the settings check.
        /// </summary>
        public static readonly GitVersion LastRecommendedVersion = new("2.33.0");

        /// <summary>
        /// The oldest version with reasonable reliable support in GE.
        /// Older than this version is red in settings.
        /// </summary>
        public static readonly GitVersion LastSupportedVersion = new("2.25.0");

        /// <summary>
        /// The oldest Git version without known incompatibilities.
        /// </summary>
        public static readonly GitVersion LastFailVersion = new("2.15.2");

        private static GitVersion? _current;

        public static GitVersion Current
        {
            get
            {
                if (_current is null || _current.IsUnknown)
                {
                    string output = new Executable(AppSettings.GitCommand).GetOutput("--version");
                    _current = new GitVersion(output);
                    if (_current < LastFailVersion)
                    {
                        MessageBox.Show(null, $"{_current} is lower than {LastSupportedVersion}. Some commands can fail.", "Unsupported Git version", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                return _current;
            }
        }

        public static void ResetVersion()
        {
            _current = null;
        }

        public readonly string Full;
        private readonly int _a;
        private readonly int _b;
        private readonly int _c;
        private readonly int _d;

        public GitVersion(string? version)
        {
            Full = Fix();

            var numbers = GetNumbers();
            _a = Get(numbers, 0);
            _b = Get(numbers, 1);
            _c = Get(numbers, 2);
            _d = Get(numbers, 3);

            string Fix()
            {
                if (version is null)
                {
                    return "";
                }

                const string Prefix = "git version";

                if (version.StartsWith(Prefix))
                {
                    return version.Substring(Prefix.Length).Trim();
                }

                return version.Trim();
            }

            IReadOnlyList<int> GetNumbers()
            {
                return ParseNumbers().ToList();

                IEnumerable<int> ParseNumbers()
                {
                    foreach (var number in Full.LazySplit('.'))
                    {
                        if (int.TryParse(number, out var value))
                        {
                            yield return value;
                        }
                    }
                }
            }

            int Get(IReadOnlyList<int> values, int index)
            {
                return index < values.Count ? values[index] : 0;
            }
        }

        public bool SupportRebaseMerges => this >= v2_19_0;
        public bool SupportGuiMergeTool => this >= v2_20_0;
        public bool SupportRangeDiffTool => this >= v2_19_0;

        public bool IsUnknown => _a == 0 && _b == 0 && _c == 0 && _d == 0;

        private static int Compare(GitVersion left, GitVersion right)
        {
            if (left is null && right is null)
            {
                return 0;
            }

            if (right is null)
            {
                return 1;
            }

            if (left is null)
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

        public int CompareTo(GitVersion other) => Compare(this, other);

        public static bool operator >(GitVersion left, GitVersion right) => Compare(left, right) > 0;
        public static bool operator <(GitVersion left, GitVersion right) => Compare(left, right) < 0;
        public static bool operator >=(GitVersion left, GitVersion right) => Compare(left, right) >= 0;
        public static bool operator <=(GitVersion left, GitVersion right) => Compare(left, right) <= 0;

        public override string ToString()
        {
            return Full
                .Replace(".windows.0", "")
                .Replace(".windows.1", "")
                .Replace(".windows.2", "")
                .Replace(".windows.3", "");
        }
    }
}
