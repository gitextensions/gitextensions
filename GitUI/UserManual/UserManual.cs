namespace GitUI.UserManual
{
    public static class UserManual
    {
        public static string UrlFor(string subFolder, string anchorName)
        {
            return For(subFolder, anchorName).GetUrl();
        }

        private static IProvideUserManual For(string subFolder, string anchorName)
        {
            return new StandardHtmlUserManual(subFolder, anchorName);

            // or local singlehtml help / TODO: put manual to GitExt setup
            // return new SingleHtmlUserManual(anchorName);
        }
    }
}