using GitExtUtils;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public class GitVersion : IComparable<GitVersion>
    {
        private static readonly GitVersion v2_19_0 = new("2.19.0");
        private static readonly GitVersion v2_20_0 = new("2.20.0");
        private static readonly GitVersion v2_32_0 = new("2.32.0");
        private static readonly GitVersion v2_35_0 = new("2.35.0");
        private static readonly GitVersion v2_38_0 = new("2.38.0");

        /// <summary>
        /// The recommended Git version (normally latest official before a GE release).
        /// This and later versions are green in the settings check.
        /// </summary>
        public static readonly GitVersion LastRecommendedVersion = new("2.38.1");

        /// <summary>
        /// The oldest version with reasonable reliable support in GE.
        /// Older than this version is red in settings.
        /// </summary>
        public static readonly GitVersion LastSupportedVersion = new("2.25.0");

        /// <summary>
        /// The oldest Git version without known incompatibilities.
        /// </summary>
        public static readonly GitVersion LastVersionWithoutKnownLimitations = new("2.15.2");

        private static readonly Dictionary<string, GitVersion> _current = new();

        /// <summary>
        /// GitVersion for the native ("Windows") Git
        /// </summary>
        public static GitVersion Current => CurrentVersion();

        /// <summary>
        /// The GitVersion for the gitIdentifiable
        /// </summary>
        /// <param name="gitIdentifiable">The unique identification of the Git executable</param>
        /// <returns>The GitVersion</returns>
        public static GitVersion CurrentVersion(IExecutable gitExec = null, string gitIdentifiable = "")
        {
            if (!_current.ContainsKey(gitIdentifiable) || _current[gitIdentifiable] is null || _current[gitIdentifiable].IsUnknown)
            {
                gitExec ??= new Executable(AppSettings.GitCommand);
                string output = gitExec.GetOutput("--version");
                _current[gitIdentifiable] = new GitVersion(output);
                if (_current[gitIdentifiable] < LastVersionWithoutKnownLimitations)
                {
                    // Report the last supported version rather than the last version without known issues
                    MessageBox.Show(null, $"{_current[gitIdentifiable]} is lower than {LastSupportedVersion}. Some commands can fail.", "Unsupported Git version", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return _current[gitIdentifiable];
        }

        public static void ResetVersion()
        {
            _current.Clear();
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
        public bool SupportStashStaged => this >= v2_35_0;
        public bool SupportUpdateRefs => this >= v2_38_0;
        public bool SupportRangeDiffPath => this >= v2_38_0;
        public bool SupportAmendCommits => this >= v2_32_0;

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
            return Full;
        }
    }
}
