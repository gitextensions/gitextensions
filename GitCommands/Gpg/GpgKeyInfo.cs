namespace GitCommands.Gpg
{
    public readonly record struct GpgKeyInfo
    {
        private readonly DateTimeOffset? _expires;

        public GpgKeyInfo(
            string fingerprint,
            string keyID,
            string userID,
            DateTimeOffset? expires,
            Capabilities capabilities,
            KeyValidity validity,
            bool disabled,
            bool isDefault = false) : this()
        {
            Fingerprint = fingerprint;
            KeyID = keyID;
            UserID = userID;
            Expires = expires;
            Capabilities = capabilities;
            Validity = validity;
            IsDefault = isDefault;
            Disabled = disabled;
        }

        public override string ToString() => $"{UserID} ({KeyID})";

        public Capabilities Capabilities { get; init; }
        public bool Disabled { get; init; }

        public DateTimeOffset? Expires { get => _expires; init => _expires = value?.ToLocalTime(); }

        public string Fingerprint { get; init; }

        public bool IsDefault { get; init; } = false;

        public string KeyID { get; init; }

        public string UserID { get; init; }

        public KeyValidity Validity { get; init; }
    }
}
