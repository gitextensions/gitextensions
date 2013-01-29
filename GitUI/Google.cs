using System;
using System.Net;

namespace GitUI
{
    /// <summary>Provides methods from Google services.</summary>
    public static class Google
    {
        /// <summary>
        /// Translate text using Google Translate API's
        /// URL - http://www.google.com/translate_t?hl=en&amp;ie=UTF8&amp;text={0}&amp;langpair={1}
        /// </summary>
        /// <param name="text">Source text to translate from.</param>
        /// <param name="translateFrom">ISO 639-1 two-letter code for the language of the <paramref name="text"/>.</param>
        /// <param name="translateTo">ISO 639-1 two-letter code for the language to translate to.</param>
        /// <returns>Translated string.</returns>
        public static string TranslateText(
            string text,
            string translateFrom,
            string translateTo)
        {
            const string url = "http://ajax.googleapis.com/ajax/services/language/translate";
            var webClient = new WebClient { Proxy = WebRequest.DefaultWebProxy, Encoding = System.Text.Encoding.UTF8 };

            webClient.QueryString.Add("v", "1.0");
            webClient.QueryString.Add("q", Uri.EscapeDataString(text));
            webClient.QueryString.Add("langpair", string.Format("{0}|{1}", translateFrom, translateTo));
            webClient.QueryString.Add("ie", "UTF8");
            webClient.QueryString.Add("key", "ABQIAAAAL-jmAvZrZhQkLeK6o_JtUhSHPdD4FWU0q3SlSmtsnuxmaaTWWhRV86w05sbgIY6R6F3MqsVyCi0-Kg");
            webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
            string result = webClient.DownloadString(url);

            const string startString = "{\"translatedText\":\"";
            const string detectedSourceLanguageString = "\",\"detectedSourceLanguage\":";
            const string endString = "\"}";

            int startOffset = result.IndexOf(startString) + startString.Length;
            int length = result.IndexOf(detectedSourceLanguageString, startOffset) - startOffset;
            if (length < 0)
                length = result.IndexOf(endString, startOffset) - startOffset;

            if (length <= 0)
                return text;

            result = result.Substring(startOffset, length);

            //Hack to decode '&' from unicode...
            result = result.Replace("\\u0022", "\u0022")
                           .Replace("\\u0023", "\u0023")
                           .Replace("\\u0024", "\u0024")
                           .Replace("\\u0025", "\u0025")
                           .Replace("\\u0026", "\u0026")
                           .Replace("\\u0027", "\u0027")
                           .Replace("\\u0028", "\u0028")
                           .Replace("\\u0029", "\u0029");

            return result;
        }
    }
}
