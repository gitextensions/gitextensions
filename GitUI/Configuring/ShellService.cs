using System;
using System.Collections.Generic;
using System.Linq;

namespace GitUI.Configuring
{
    public interface IShellService
    {
        void Default(string name);

        IEnumerable<IShell> List();

        IShell GetDefault();
    }

    internal sealed class ShellService : IShellService
    {
        private readonly IShellRepository _shellRepository = new ShellRepository();

        public void Default(string name)
        {
            var defaultShell = _shellRepository
                .FindByDefault();

            if (defaultShell == null)
            {
                throw new InvalidOperationException("Default Shell must be.");
            }

            defaultShell.Default = false;

            var shell = _shellRepository
                .FindByName(name);

            if (shell == null)
            {
                throw new InvalidOperationException($"Unknown Shell: {name}.");
            }

            shell.Default = true;

            _shellRepository.Save();
        }

        public IEnumerable<IShell> List()
        {
            return _shellRepository
                .Query();
        }

        public IShell GetDefault()
        {
            var defaultShell = _shellRepository
                .FindByDefault();

            if (defaultShell == null)
            {
                throw new InvalidOperationException("Default Shell must be.");
            }

            return defaultShell;
        }
    }
}
