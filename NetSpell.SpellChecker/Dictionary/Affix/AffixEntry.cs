namespace NetSpell.SpellChecker.Dictionary.Affix
{
    /// <summary>
    ///     Rule Entry for expanding base words
    /// </summary>
    public class AffixEntry
    {
        /// <summary>
        ///     The characters to add to the string
        /// </summary>
        public string AddCharacters { get; set; } = "";

        /// <summary>
        ///     The condition to be met in order to add characters
        /// </summary>
        public int[] Condition { get; set; } = new int[1200];

        /// <summary>
        ///     The characters to remove before adding characters
        /// </summary>
        public string StripCharacters { get; set; } = "";

        /// <summary>
        ///     The number of conditions that must be met
        /// </summary>
        public int ConditionCount { get; set; }
    }
}
