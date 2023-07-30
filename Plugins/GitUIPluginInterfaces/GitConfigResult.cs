namespace GitUIPluginInterfaces
{
    public readonly record struct GitConfigGetResult(GitConfigStatus? Status, string Name, string Value, DateTimeOffset Retrieved, string Scope, string File, string ErrorMessage)
    {
        public static implicit operator (GitConfigStatus? status, string name, string value, DateTimeOffset retrieved, string scope, string file, string errorMessage)(GitConfigGetResult value) =>
            (value.Status, value.Name, value.Value, value.Retrieved, value.Scope, value.File, value.ErrorMessage);

        public static implicit operator GitConfigGetResult((GitConfigStatus? status, string name, string value, DateTimeOffset retrieved, string scope, string file, string error) value) =>
            new(value.status, value.name, value.value, value.retrieved, value.scope, value.file, value.error);
    }
}
