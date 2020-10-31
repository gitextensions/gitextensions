using System;
using System.Drawing;
using JetBrains.Annotations;

namespace GitExtensions.Core.Settings
{
    public interface ISettingsSource
    {
        SettingLevel SettingLevel { get; set; }

        T GetValue<T>([NotNull] string name, T defaultValue, [NotNull] Func<string, T> decode);

        void SetValue<T>([NotNull] string name, T value, [NotNull] Func<T, string> encode);

        bool? GetBool([NotNull] string name);

        bool GetBool([NotNull] string name, bool defaultValue);

        void SetBool([NotNull] string name, bool? value);

        void SetInt([NotNull] string name, int? value);

        int? GetInt([NotNull] string name);

        void SetFloat([NotNull] string name, float? value);

        float? GetFloat([NotNull] string name);

        DateTime GetDate([NotNull] string name, DateTime defaultValue);

        void SetDate([NotNull] string name, DateTime? value);

        DateTime? GetDate([NotNull] string name);

        int GetInt([NotNull] string name, int defaultValue);

        void SetFont([NotNull] string name, Font value);

        Font GetFont([NotNull] string name, Font defaultValue);

        Color GetColor([NotNull] string name, Color defaultValue);

        void SetEnum<T>([NotNull] string name, T value);

        T GetEnum<T>([NotNull] string name, T defaultValue) where T : struct, Enum;

        void SetNullableEnum<T>([NotNull] string name, T? value) where T : struct, Enum;

        T? GetNullableEnum<T>([NotNull] string name) where T : struct;

        void SetString([NotNull] string name, [CanBeNull] string value);

        string GetString([NotNull] string name, string defaultValue);
    }
}
