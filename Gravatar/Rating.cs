namespace Gravatar
{
    /// <summary>
    /// Specifies the maximum rating of a given gravatar image request.
    /// </summary>
    public enum Rating
    {
        /// <summary>
        /// Suitable for all audiences.
        /// </summary>
        G,

        /// <summary>
        /// May contain rude gestures, provocatively dressed indiviiduals,
        /// the lesser swear words, or mild violence
        /// </summary>
        PG,

        /// <summary>
        /// May contain such things as harsh profanity, intense violence,
        /// nudity, or hard drug use.
        /// </summary>
        R,

        /// <summary>
        /// May contain hardcore sexual imagery or extremely disturbing
        /// violence.
        /// </summary>
        X,
    }
}