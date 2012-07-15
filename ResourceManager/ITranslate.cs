using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Translation
{
    public interface ITranslate
    {
        void AddTranslationItems(Translation translation);

        void TranslateItems(Translation translation);
    }
}
