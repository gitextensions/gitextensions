namespace GitCommands.Gpg
{
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
    public enum LineType
    {
        Key,
        Fingerprint,
        User,
        Other
    }
}
