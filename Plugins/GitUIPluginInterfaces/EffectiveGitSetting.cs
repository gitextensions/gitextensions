namespace GitUIPluginInterfaces
{
    public record struct EffectiveGitSetting(GitConfigStatus? Status, string Value)
    {
        public static implicit operator (GitConfigStatus? status, string value)(EffectiveGitSetting value)
        {
            return (value.Status, value.Value);
        }

        public static implicit operator EffectiveGitSetting((GitConfigStatus? status, string value) value)
        {
            return new EffectiveGitSetting(value.status, value.value);
        }
    }
}
