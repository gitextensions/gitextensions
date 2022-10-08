namespace GitCommands.Gpg
{
    public interface IGPGSecretKeysParser
    {
        /// <summary>
        /// Parses gpg -K --with-colons --keyid-format LONG output
        /// </summary>
        /// <returns>gpg key info</returns>
        IEnumerable<GpgKeyInfo> ParseKeysOutput(string txt, string defaultKey);
    }

    public class GpgSecretKeysParser : IGPGSecretKeysParser
    {
        /// <inherit/>
        public IEnumerable<GpgKeyInfo> ParseKeysOutput(string txt, string? defaultKey)
        {
            try
            {
                GpgLine[] rec = txt.Split('\n').
                    Select(ln => ln.Split(':')).
                    Select((f, i) => new GpgLine(
                        i,
                        f.Select((d, i2) => new GpgLine.Field(i2, d)))).
                    Where(ln => ln.LineType != LineType.Other).
                    ToArray();

                IEnumerable<GpgLine> keys = (from r in rec
                                             where r.LineType == LineType.Key
                                             select r).DistinctBy(k => k.Fields.ElementAt(GpgLine.FieldIndex.KeyID).Value);

                IEnumerable<GpgLine> fingerprints = from r in rec
                                                    where r.LineType == LineType.Fingerprint
                                                    select r;

                IEnumerable<GpgLine> users = from r in rec
                                             where r.LineType == LineType.User
                                             select r;

                // We don't care about subkeys and fingerprints.  We just want main keys and their related items.
                var combined = from k in keys
                               let f = fingerprints.First(fr => fr.LineNumber > k.LineNumber && fr.Fields.ElementAt(GpgLine.FieldIndex.UserID).Value.EndsWith(k.Fields.ElementAt(GpgLine.FieldIndex.KeyID).Value))
                               let u = users.First(ur => ur.LineNumber > k.LineNumber)
                               select new { Key = k, Fingerprint = f, User = u };

                IEnumerable<GpgKeyInfo> output = combined.ToArray().Select((k, i) => new GpgKeyInfo(
                    k.Fingerprint.Fields.ElementAt(GpgLine.FieldIndex.UserID).Value,
                    k.Key.Fields.ElementAt(GpgLine.FieldIndex.KeyID).Value,
                    k.User.Fields.ElementAt(GpgLine.FieldIndex.UserID).Value,
                    k.Key.ExpirationDate,
                    k.Key.LineCapabilities,
                    k.Key.Validity,
                    k.Key.Disabled,
                    !string.IsNullOrEmpty(defaultKey) && k.Key.Fields.ElementAt(GpgLine.FieldIndex.KeyID).Value == defaultKey));

                return output.ToArray();
            }
            catch
            {
                return Array.Empty<GpgKeyInfo>();
            }
        }
    }
}
