using System;

namespace GitUIPluginInterfaces
{
    public readonly struct Remote
    {
        public string Name { get; }
        public string PushUrl { get; }
        public string FetchUrl { get; }

        public Remote(string name, string pushUrl, string fetchUrl)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PushUrl = pushUrl ?? throw new ArgumentNullException(nameof(pushUrl));
            FetchUrl = fetchUrl ?? throw new ArgumentNullException(nameof(fetchUrl));
        }
    }
}