using System;
using System.Diagnostics;
using GitCommands;
using JetBrains.Annotations;

namespace GitUIPluginInterfaces
{
    [DebuggerDisplay("{" + nameof(Arguments) + "}")]
    public readonly struct ArgumentString
    {
        [CanBeNull] public string Arguments { get; }

        private ArgumentString([NotNull] string arguments)
        {
            Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }

        public static implicit operator ArgumentString(string args) => new ArgumentString(args);
        public static implicit operator ArgumentString(ArgumentBuilder args) => new ArgumentString(args.ToString());
        [CanBeNull]
        public static implicit operator string(ArgumentString args) => args.Arguments;
        public override string ToString() => Arguments ?? "";
    }
}