using System.Collections.Generic;
using System.Drawing;

namespace GitExtUtils.GitUI.Theming
{
    public interface IThemeSerializationFields
    {
        IReadOnlyDictionary<AppColor, Color> AppColorValues { get; }
        IReadOnlyDictionary<KnownColor, Color> SysColorValues { get; }
    }
}
