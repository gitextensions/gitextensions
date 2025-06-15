using StrongOf;

namespace GitUI;

partial class FileStatusList
{
    public sealed class GroupKey(string value) : StrongString<GroupKey>(value)
    {
    }
}
