using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace GitCommands
{
    public class RepositoryCategory
    {
        public RepositoryCategory()
        {
            Repositories.AllowNew = true;
        }


        public RepositoryCategory(string description)
        {
            this.Description = description;
        }

        public BindingList<Repository> Repositories = new BindingList<Repository>();

        public string Description { get; set; }

        public void AddRepository(Repository repo)
        {
            Repositories.Add(repo);
        }
    }
}
