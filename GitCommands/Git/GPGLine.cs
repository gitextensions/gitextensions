namespace GitCommands.Gpg
{
    internal sealed class GPGLine // : IEquatable<GPGLine>
    {
        public enum LineTypes
        {
            Key,
            Fingerprint,
            User,
            Other
        }

        public GPGLine(int lineNumber, IEnumerable<Field> fields)
        {
            LineNumber = lineNumber;
            Fields = fields;
            LineType = DetermineLineType(fields.First().Value);
        }

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        public readonly record struct Field(int Index, string Value);
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter

        public int LineNumber { get; private set; }
        public IEnumerable<Field> Fields { get; private set; }

        public LineTypes LineType { get; private set; }

        private static LineTypes DetermineLineType(string lineTypeKey)
        {
            LineTypes result = LineTypes.Other;
            switch (lineTypeKey)
            {
                case "sec":
                    result = LineTypes.Key;
                    break;
                case "fpr":
                    result = LineTypes.Fingerprint;
                    break;
                case "uid":
                    result = LineTypes.User;
                    break;
            }

            return result;
        }
    }
}
