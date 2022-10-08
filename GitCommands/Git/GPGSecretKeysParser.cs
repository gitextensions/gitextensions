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

    public class GPGSecretKeysParser : IGPGSecretKeysParser
    {
        /// <inherit/>
        public IEnumerable<GpgKeyInfo> ParseKeysOutput(string txt, string? defaultKey)
        {
            try
            {
                var rec = txt.Split('\n').
                    Select(ln => ln.Split(':')).
                    Select((f, i) => new GPGLine(
                        i,
                        f.Select((d, i2) => new GPGLine.Field(i2, d)))).
                    Where(ln => ln.LineType != GPGLine.LineTypes.Other).
                    ToArray();

                var keys = (from r in rec
                            where r.LineType == GPGLine.LineTypes.Key
                            select r).DistinctBy(k => k.Fields.ElementAt(4).Value);

                var fingerprints = from r in rec
                                   where r.LineType == GPGLine.LineTypes.Fingerprint
                                   select r;

                var users = from r in rec
                            where r.Fields.First().Value == "uid"
                            select r;

                // We don't care about subkeys and fingerprints.  We just want main keys and their related items.
                var combined = (from k in keys
                                let f = fingerprints.First(fr => fr.LineNumber > k.LineNumber && fr.Fields.ElementAt(9).Value.EndsWith(k.Fields.ElementAt(4).Value))
                                let u = users.First(ur => ur.LineNumber > k.LineNumber)
                                select new { Key = k, Fingerprint = f, User = u }).ToArray();
                long tmp = 0;

                var output = combined.Select((k, i) => new GpgKeyInfo(
                    k.Fingerprint.Fields.ElementAt(9).Value,
                    k.Key.Fields.ElementAt(4).Value,
                    k.User.Fields.ElementAt(9).Value,
                    string.IsNullOrEmpty(k.Key.Fields.ElementAt(6).Value) ? null : DateTimeOffset.FromUnixTimeSeconds(long.TryParse(k.Key.Fields.ElementAt(6).Value, out tmp) ? tmp : 0L),
                    !string.IsNullOrEmpty(defaultKey) && k.Key.Fields.ElementAt(4).Value == defaultKey));

                return output.ToArray();
            }
            catch
            {
                return Enumerable.Empty<GpgKeyInfo>();
            }
        }
    }
}
