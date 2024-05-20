namespace NetSpell.SpellChecker.Dictionary.Phonetic
{
    /// <summary>
    ///     This class hold the settings for a phonetic rule
    /// </summary>
    public class PhoneticRule
    {
        /// <summary>
        ///     True if this rule should be applied to the beginning only
        /// </summary>
        public bool BeginningOnly { get; set; }

        /// <summary>
        ///     The ascii condition array
        /// </summary>
        public int[] Condition { get; } = new int[256];

        /// <summary>
        ///     The number of conditions
        /// </summary>
        public int ConditionCount { get; set; }

        /// <summary>
        ///     The number of chars to consume with this rule
        /// </summary>
        public int ConsumeCount { get; set; }

        /// <summary>
        ///     True if this rule should be applied to the end only
        /// </summary>
        public bool EndOnly { get; set; }

        /// <summary>
        ///     The priority of this rule
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        ///     True if this rule should run in replace mode
        /// </summary>
        public bool ReplaceMode { get; set; }

        /// <summary>
        ///     The string to use when replacing
        /// </summary>
        public string ReplaceString { get; set; }
    }
}
