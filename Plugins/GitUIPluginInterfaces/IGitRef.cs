namespace GitUIPluginInterfaces
{
    public interface IGitRef : IGitItem
    {
        string CompleteName { get; }
        bool IsBisect { get; }
        bool IsBisectGood { get; }
        bool IsBisectBad { get; }
        bool IsStash { get; }

        /// <summary>
        /// True when Guid is a checksum of an object (e.g. commit) to which another object
        /// with Name (e.g. annotated tag) is applied.
        /// <para>False when Name and Guid are denoting the same object.</para>
        /// </summary>
        bool IsDereference { get; }

        bool IsHead { get; }
        bool IsRemote { get; }
        bool IsTag { get; }
        string LocalName { get; }
        string MergeWith { get; set; }
        IGitModule Module { get; }
        string Remote { get; }
        string TrackingRemote { get; set; }
        bool IsSelected { get; set; }
        bool IsSelectedHeadMergeSource { get; set; }

        /// <summary>
        /// This method is a faster than the property above. The property reads the config file
        /// every time it is accessed. This method accepts a config file what makes it faster when loading
        /// the revision graph.
        /// </summary>
        string GetTrackingRemote(ISettingsValueGetter configFile);

        /// <summary>
        /// This method is a faster than the property above. The property reads the config file
        /// every time it is accessed. This method accepts a config file which makes it faster when loading
        /// the revision graph.
        /// </summary>
        string GetMergeWith(ISettingsValueGetter configFile);
    }
}