using GitUI;

namespace GitCommands.Services
{
    public sealed class WinFormsMessageBoxService : IMessageBoxService
    {
        private Control? _owner;

        public WinFormsMessageBoxService(Control? owner = null)
        {
            _owner = owner;
        }

        public async Task ShowInfoMessageAsync(string? text, string? caption)
        {
            _owner ??= Form.ActiveForm;

            if (_owner is not null)
            {
                if (_owner.InvokeRequired)
                {
                    await _owner.SwitchToMainThreadAsync();
                }

                MessageBox.Show(
                    _owner,
                    text ?? string.Empty,
                    caption ?? string.Empty,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation
                );
            }
            else
            {
                MessageBox.Show(
                    text ?? string.Empty,
                    caption ?? string.Empty,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation
                );
            }
        }
    }
}
