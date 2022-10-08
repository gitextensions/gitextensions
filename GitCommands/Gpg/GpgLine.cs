namespace GitCommands.Gpg
{
    internal record struct GpgLine
    {
        /// <summary>
        /// Models the records from running gpg -K --with-colons.  See <see href="https://git.gnupg.org/cgi-bin/gitweb.cgi?p=gnupg.git;a=blob_plain;f=doc/DETAILS"/>
        /// </summary>
        /// <param name="lineNumber">Line row number</param>
        /// <param name="fields">The : split fields from the line</param>
        public GpgLine(int lineNumber, IEnumerable<Field> fields) : this()
        {
            LineNumber = lineNumber;
            Fields = fields;
            LineType = DetermineLineType(fields?.FirstOrDefault().Value ?? "");
            Validity = DetermineLineValidity(fields?.Skip(FieldIndex.Validity)?.FirstOrDefault().Value ?? "");
            Disabled = fields?.Skip(FieldIndex.KeyCapabilities)?.FirstOrDefault().Value?.Contains('D') ?? false;
            LineCapabilities = DetermineCapabilities(fields?.Skip(FieldIndex.KeyCapabilities)?.FirstOrDefault().Value ?? "");
            ExpirationDate = DettermineExpiration(fields?.Skip(FieldIndex.ExpirationDate)?.FirstOrDefault().Value ?? "");
        }

        /// <summary>
        /// Description of the fields: Field numbers 0 based in c# so field 1 is indexed at 0
        /// </summary>
        public sealed class FieldIndex
        {
            public const int ExpirationDate = 6;
            public const int KeyCapabilities = 11;
            public const int KeyID = 4;
            public const int RecordType = 0;
            public const int UserID = 9;
            public const int Validity = 1;
        }

        /*
        *** Field 1 - Type of record
        - pub :: Public key
        - crt :: X.509 certificate
        - crs :: X.509 certificate and private key available
        - sub :: Subkey (secondary key)
        - sec :: Secret key
        - ssb :: Secret subkey (secondary key)
        - uid :: User id
        - uat :: User attribute (same as user id except for field 10).
        - sig :: Signature
        - rev :: Revocation signature
        - rvs :: Revocation signature (standalone) [since 2.2.9]
        - fpr :: Fingerprint (fingerprint is in field 10)
        - fp2 :: SHA-256 fingerprint (fingerprint is in field 10)
        - pkd :: Public key data [*]
        - grp :: Keygrip
        - rvk :: Revocation key
        - tfs :: TOFU statistics [*]
        - tru :: Trust database information [*]
        - spk :: Signature subpacket [*]
        - cfg :: Configuration data [*]
        */
        private static LineType DetermineLineType(string lineTypeKey)
        {
            var result = lineTypeKey switch
            {
                "sec" => LineType.Key,
                "fpr" or "fp2" => LineType.Fingerprint,
                "uid" => LineType.User,
                _ => LineType.Other,
            };
            return result;
        }

        /*
        Records marked with an asterisk are described at [[*Special%20field%20formats][*Special fields]].

*** Field 2 - Validity

    This is a letter describing the computed validity of a key.
    Currently this is a single letter, but be prepared that additional
    information may follow in some future versions. Note that GnuPG <
    2.1 does not set this field for secret key listings.

    - o :: Unknown (this key is new to the system)
    - i :: The key is invalid (e.g. due to a missing self-signature)
    - d :: The key has been disabled
       (deprecated - use the 'D' in field 12 instead)
    - r :: The key has been revoked
    - e :: The key has expired
    - - :: Unknown validity (i.e. no value assigned)
    - q :: Undefined validity.  '-' and 'q' may safely be treated as
           the same value for most purposes
    - n :: The key is not valid
    - m :: The key is marginal valid.
    - f :: The key is fully valid
    - u :: The key is ultimately valid.  This often means that the
           secret key is available, but any key may be marked as
           ultimately valid.
    - w :: The key has a well known private part.
    - s :: The key has special validity.  This means that it might be
           self-signed and expected to be used in the STEED system.
        */

        private static KeyValidity DetermineLineValidity(string validityKey)
        {
            KeyValidity result = validityKey switch
            {
                "r" => KeyValidity.Revoked,
                "e" => KeyValidity.Expired,
                "o" or "-" or "q" => KeyValidity.Unknown,
                "i" or "n" => KeyValidity.Invalid,
                "m" or "f" or "u" or "w" or "s" => KeyValidity.Valid,
                _ => KeyValidity.Unknown
            };
            return result;
        }

        /*
    If the validity information is given for a UID or UAT record, it
    describes the validity calculated based on this user ID.  If given
for a key record it describes the validity taken from the best
    rated user ID.


    For X.509 certificates a 'u' is used for a trusted root
    certificate (i.e. for the trust anchor) and an 'f' for all other
    valid certificates.

In "sig" records, this field may have one of these values as first
    character:

- ! :: Signature is good.
- - :: Signature is bad.
- ? :: No public key to verify signature or public key is not usable.
- % :: Other error verifying a signature

More values may be added later.The field may also be empty if
gpg has been invoked in a non-checking mode (--list-sigs) or in a
fast checking mode.Since 2.2.7 '?' will also be printed by the
command --list-sigs if the key is not in the local keyring.

*** Field 3 - Key length

    The length of key in bits.

*** Field 4 - Public key algorithm

    The values here are those from the OpenPGP specs or if they are
    greater than 255 the algorithm ids as used by Libgcrypt.

*** Field 5 - KeyID

    This is the 64 bit keyid as specified by OpenPGP and the last 64
    bit of the SHA-1 fingerprint of an X.509 certifciate.

*** Field 6 - Creation date

    The creation date of the key is given in UTC.For UID and UAT
    records, this is used for the self-signature date.  Note that the
    date is usually printed in seconds since epoch, however, we are
    migrating to an ISO 8601 format (e.g. "19660205T091500").  This is
    currently only relevant for X.509.  A simple way to detect the new
    format is to scan for the 'T'.  Note that old versions of gpg
    without using the =--fixed-list - mode = option used a "yyyy-mm-tt"
    format.

* **Field 7 - Expiration date

    Key or UID / UAT expiration date or empty if it does not expire.
         */
        #region Expiration
        private static DateTimeOffset? DettermineExpiration(string expired)
        {
            return string.IsNullOrEmpty(expired)
                ? null
                : expired.Contains('T')
                    ? DateTimeOffset.ParseExact(expired, "yyyyMMdd'T'HHmmss", null, System.Globalization.DateTimeStyles.AssumeUniversal) // ISO 8601 UTC format expected. In case they use the same format in creation date
                    : DateTimeOffset.FromUnixTimeSeconds(long.TryParse(expired, out long tmp) ? tmp : 0L); // Convert from epoch to DatetimeOffset
        }
        #endregion

        /*

* **Field 8 - Certificate S / N, UID hash, trust signature info

    Used for serial number in crt records.For UID and UAT records,
    this is a hash of the user ID contents used to represent that
    exact user ID.For trust signatures, this is the trust depth
    separated by the trust value by a space.

* **Field 9 - Ownertrust

    This is only used on primary keys.This is a single letter, but
    be prepared that additional information may follow in future
    versions.For trust signatures with a regular expression, this is
    the regular expression value, quoted as in field 10.

* **Field 10 - User - ID

    The value is quoted like a C string to avoid control characters
    (the colon is quoted =\x3a =).For a "pub" record this field is
    not used on --fixed-list - mode.A UAT record puts the attribute
    subpacket count here, a space, and then the total attribute
    subpacket size.In gpgsm the issuer name comes here.The FPR and FP2
    records store the fingerprints here.The fingerprint of a
    revocation key is stored here.

***Field 11 - Signature class

    Signature class as per RFC-4880.  This is a 2 digit hexnumber
    followed by either the letter 'x' for an exportable signature or
    the letter 'l' for a local-only signature.The class byte of an
    revocation key is also given here, by a 2 digit hexnumber and
    optionally followed by the letter 's' for the "sensitive"
    flag.This field is not used for X.509.

    "rev" and "rvs" may be followed by a comma and a 2 digit hexnumber
    with the revocation reason.
        */

        /*
*** Field 12 - Key capabilities

    The defined capabilities are:

    - e::Encrypt
    - s::Sign
    - c::Certify
    - a::Authentication
    - r::Restricted encryption (subkey only use)
    - t::Timestamping
    - g::Group key
    - ? ::Unknown capability

    A key may have any combination of them in any order.In addition
    to these letters, the primary key has uppercase versions of the
    letters to denote the _usable_ capabilities of the entire key, and
    a potential letter 'D' to indicate a disabled key.
        */
        private static Capabilities DetermineCapabilities(string capabilities)
        {
            Capabilities result = Capabilities.None;

            result = capabilities?.Contains('E', StringComparison.CurrentCultureIgnoreCase) ?? false ? result | Capabilities.Encrypt : result;
            result = capabilities?.Contains('S', StringComparison.CurrentCultureIgnoreCase) ?? false ? result | Capabilities.Sign : result;
            result = capabilities?.Contains('C', StringComparison.CurrentCultureIgnoreCase) ?? false ? result | Capabilities.Certify : result;
            result = capabilities?.Contains('A', StringComparison.CurrentCultureIgnoreCase) ?? false ? result | Capabilities.Authentication : result;
            result = capabilities?.Contains('R', StringComparison.CurrentCultureIgnoreCase) ?? false ? result | Capabilities.RestrictedEncryption : result;
            result = capabilities?.Contains('T', StringComparison.CurrentCultureIgnoreCase) ?? false ? result | Capabilities.Timestamping : result;
            result = capabilities?.Contains('G', StringComparison.CurrentCultureIgnoreCase) ?? false ? result | Capabilities.GroupKey : result;

            return result;
        }

        /*

*** Field 13 - Issuer certificate fingerprint or other info

    Used in FPR records for S/MIME keys to store the fingerprint of
    the issuer certificate.  This is useful to build the certificate
    path based on certificates stored in the local key database it is
    only filled if the issuer certificate is available.The root has
    been reached if this is the same string as the fingerprint. The
    advantage of using this value is that it is guaranteed to have
    been built by the same lookup algorithm as gpgsm uses.

    For "uid" records this field lists the preferences in the same way
    gpg's --edit-key menu does.

    For "sig", "rev" and "rvs" records, this is the fingerprint of the
    key that issued the signature.Note that this may only be filled
    if the signature verified correctly.  Note also that for various
    technical reasons, this fingerprint is only available if
    --no-sig-cache is used.Since 2.2.7 this field will also be set
    if the key is missing but the signature carries an issuer
    fingerprint as meta data.

*** Field 14 - Flag field

    Flag field used in the --edit-key menu output

*** Field 15 - S/N of a token

    Used in sec/ssb to print the serial number of a token (internal
    protect mode 1002) or a '#' if that key is a simple stub(internal
    protect mode 1001).  If the option --with-secret is used and a
    secret key is available for the public key, a '+' indicates this.

*** Field 16 - Hash algorithm

    For sig records, this is the used hash algorithm.  For example:
    2 = SHA-1, 8 = SHA-256.

*** Field 17 - Curve name

    For pub, sub, sec, ssb, crt, and crs records this field is used
    for the ECC curve name.

*** Field 18 - Compliance flags

    Space separated list of asserted compliance modes and
    screening result for this key.

    Valid values are:

    - 8  :: The key is compliant with RFC4880bis
    - 23 :: The key is compliant with compliance mode "de-vs".
    - 6001 :: Screening hit on the ROCA vulnerability.

*** Field 19 - Last update

    The timestamp of the last update of a key or user ID.  The update
    time of a key is defined a lookup of the key via its unique
    identifier (fingerprint); the field is empty if not known.  The
    update time of a user ID is defined by a lookup of the key using a
    trusted mapping from mail address to key.

*** Field 20 - Origin

    The origin of the key or the user ID.This is an integer
    optionally followed by a space and an URL.  This goes along with
    the previous field.The URL is quoted in C style.

*** Field 21 - Comment

    This is currently only used in "rev" and "rvs" records to carry
    the the comment field of the recocation reason.The value is
    quoted in C style.
 */

        public bool Disabled { get; private set; }

        public readonly record struct Field(int Index, string Value);
        public DateTimeOffset? ExpirationDate { get; private set; }
        public IEnumerable<Field> Fields { get; private set; }
        public Capabilities LineCapabilities { get; private set; }
        public int LineNumber { get; private set; }
        public LineType LineType { get; private set; }
        public KeyValidity Validity { get; private set; }
    }
}
