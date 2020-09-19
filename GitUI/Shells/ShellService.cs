#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace GitUI.Shells
{
    public interface IShellService
    {
        void Update(IEnumerable<IShell> shels);

        IEnumerable<IShell> AllShells();

        IEnumerable<IShell> EnabledShells();

        IShell? FindDefault();
    }

    internal sealed class ShellService : IShellService
    {
        private readonly IShellRepository _shellRepository;

        public ShellService(IShellRepository shellRepository)
        {
            _shellRepository = shellRepository;
        }

        public void Update(IEnumerable<IShell> shels)
        {
            var shellsMap = shels.ToDictionary(x => x.Id, x => x);
            var forRemoveShells = _shellRepository
                .Query()
                .Where(x => !shels.Any(y => y.Id == x.Id))
                .ToList();

            foreach (var forRemoveShell in forRemoveShells)
            {
                _shellRepository.Remove(forRemoveShell);
            }

            var forUpdateShells = _shellRepository
                .Query()
                .Where(x => shels.Any(y => y.Id == x.Id));

            foreach (var forUpdateShell in forUpdateShells)
            {
                var newShell = shellsMap[forUpdateShell.Id];

                shellsMap.Remove(forUpdateShell.Id);

                forUpdateShell.Icon = newShell.Icon;
                forUpdateShell.Name = newShell.Name;
                forUpdateShell.Command = newShell.Command;
                forUpdateShell.Arguments = newShell.Arguments;
                forUpdateShell.Default = newShell.Default;
                forUpdateShell.Enabled = newShell.Enabled;
            }

            foreach ((Guid id, IShell newShell) in shellsMap)
            {
                var shell = new Shell
                {
                    Id = id,
                    Name = newShell.Name,
                    Icon = newShell.Icon,
                    Command = newShell.Command,
                    Arguments = newShell.Arguments,
                    Default = newShell.Default,
                    Enabled = newShell.Enabled
                };

                _shellRepository.Add(shell);
            }

            _shellRepository.Save();
        }

        public IEnumerable<IShell> AllShells()
        {
            return _shellRepository
                .Query();
        }

        public IEnumerable<IShell> EnabledShells()
        {
            return _shellRepository
                .Query()
                .Where(x => x.Enabled);
        }

        public IShell? FindDefault()
        {
            return _shellRepository
                .Query()
                .FirstOrDefault(x => x.Default);
        }
    }
}
