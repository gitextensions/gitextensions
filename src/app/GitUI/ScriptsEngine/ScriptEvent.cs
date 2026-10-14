namespace GitUI.ScriptsEngine;

public enum ScriptEvent
{
    None,
    BeforeCommit,
    AfterCommit,
    BeforePull,
    AfterPull,
    BeforePush,
    AfterPush,
    BeforeCheckout,
    AfterCheckout,
    BeforeMerge,
    AfterMerge,
    BeforeFetch,
    AfterFetch,
    ShowInFileList
}
