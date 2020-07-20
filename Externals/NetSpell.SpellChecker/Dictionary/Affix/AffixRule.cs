using System.Collections.Generic;

namespace NetSpell.SpellChecker.Dictionary.Affix
{
    using AffixEntryCollection = List<AffixEntry>;

    /// <summary>
    ///     Rule for expanding base words
    /// </summary>
    public class AffixRule
    {
        /// <summary>
        ///     Allow combining prefix and suffix
        /// </summary>
        public bool AllowCombine { get; set; }

        /// <summary>
        ///     Collection of text entries that make up this rule
        /// </summary>
        public AffixEntryCollection AffixEntries { get; set; } = new AffixEntryCollection();

        /// <summary>
        ///     Name of the Affix rule
        /// </summary>
        public string Name { get; set; } = "";
    }
}
