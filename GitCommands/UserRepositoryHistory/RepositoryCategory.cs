using System.ComponentModel;

namespace GitCommands.UserRepositoryHistory
{
    public class RepositoryCategory
    {
        private BindingList<Repository> _repositories;

        public RepositoryCategory()
        {
            Repositories.AllowNew = true;
            Repositories.RaiseListChangedEvents = false;
            Repositories.AllowRemove = true;
        }

        public RepositoryCategory(string description)
        {
            Description = description;
            Repositories.AllowNew = true;
            Repositories.RaiseListChangedEvents = false;
            Repositories.AllowRemove = true;
        }

        public BindingList<Repository> Repositories
        {
            get => _repositories ?? (_repositories = new BindingList<Repository>());
            set => _repositories = value;
        }

        public string Description { get; set; }

        public virtual void SetIcon()
        {
        }

        public void RemoveRepository(Repository repository)
        {
            Repositories.Remove(repository);
        }

        public void AddRepository(Repository repo)
        {
            Repositories.Add(repo);
        }
    }
}
