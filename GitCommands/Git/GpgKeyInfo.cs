namespace GitCommands.Gpg
{
    public class GpgKeyInfo : IEquatable<GpgKeyInfo>
    {
        public GpgKeyInfo(string fingerprint, string keyID, string userID, DateTimeOffset? expires, bool isDefault = false)
        {
            Fingerprint = fingerprint;
            KeyID = keyID;
            UserID = userID;
            Expires = expires;
            IsDefault = isDefault;
        }

        public string Fingerprint { get; private set; }
        public string KeyID { get; private set; }
        public string UserID { get; private set; }
        private DateTimeOffset? _expires;
        public DateTimeOffset? Expires
        {
            get => _expires;
            private set
            {
                _expires = value?.ToLocalTime();
            }
        }

        public bool IsDefault { get; private set; } = false;
        public override string ToString()
        {
            return $"{UserID} ({KeyID})";
        }

        public override bool Equals(object obj)
        {
            if (obj is GpgKeyInfo gpgKeyInfo)
            {
                return Equals(gpgKeyInfo);
            }

            return base.Equals(obj);
        }

        public static bool operator ==(GpgKeyInfo first, GpgKeyInfo second)
        {
            if ((object)first == null)
            {
                return (object)second == null;
            }

            return first.Equals(second);
        }

        public static bool operator !=(GpgKeyInfo first, GpgKeyInfo second)
        {
            return !(first == second);
        }

        public bool Equals(GpgKeyInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Fingerprint, other.Fingerprint)
                   && Equals(KeyID, other.KeyID)
                   && Equals(UserID, other.UserID);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 47;
                if (Fingerprint != null)
                {
                    hashCode = (hashCode * 53) ^ EqualityComparer<string>.Default.GetHashCode(Fingerprint);
                }

                if (KeyID != null)
                {
                    hashCode = (hashCode * 53) ^ EqualityComparer<string>.Default.GetHashCode(KeyID);
                }

                if (UserID != null)
                {
                    hashCode = (hashCode * 53) ^ EqualityComparer<string>.Default.GetHashCode(UserID);
                }

                return hashCode;
            }
        }
    }
}
