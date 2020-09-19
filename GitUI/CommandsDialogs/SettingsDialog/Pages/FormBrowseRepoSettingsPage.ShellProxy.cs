#nullable enable

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using GitUI.Design;
using GitUI.Shells;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class FormBrowseRepoSettingsPage
    {
        [TypeConverter(typeof(PropertySorter))]
        private sealed class ShellProxy : IShell, INotifyPropertyChanged, ICheckable
        {
            private const string ShellCategory = "Shell";

            public static ShellProxy Create(IShell shell)
            {
                return new ShellProxy
                {
                    Id = shell.Id,
                    Name = shell.Name,
                    Enabled = shell.Enabled,
                    Command = shell.Command,
                    Arguments = shell.Arguments,
                    Default = shell.Default,
                    Icon = shell.Icon
                };
            }

            private string _name;
            private bool _enabled;
            private string _command;
            private string _arguments;
            private bool _default;
            private string _icon;

            public event PropertyChangedEventHandler? PropertyChanged;

            public ShellProxy()
            {
                Id = Guid.NewGuid();
                _name = ShellConstants.DefaultName;
                _icon = ShellConstants.DefaultIcon;
                _command = ShellConstants.DefaultCommand;
                _arguments = ShellConstants.DefaultArguments;
                _default = false;
                _enabled = true;
            }

            [Browsable(false)]
            public Guid Id { get; private set; }

            [Category(ShellCategory)]
            [PropertyOrder(0)]
            public string Name
            {
                get => _name;

                set
                {
                    if (_name == value)
                    {
                        return;
                    }

                    _name = value;

                    NotifyPropertyChanged();
                }
            }

            [Category(ShellCategory)]
            [PropertyOrder(1)]
            public bool Enabled
            {
                get => _enabled;

                set
                {
                    if (_enabled == value)
                    {
                        return;
                    }

                    _enabled = value;

                    NotifyPropertyChanged();

                    if (!_enabled)
                    {
                        Default = false;
                    }
                }
            }

            [Category(ShellCategory)]
            [PropertyOrder(2)]
            [Editor(typeof(ShellsExecutableFileNameEditor), typeof(UITypeEditor))]
            public string Command
            {
                get => _command;

                set
                {
                    if (_command == value)
                    {
                        return;
                    }

                    _command = value;

                    NotifyPropertyChanged();
                }
            }

            [Category(ShellCategory)]
            [PropertyOrder(3)]
            [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
            public string Arguments
            {
                get => _arguments;

                set
                {
                    if (_arguments == value)
                    {
                        return;
                    }

                    _arguments = value;

                    NotifyPropertyChanged();
                }
            }

            [Category(ShellCategory)]
            [PropertyOrder(4)]
            public bool Default
            {
                get => _default;

                set
                {
                    if (_default == value)
                    {
                        return;
                    }

                    _default = value;

                    NotifyPropertyChanged();

                    if (_default)
                    {
                        Enabled = value;
                    }
                }
            }

            [Category(ShellCategory)]
            [PropertyOrder(5)]
            [Editor(typeof(ResourceImagesTypeEditor), typeof(UITypeEditor))]
            [TypeConverter(typeof(ResourceImagesTypeConverter))]
            public string Icon
            {
                get => _icon;

                set
                {
                    if (_icon == value)
                    {
                        return;
                    }

                    _icon = value;

                    NotifyPropertyChanged();
                }
            }

            bool ICheckable.Checked
            {
                get => Default;
                set => Default = value;
            }

            private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
