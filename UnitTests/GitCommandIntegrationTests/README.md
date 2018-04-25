# Goal

Provide reliable test data and automated testing facilities for GitExtensions features which depend on the ouptut of an external git command

# Future

- select an initial set of features which are worth being autmatically tested
    - input needed on this topic
- design and create one or more test repositories which are either
    - persistent enough
        - so many tests can depend on a smaller set of test repositories
    - small enough
        - so many independent test data may be created without affecting maintainability of the GitExtensions repository
        - no blobs at all if possible, or only very small ones
        - clean up before adding (see https://stackoverflow.com/a/2116892, https://stackoverflow.com/a/14729486)
    - balance size and persistence
        - exhibiting relevant combinations of some selected git features
    - packaged independently of the GitExtensions repository via a delivery mechanism
- select supported git versions
    - download selected git versions via a delivery mechanism
        - use locally installed git (find on path)
        - specify a custom git instance (via environment variable or test runner configuration)
        - NuGet: https://www.nuget.org/packages/GitForWindows/
        - Choco: https://chocolatey.org/packages/git
        - ???
- examine if there are other factors which may have effect on the execution
    - regional settings, non-English locale
    - output containing absolute paths or other values that may differ per environment or per execution
    - output returned in non-predictable order
    - ???
- exercise selected git commands using supported git versions over the selected repository(ies)
    - keep in mind that some commands may modify the repository and must be run on a copy
