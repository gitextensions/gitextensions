namespace GitCommands
{
    public sealed class EnumViewSetting<T> : ViewSetting<T> where T : struct, Enum
    {
        private T? _value;

        public override T Value
        {
            get
            {
                _value ??= AppSettings.GetEnum(Name, DefaultValue);
                return _value.Value;
            }
            set
            {
                _value = value;
            }
        }

        public EnumViewSetting(string name, T defaultValue)
        : base(name, defaultValue)
        {
        }

        public override void Reload()
        {
            // The value will be loaded on next access
            _value = null;
        }

        public override void Save()
        {
            AppSettings.SetEnum(Name, Value);
        }
    }
}
