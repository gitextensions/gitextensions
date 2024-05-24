﻿using System.Runtime.InteropServices;

namespace GitExtUtils
{
    public static class ClipboardUtil
    {
        public static bool TrySetText(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            try
            {
                // Setting clipboard data can fail, as applications can lock the clipboard.
                // Such failures surface as ExternalException.
                // Here we use an approach that retries to set the data periodically as required,
                // and throws if it's unable to do so after a given number of attempts.
                //
                // See https://github.com/gitextensions/gitextensions/issues/4542

                Clipboard.SetDataObject(
                    text,
                    copy: true, // keep the data on the clipboard, even after Git Extensions exits
                    retryTimes: 5,
                    retryDelay: 100);

                return true;
            }
            catch (ExternalException)
            {
                // The clipboard is being used by another process
                return false;
            }
        }
    }
}
