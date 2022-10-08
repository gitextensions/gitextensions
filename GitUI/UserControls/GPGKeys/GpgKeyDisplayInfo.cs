using ResourceManager;

namespace GitUI.UserControls.GPGKeys
{
    public class GpgKeyDisplayInfo : IEquatable<GpgKeyDisplayInfo>
    {
        internal class Translations : Translate
        {
            internal readonly TranslationString _noKeySelected = new("No key selected");
        }

        public GpgKeyDisplayInfo(string fingerprint, string keyID, string userID, bool isDefault)
        {
            Fingerprint = fingerprint;
            KeyID = keyID;
            UserID = userID;
            IsDefault = isDefault;
        }

        private static Translations? instance;
        private static Translations Instance => instance ??= new();
        public string Fingerprint { get; set; }

        public string KeyID { get; set; }

        public string UserID { get; set; }

        public bool IsDefault { get; set; }

        public string Caption { get => string.IsNullOrEmpty(UserID) ? Instance._noKeySelected.Text : $"{(IsDefault ? "*" : "")} ({KeyID}) {UserID}"; }

        public override string ToString() => Caption;

        public override bool Equals(object obj)
        {
            if (obj is GpgKeyDisplayInfo gpgKeyDisplayInfo)
            {
                return Equals(gpgKeyDisplayInfo);
            }

            return base.Equals(obj);
        }

        public static bool operator ==(GpgKeyDisplayInfo first, GpgKeyDisplayInfo second)
        {
            if ((object)first == null)
            {
                return (object)second == null;
            }

            return first.Equals(second);
        }

        public static bool operator !=(GpgKeyDisplayInfo first, GpgKeyDisplayInfo second)
        {
            return !(first == second);
        }

        public bool Equals(GpgKeyDisplayInfo other)
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
                    hashCode = hashCode * 53 ^ EqualityComparer<string>.Default.GetHashCode(Fingerprint);
                }

                if (KeyID != null)
                {
                    hashCode = hashCode * 53 ^ EqualityComparer<string>.Default.GetHashCode(KeyID);
                }

                if (UserID != null)
                {
                    hashCode = hashCode * 53 ^ EqualityComparer<string>.Default.GetHashCode(UserID);
                }

                return hashCode;
            }
        }
    }
}
