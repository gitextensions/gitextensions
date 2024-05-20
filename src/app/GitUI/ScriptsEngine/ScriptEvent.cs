namespace GitUI.ScriptsEngine
{
    public enum ScriptEvent
    {
        None,
        BeforeCommit,
        AfterCommit,
        BeforePull,
        AfterPull,
        BeforePush,
        AfterPush,
        ShowInUserMenuBar,
        BeforeCheckout,
        AfterCheckout,
        BeforeMerge,
        AfterMerge,
        BeforeFetch,
        AfterFetch,
        ShowInFileList
    }
}
