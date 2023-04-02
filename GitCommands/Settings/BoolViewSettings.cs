namespace GitCommands
{
    public sealed class BoolViewSetting : ViewSetting<bool>
    {
        private bool? _value;

        public override bool Value
        {
            get
            {
                _value ??= AppSettings.GetBool(Name, DefaultValue);
                return _value.Value;
            }
            set
            {
                _value = value;
            }
        }

        public BoolViewSetting(string name, bool defaultValue)
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
            AppSettings.SetBool(Name, Value);
        }

        public void Toggle()
        {
            Value = !Value;
        }
    }
}
