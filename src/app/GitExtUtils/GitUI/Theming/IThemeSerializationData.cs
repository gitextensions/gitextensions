﻿namespace GitExtUtils.GitUI.Theming
{
    public interface IThemeSerializationData
    {
        IReadOnlyDictionary<AppColor, Color> AppColorValues { get; }
        IReadOnlyDictionary<KnownColor, Color> SysColorValues { get; }
    }
}
