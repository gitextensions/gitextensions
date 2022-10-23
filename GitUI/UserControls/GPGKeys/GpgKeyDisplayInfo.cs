namespace GitUI.UserControls.GPGKeys
{
    public readonly record struct GpgKeyDisplayInfo
    {
        public GpgKeyDisplayInfo(string fingerprint, string keyId, string userId, bool isDefault) : this()
        {
            Fingerprint = fingerprint;
            KeyId = keyId;
            UserId = userId;
            IsDefault = isDefault;
        }

        public string Fingerprint { get; init; }

        public string KeyId { get; init; }

        public string UserId { get; init; }

        public bool IsDefault { get; init; }

        public string Caption { get => string.IsNullOrEmpty(UserId) ? TranslatedStrings.NoKeySelected : $"{(IsDefault ? "*" : "")} ({KeyId}) {UserId}"; }

        public override string ToString() => Caption;
    }
}
