namespace GitCommands.Gpg
{
    /*
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
           self-signed and expected to be used in the STEED system.*/
    public enum KeyValidity
    {
        /*
         NOT providing all distinct statuses.  We are interested in Expired, Revoked, Invalid, and Unknown.
         Other statuses do not mean the keys are unable to be used.
        */
        Unknown,
        Invalid,
        Revoked,
        Expired,
        Valid
    }
}
