namespace GitUI.LeftPanel.Interfaces;

public interface ICanToggleDeletionProtection
{
    bool IsDeleteProtected { get; }
    bool ProtectFromDeletion();
    bool UnprotectFromDeletion();
}
